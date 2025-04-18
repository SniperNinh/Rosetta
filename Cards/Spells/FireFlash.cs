using System;
using System.Collections.Generic;
using System.Reflection;
using Rosseta.External;
using Nanoray.PluginManager;
using Nickel;
using Rosseta.StatusManagers;

namespace Rosseta.Cards.Spells;

public class FireFlash : Card, IRegisterable, IFireCard
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
                /*
                 * this card is marked as rare here because of card rewards even though it doesnt count as a special card
                 */
                rarity = Rarity.rare,
                dontOffer = true,
                upgradesTo = [Upgrade.A, Upgrade.B]
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "FireFlash", "name"]).Localize,
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
                    8),
                ModEntry.Instance.KokoroApi.ContinueStop.MakeTriggerAction(IKokoroApi.IV2.IContinueStopApi.ActionType.Continue, out Guid triggerGuid).AsCardAction
            ).AsCardAction,
            ModEntry.Instance.KokoroApi.ContinueStop.MakeFlaggedAction
            (
                IKokoroApi.IV2.IContinueStopApi.ActionType.Continue,
                triggerGuid,
                new AStatus()
                {
                    status = Status.heat,
                    statusAmount = 3,
                    targetPlayer = s.ship.isPlayerShip
                }).AsCardAction,
            ModEntry.Instance.KokoroApi.ContinueStop.MakeFlaggedAction
            (
                IKokoroApi.IV2.IContinueStopApi.ActionType.Continue,
                triggerGuid,
                new AEndTurn()).AsCardAction,
            ModEntry.Instance.KokoroApi.ContinueStop.MakeFlaggedAction
            (
                IKokoroApi.IV2.IContinueStopApi.ActionType.Continue,
                triggerGuid,
                new AStunShip()).AsCardAction
        ];
    }

    public override CardData GetData(State state)
    {
        return new CardData
        {
            cost = 0,
            exhaust = true,
            temporary = true
        };
    }
}
