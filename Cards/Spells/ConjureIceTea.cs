using System;
using System.Collections.Generic;
using System.Reflection;
using Rosseta.External;
using Nanoray.PluginManager;
using Nickel;
using Rosseta.StatusManagers;

namespace Rosseta.Cards.Spells;

public class ConjureIceTea : Card, IRegisterable
{
    private static IKokoroApi.IV2.IConditionalApi Conditional => ModEntry.Instance.KokoroApi.Conditional;

    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        helper.Content.Cards.RegisterCard(new CardConfiguration
        {
            
            CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
            Meta = new CardMeta
            {
                deck = ModEntry.Instance.RossetaSpellDeck.Deck,
                rarity = Rarity.common,
                dontOffer = true,
                upgradesTo = [Upgrade.A, Upgrade.B]
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "ConjureIceTea", "name"]).Localize,
            // Art = ModEntry.RegisterSprite(package, "assets/Cards/Ponder.png").Sprite
        });
    }

    public override List<CardAction> GetActions(State s, Combat c)
    {
        return
        [
            ModEntry.Instance.KokoroApi.ActionCosts.MakeCostAction(
                ModEntry.Instance.KokoroApi.ActionCosts.MakeResourceCost(
                    ModEntry.Instance.KokoroApi.ActionCosts.MakeStatusResource(ManaStatusManager.ManaStatus.Status),
                    4),
                ModEntry.Instance.KokoroApi.ContinueStop.MakeTriggerAction(IKokoroApi.IV2.IContinueStopApi.ActionType.Continue, out Guid triggerGuid).AsCardAction
            ).AsCardAction,
            ModEntry.Instance.KokoroApi.ContinueStop.MakeFlaggedAction
            (
                IKokoroApi.IV2.IContinueStopApi.ActionType.Continue,
                triggerGuid,
                new AStatus()
                {
                    status = Status.heat,
                    statusAmount = -2,
                    targetPlayer = s.ship.isPlayerShip,
                    disabled = flipped
                }).AsCardAction,
            ModEntry.Instance.KokoroApi.ContinueStop.MakeFlaggedAction
            (
                IKokoroApi.IV2.IContinueStopApi.ActionType.Continue,
                triggerGuid,
                new AStatus()
                {
                    status = Status.serenity,
                    statusAmount = 1,
                    targetPlayer = s.ship.isPlayerShip,
                    disabled = !flipped
                }).AsCardAction
        ];
    }

    public override CardData GetData(State state)
    {
        return new CardData
        {
            cost = 0,
            exhaust = true,
            floppable = true,
            temporary = true
        };
    }
}
