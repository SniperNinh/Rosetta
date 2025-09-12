using System;
using System.Collections.Generic;
using System.Reflection;
using Rosseta.External;
using Nanoray.PluginManager;
using Nickel;
using Rosseta.StatusManagers;

namespace Rosseta.Cards.Rosseta;

public class BottleRepair : Card, IRegisterable
{
    private static IKokoroApi.IV2.IConditionalApi Conditional => ModEntry.Instance.KokoroApi.Conditional;

    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        helper.Content.Cards.RegisterCard(new CardConfiguration
        {
            CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
            Meta = new CardMeta
            {
                deck = ModEntry.Instance.RossetaDeck.Deck,
                rarity = Rarity.common,
                upgradesTo = [Upgrade.A, Upgrade.B]
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "BottleRepair", "name"]).Localize,
            // Art = ModEntry.RegisterSprite(package, "assets/Cards/Ponder.png").Sprite
        });
    }

    public override List<CardAction> GetActions(State s, Combat c)
        => upgrade switch
        {
            Upgrade.A =>
            [
                new AStatus()
                {
                    status = ManaSpillStatusManager.ManaSpillStatus.Status,
                    statusAmount = -1,
                    targetPlayer = s.ship.isPlayerShip,
                    shardcost = 1
                }
            ],
            Upgrade.B =>
            [
                ModEntry.Instance.KokoroApi.ContinueStop
                    .MakeTriggerAction(IKokoroApi.IV2.IContinueStopApi.ActionType.Continue, out Guid triggerGuid)
                    .AsCardAction,
                ModEntry.Instance.KokoroApi.ContinueStop.MakeFlaggedAction
                (
                    IKokoroApi.IV2.IContinueStopApi.ActionType.Continue,
                    triggerGuid,
                    new AStatus()
                    {
                        status = ManaSpillStatusManager.ManaSpillStatus.Status,
                        statusAmount = -1,
                        targetPlayer = s.ship.isPlayerShip,
                        shardcost = 2
                    }).AsCardAction,
                ModEntry.Instance.KokoroApi.ContinueStop.MakeFlaggedAction
                (
                    IKokoroApi.IV2.IContinueStopApi.ActionType.Continue,
                    triggerGuid,
                    new AStatus()
                    {
                        status = ManaStatusManager.ManaStatus.Status,
                        statusAmount = +2,
                        targetPlayer = s.ship.isPlayerShip,
                        shardcost = 2
                    }).AsCardAction
            ],
            _ => [
                new AStatus()
                {
                    status = ManaSpillStatusManager.ManaSpillStatus.Status,
                    statusAmount = -1,
                    targetPlayer = s.ship.isPlayerShip,
                    shardcost = 2
                }
            ]
        };

    public override CardData GetData(State state)
    {
        return new CardData
        {
            artOverlay = ModEntry.Instance.RossetaCommonOverlay,
            cost = 1
        };
    }
}
