using System.Collections.Generic;
using System.Reflection;
using Rosseta.External;
using Nanoray.PluginManager;
using Nickel;
using Rosseta.StatusManagers;

namespace Rosseta.Cards.Rosseta;

public class ManaBottle : Card, IRegisterable
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
                dontOffer = true,
                upgradesTo = [Upgrade.A, Upgrade.B]
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "ManaBottle", "name"]).Localize,
            // Art = ModEntry.RegisterSprite(package, "assets/Cards/Ponder.png").Sprite
        });
    }

    public override List<CardAction> GetActions(State s, Combat c)
    {
        return
        [
            new AStatus()
            {
                status = ManaStatusManager.ManaStatus.Status,
                statusAmount = 1,
                targetPlayer = s.ship.isPlayerShip
            },
            new ADrawCard
            {
                count = 1
            }
        ];
    }

    public override CardData GetData(State state)
    {
        return new CardData
        {
            artOverlay = ModEntry.RegisterSprite(ModEntry.Instance.Package, "assets/Border_Special_Rosseta.png").Sprite,
            cost = 0,
            temporary = true
        };
    }
}
