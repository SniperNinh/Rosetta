using System.Collections.Generic;
using System.Reflection;
using Rosseta.External;
using Nanoray.PluginManager;
using Nickel;
using Rosseta.StatusManagers;

namespace Rosseta.Cards.Rosseta;

public class GlassPack : Card, IRegisterable
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
            Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "GlassPack", "name"]).Localize,
            // Art = ModEntry.RegisterSprite(package, "assets/Cards/Ponder.png").Sprite
        });
    }

    public override List<CardAction> GetActions(State s, Combat c)
    {
        return
        [
            new AStatus()
            {
                status = Status.heat,
                statusAmount = 1,
                targetPlayer = s.ship.isPlayerShip
            },
            new AAddCard()
            {
                amount = 1,
                card = new ShardCard()
            },
            new AAddCard()
            {
                amount = 1,
                card = new ManaBottle()
            }
        ];
    }

    public override CardData GetData(State state)
    {
        return new CardData
        {
            cost = 1,
            description = string.Format(ModEntry.Instance.Localizations.Localize(["card", "GlassPack", "desc"]))
        };
    }
}
