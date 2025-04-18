using System.Collections.Generic;
using System.Reflection;
using Rosseta.External;
using Nanoray.PluginManager;
using Nickel;
using Rosseta.StatusManagers;

namespace Rosseta.Cards.Rosseta;

public class Brooooom : Card, IRegisterable
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
            Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "Brooooom", "name"]).Localize,
            // Art = ModEntry.RegisterSprite(package, "assets/Cards/Ponder.png").Sprite
        });
    }
    
    public override List<CardAction> GetActions(State s, Combat c)
    {
        return
        [
            new AMove()
            {
                dir = 3,
                targetPlayer = s.ship.isPlayerShip
            }
        ];
    }
    
    public override CardData GetData(State state)
    {
        return new CardData
        {
            cost = 0,
            temporary = true,
            singleUse = true,
            retain = true,
            flippable = true
        };
    }
}
