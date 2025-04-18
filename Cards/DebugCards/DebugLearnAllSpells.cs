using System.Collections.Generic;
using System.Reflection;
using Rosseta.External;
using Nanoray.PluginManager;
using Nickel;
using Rosseta.Artifacts;
using System.Linq;

namespace Rosseta.Cards.DebugCards;

public class DebugLearnAllSpells : Card, IRegisterable
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
                unreleased = true,
                upgradesTo = [Upgrade.A, Upgrade.B]
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "DebugLearnAllSpells", "name"]).Localize,
            // Art = ModEntry.RegisterSprite(package, "assets/Cards/Ponder.png").Sprite
        });
    }

    public override List<CardAction> GetActions(State s, Combat c)
    {
        List<Card> cards = new List<Card>();
        List<CardAction> actions = new List<CardAction>();

        if (s.EnumerateAllArtifacts().OfType<SpellBook>().FirstOrDefault() is { } spellBook)
        {
            foreach (var card in spellBook.UnLearnedSpells.ToList())
            {
                spellBook.LearnedSpells.Add(card);
                spellBook.UnLearnedSpells.Remove(card);
            }
        }
        return actions;
    }

    public override CardData GetData(State state)
    {
        return new CardData
        {
            cost = 1
        };
    }
}
