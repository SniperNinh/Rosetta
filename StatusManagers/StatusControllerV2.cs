using daisyowl.text;
using HarmonyLib;
using Nickel;
using Rosseta.External;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rosseta.StatusManagers;

internal sealed class StatusControllerV2
{
    internal static IStatusEntry ManaStatus { get; private set; } = null!;
    internal static IStatusEntry ManaMaxStatus { get; private set; } = null!;
    internal static IStatusEntry ManaSpillStatus { get; private set; } = null!;
    internal static IStatusEntry StirStatus { get; private set; } = null!;

    private static bool IsDuringNormalDamage;
    private static int ReducedDamage;

    public StatusControllerV2()
    {
        ManaStatus = ModEntry.Instance.Helper.Content.Statuses.RegisterStatus("Mana", new()
        {
            Definition = new()
            {
                icon = ModEntry.Instance.Helper.Content.Sprites.RegisterSprite(ModEntry.Instance.Package.PackageRoot.GetRelativeFile("assets/Status/Veiling.png")).Sprite,
                color = new("A17FFF"),
                isGood = true,
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["status", "Mana", "name"]).Localize,
            Description = ModEntry.Instance.AnyLocalizations.Bind(["status", "Mana", "description"]).Localize
        });

        ManaMaxStatus = ModEntry.Instance.Helper.Content.Statuses.RegisterStatus("ManaMax", new()
        {
            Definition = new()
            {
                icon = ModEntry.Instance.Helper.Content.Sprites.RegisterSprite(ModEntry.Instance.Package.PackageRoot.GetRelativeFile("assets/Status/Feedback.png")).Sprite,
                color = new("A17FFF"),
                isGood = true,
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["status", "ManaMax", "name"]).Localize,
            Description = ModEntry.Instance.AnyLocalizations.Bind(["status", "ManaMax", "description"]).Localize
        });

        ManaSpillStatus = ModEntry.Instance.Helper.Content.Statuses.RegisterStatus("ManaSpill", new()
        {
            Definition = new()
            {
                icon = ModEntry.Instance.Helper.Content.Sprites.RegisterSprite(ModEntry.Instance.Package.PackageRoot.GetRelativeFile("assets/Status/Insight.png")).Sprite,
                color = new("A17FFF"),
                isGood = true,
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["status", "ManaSpill", "name"]).Localize,
            Description = ModEntry.Instance.AnyLocalizations.Bind(["status", "ManaSpill", "description"]).Localize
        });

        StirStatus = ModEntry.Instance.Helper.Content.Statuses.RegisterStatus("Stir", new()
        {
            Definition = new()
            {
                icon = ModEntry.Instance.Helper.Content.Sprites.RegisterSprite(ModEntry.Instance.Package.PackageRoot.GetRelativeFile("assets/Status/Intensify.png")).Sprite,
                color = new("DBC6FF"),
                affectedByTimestop = true,
                isGood = true,
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["status", "Stir", "name"]).Localize,
            Description = ModEntry.Instance.AnyLocalizations.Bind(["status", "Stir", "description"]).Localize
        });
        
        ModEntry.Instance.Helper.Events.RegisterAfterArtifactsHook(nameof(Artifact.OnTurnStart), (State state, Combat combat) =>
        {
            var insight = state.ship.Get(ManaSpillStatus.Status);
            var maxInsight = Math.Min(insight, state.ship.Get(StirStatus.Status) + 1);
            maxInsight = Math.Min(maxInsight, state.deck.Count + combat.discard.Count);

            if (maxInsight <= 0)
                return;

            combat.Queue([
                new AStatus
                {
                    targetPlayer = true,
                    status = ManaSpillStatus.Status,
                    statusAmount = -maxInsight,
                    statusPulse = maxInsight > 1 ? StirStatus.Status : null,
                },
                new ScryAction { Amount = maxInsight, FromInsight = true },
                new ADrawCard { count = maxInsight }
            ]);
        });

        ModEntry.Instance.Helper.Events.RegisterAfterArtifactsHook(nameof(Artifact.OnCombatEnd), (State state) =>
        {
            foreach (var card in state.deck)
                ModEntry.Instance.Helper.ModData.RemoveModData(card, "ChosenAuras");
        });

        ModEntry.Instance.Harmony.Patch(
            original: AccessTools.DeclaredMethod(typeof(Ship), nameof(Ship.NormalDamage)),
            prefix: new HarmonyMethod(GetType(), nameof(Ship_NormalDamage_Prefix)),
            finalizer: new HarmonyMethod(GetType(), nameof(Ship_NormalDamage_Finalizer))
        );
        ModEntry.Instance.Harmony.Patch(
            original: AccessTools.DeclaredMethod(typeof(Ship), nameof(Ship.ModifyDamageDueToParts)),
            prefix: new HarmonyMethod(GetType(), nameof(Ship_ModifyDamageDueToParts_Prefix))
        );

        ModEntry.Instance.KokoroApi.StatusLogic.RegisterHook(this);
        ModEntry.Instance.KokoroApi.StatusRendering.RegisterHook(this);
    }

    private static void Ship_NormalDamage_Prefix()
    {
        IsDuringNormalDamage = true;
        ReducedDamage = 0;
    }

    private static void Ship_NormalDamage_Finalizer(Ship __instance, Combat c)
    {
        IsDuringNormalDamage = false;

        var feedback = __instance.Get(ManaMaxStatus.Status);
        var maxFeedback = Math.Min(feedback, __instance.Get(StirStatus.Status) + 1);

        if (maxFeedback > 0)
            c.QueueImmediate([
                new AStatus
                {
                    targetPlayer = true,
                    status = ManaMaxStatus.Status,
                    statusAmount = -maxFeedback,
                    statusPulse = maxFeedback > 1 ? StirStatus.Status : null
                },
                new AHurt
                {
                    targetPlayer = !__instance.isPlayerShip,
                    hurtAmount = maxFeedback,
                    statusPulse = ManaMaxStatus.Status
                }
            ]);

        if (ReducedDamage <= 0)
            return;

        c.QueueImmediate(new AStatus
        {
            targetPlayer = true,
            status = ManaStatus.Status,
            statusAmount = -ReducedDamage,
            statusPulse = ReducedDamage > 1 ? StirStatus.Status : null
        });
        ReducedDamage = 0;
    }

    private static void Ship_ModifyDamageDueToParts_Prefix(Ship __instance, ref int incomingDamage, Part part, bool piercing)
    {
        if (piercing)
            return;
        if (!IsDuringNormalDamage)
            return;
        if (__instance.Get(Status.perfectShield) > 0)
            return;

        var modifier = part.GetDamageModifier();
        if (incomingDamage <= (modifier == PDamMod.weak ? -1 : 0))
            return;

        var veiling = __instance.Get(ManaStatus.Status);
        var maxVeiling = Math.Min(veiling, __instance.Get(StirStatus.Status) + 1);
        if (maxVeiling <= 0)
            return;

        var preMultiplicationDamage = Math.Max(modifier switch
        {
            PDamMod.weak => incomingDamage + 1,
            PDamMod.armor => incomingDamage - 1,
            _ => incomingDamage,
        }, 0);

        var tempShield = __instance.Get(Status.tempShield);
        var accountingTempShieldDamage = Math.Max(modifier switch
        {
            PDamMod.brittle => preMultiplicationDamage - tempShield / 2,
            _ => preMultiplicationDamage - tempShield,
        }, 0);

        if (accountingTempShieldDamage <= 0)
            return;

        var toReduce = Math.Min(maxVeiling, accountingTempShieldDamage);
        incomingDamage -= toReduce;
        ReducedDamage = toReduce;
    }

    public bool HandleStatusTurnAutoStep(IKokoroApi.IV2.IStatusLogicApi.IHook.IHandleStatusTurnAutoStepArgs args)
    {
        if (args.Status != StirStatus.Status)
            return false;
        if (args.Timing != IKokoroApi.IV2.IStatusLogicApi.StatusTurnTriggerTiming.TurnStart)
            return false;
        if (args.Amount == 0)
            return false;

        if (args.State.EnumerateAllArtifacts().FirstOrDefault(a => a is ComposureArtifact) is { } composureArtifact)
        {
            composureArtifact.Pulse();
            args.Amount = Math.Max(args.Amount - 1, 0);
        }
        else
        {
            args.Amount /= 2;
        }
        return false;
    }

    public IReadOnlyList<Tooltip> OverrideStatusTooltips(IKokoroApi.IV2.IStatusRenderingApi.IHook.IOverrideStatusTooltipsArgs args)
    {
        if (args.Status == ManaSpillStatus.Status)
            return [.. args.Tooltips, ..new ScryAction { Amount = 1 }.GetTooltips(DB.fakeState)];
        return args.Tooltips;
    }

    internal sealed class ChooseAuraAction : CardAction
    {
        public required int CardId;
        public required int ActionId;
        public required int Amount;
        public required string UISubtitle;

        public override Icon? GetIcon(State s)
            => new(ChooseAuraIcon.Sprite, Amount, Colors.textMain);

        public override List<Tooltip> GetTooltips(State s)
            => [
                new GlossaryTooltip($"action.{GetType().Namespace!}::ChooseAura")
                {
                    Icon = ChooseAuraIcon.Sprite,
                    TitleColor = Colors.action,
                    Title = ModEntry.Instance.Localizations.Localize(["action", "ChooseAura", "name"]),
                    Description = ModEntry.Instance.Localizations.Localize(["action", "ChooseAura", "description"], new { Amount }),
                },
                ..StatusMeta.GetTooltips(ManaStatus.Status, Math.Max(s.ship.Get(ManaStatus.Status), Amount)),
                ..StatusMeta.GetTooltips(ManaMaxStatus.Status, Math.Max(s.ship.Get(ManaMaxStatus.Status), Amount)),
                ..StatusMeta.GetTooltips(ManaSpillStatus.Status, Math.Max(s.ship.Get(ManaSpillStatus.Status), Amount)),
            ];

        public override Route? BeginWithRoute(G g, State s, Combat c)
            => new ChooseAuraRoute { CardId = CardId, ActionId = ActionId, Amount = Amount, UISubtitle = UISubtitle };
    }

    private sealed class ChooseAuraRoute : Route
    {
        private const UK ChoiceKey = (UK)2137522;

        public required int CardId;
        public required int ActionId;
        public required int Amount;
        public required string UISubtitle;

        public List<Status> Statuses = [
            ManaStatus.Status,
            ManaMaxStatus.Status,
            ManaSpillStatus.Status
        ];

        public override bool GetShowOverworldPanels()
            => true;

        public override bool CanBePeeked()
            => true;

        public override void Render(G g)
        {
            base.Render(g);

            var centerX = 240;
            var topY = 80;

            var choiceWidth = 56;
            var choiceHeight = 24;
            var choiceSpacing = 4;
            var actionYOffset = 7;
            var actionHoverYOffset = 1;

            SharedArt.DrawEngineering(g);

            Draw.Text(ModEntry.Instance.Localizations.Localize((["action", "ChooseAura", "uiTitle"])), centerX, topY, font: DB.stapler, color: Colors.textMain, align: TAlign.Center);
            Draw.Text(UISubtitle, centerX, topY + 24, color: Colors.textMain, align: TAlign.Center);

            var rowWidth = Statuses.Count * choiceWidth + Math.Max(Statuses.Count - 1, 0) * choiceSpacing;
            var rowStartX = centerX - rowWidth / 2;
            for (var i = 0; i < Statuses.Count; i++)
            {
                var ii = i;
                var choice = Statuses[i];
                var fakeAction = new AStatus { targetPlayer = true, status = choice, statusAmount = Amount };
                var choiceStartX = rowStartX + (choiceWidth + choiceSpacing) * i;
                var choiceTopY = topY + 48;

                var buttonRect = new Rect(choiceStartX, choiceTopY, choiceWidth, choiceHeight);
                SharedArt.ButtonText(
                    g, Vec.Zero, new UIKey(ChoiceKey, i), "", rect: buttonRect,
                    onMouseDown: new MouseDownHandler(() => this.OnFinishChoosing(g, ii))
                );

                var isHover = g.boxes.FirstOrDefault(b => b.key == new UIKey(ChoiceKey, i))?.IsHover() == true;
                if (isHover)
                    g.tooltips.Add(new Vec(buttonRect.x + buttonRect.w, buttonRect.y + buttonRect.h), StatusMeta.GetTooltips(choice, Amount));

                var actionWidth = Card.RenderAction(g, g.state, fakeAction, dontDraw: true);
                var actionStartX = choiceStartX + 3 + choiceWidth / 2 - actionWidth / 2;
                var actionXOffset = 0;

                g.Push(rect: new(actionStartX + actionXOffset, choiceTopY + actionYOffset + (isHover ? actionHoverYOffset : 0)));
                Card.RenderAction(g, g.state, fakeAction, dontDraw: false);
                g.Pop();
            }
        }

        private void OnFinishChoosing(G g, int choiceIndex)
        {
            if (g.state.FindCard(CardId) is not { } card)
            {
                g.CloseRoute(this);
                return;
            }

            var choice = Statuses[choiceIndex];
            (g.state.route as Combat)?.QueueImmediate(new AStatus
            {
                targetPlayer = true,
                status = choice,
                statusAmount = Amount
            });

            ModEntry.Instance.Helper.ModData.ObtainModData<Dictionary<int, Status>>(card, "ChosenAuras")[ActionId] = choice;
            g.CloseRoute(this);
        }
    }
}