using System.Collections.Generic;
using System.Reflection;
using Rosseta.External;
using Nanoray.PluginManager;
using Nickel;
using Rosseta.StatusManagers;

namespace Rosseta.Cards.Rosseta;

public class Equivalence : Card, IRegisterable
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
            Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "Equivalence", "name"]).Localize,
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
                    status = ManaStatusManager.ManaStatus.Status,
                    statusAmount = 2,
                    targetPlayer = s.ship.isPlayerShip,
                    shardcost = 1,
                    disabled = flipped
                },
                ModEntry.Instance.KokoroApi.ActionCosts.MakeCostAction(
                    ModEntry.Instance.KokoroApi.ActionCosts.MakeResourceCost(
                        ModEntry.Instance.KokoroApi.ActionCosts.MakeStatusResource(ManaStatusManager.ManaStatus.Status),
                        2),
                    new AStatus()
                    {
                        status = Status.shard,
                        statusAmount = 1,
                        targetPlayer = s.ship.isPlayerShip,
                        disabled = !flipped
                    }).AsCardAction
            ],
            Upgrade.B =>
            [
                new AStatus()
                {
                    status = ManaStatusManager.ManaStatus.Status,
                    statusAmount = 2,
                    targetPlayer = s.ship.isPlayerShip,
                    shardcost = 1
                }
            ],
            _ =>
            [
                new AStatus()
                {
                    status = ManaStatusManager.ManaStatus.Status,
                    statusAmount = 3,
                    targetPlayer = s.ship.isPlayerShip,
                    shardcost = 1
                }
            ]
        };

    public override CardData GetData(State state)
        => new()
        {
            artOverlay = ModEntry.Instance.RossetaCommonOverlay,
            cost = upgrade switch
            {
                Upgrade.B => 0,
                _ => 1
            },
            floppable = upgrade switch
            {
                Upgrade.A => true,
                _ => false
            }
        };
}
