using System.Collections.Generic;
using System.Reflection;
using Rosseta.External;
using Nanoray.PluginManager;
using Nickel;
using Rosseta.StatusManagers;

namespace Rosseta.Cards.Rosseta;

public class BottleSmash : Card, IRegisterable
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
                rarity = Rarity.uncommon,
                upgradesTo = [Upgrade.A, Upgrade.B]
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "BottleSmash", "name"]).Localize,
            // Art = ModEntry.RegisterSprite(package, "assets/Cards/Ponder.png").Sprite
        });
    }

    public override List<CardAction> GetActions(State s, Combat c)
        => upgrade switch
        {
            Upgrade.A =>
            [
                new AAttack()
                {
                    damage = 3,
                    targetPlayer = !s.ship.isPlayerShip
                },
                new AStatus()
                {
                    status = ManaSpillStatusManager.ManaSpillStatus.Status,
                    statusAmount = 1,
                    targetPlayer = s.ship.isPlayerShip
                },
                new AStatus()
                {
                    status = ManaStatusManager.ManaStatus.Status,
                    statusAmount = 0,
                    mode = AStatusMode.Set,
                    targetPlayer = s.ship.isPlayerShip
                }
            ],
            _ =>
            [
                new AAttack()
                {
                    damage = 2,
                    targetPlayer = !s.ship.isPlayerShip
                },
                new AStatus()
                {
                    status = ManaSpillStatusManager.ManaSpillStatus.Status,
                    statusAmount = 1,
                    targetPlayer = s.ship.isPlayerShip
                }
            ]
        };

    public override CardData GetData(State state)
    {
        return new CardData
        {
            artOverlay = ModEntry.Instance.RossetaUncommonOverlay,
            cost = 0
        };
    }
}
