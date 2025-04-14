using System.Collections.Generic;
using System.Reflection;
using Rosseta.External;
using Nanoray.PluginManager;
using Nickel;

namespace Rosseta.Cards;

public class BundledCrystals : Card, IRegisterable
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
                rarity = Rarity.rare,
                upgradesTo = [Upgrade.A, Upgrade.B]
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "BundledCrystals", "name"]).Localize,
            // Art = ModEntry.RegisterSprite(package, "assets/Cards/Ponder.png").Sprite
        });
    }

    public override List<CardAction> GetActions(State s, Combat c)
    {
        return
        [
            new AAddCard
            {
                destination = CardDestination.Deck,
                card = new ShardCard(),
                amount = 3
            }
        ];
    }

    public override CardData GetData(State state)
    {
        return new CardData
        {
            cost = 2,
            exhaust = true,
            description = string.Format(ModEntry.Instance.Localizations.Localize([
            "card",
            "BundledCrystals",
            "desc"]))
        };
    }
}
