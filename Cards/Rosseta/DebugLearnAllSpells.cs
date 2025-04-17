using System.Collections.Generic;
using System.Reflection;
using Rosseta.External;
using Nanoray.PluginManager;
using Nickel;
using Rosseta.Artifacts;
using System.Linq;

namespace Rosseta.Cards.Rosseta;

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
                upgradesTo = [Upgrade.A, Upgrade.B]
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "BasicCard", "name"]).Localize,
            // Art = ModEntry.RegisterSprite(package, "assets/Cards/Ponder.png").Sprite
        });
    }

    public override List<CardAction> GetActions(State s, Combat c)
    {
        List<Card> cards = new List<Card>();
        List<CardAction> actions = new List<CardAction>();
        
        if (s.EnumerateAllArtifacts().OfType<SpellBook>().FirstOrDefault() is { } spellBook)
        {
            List<Card> acidcards = spellBook.UnLearnedAcidSpells.ToList();
            List<Card> firecards = spellBook.UnLearnedFireSpells.ToList();
            List<Card> aircards = spellBook.UnLearnedAirSpells.ToList();
            List<Card> icecards = spellBook.UnLearnedIceSpells.ToList();
            List<Card> specialcards = spellBook.UnLearnedSpecialSpells.ToList();

            foreach (var acidcard in acidcards)
            {
                spellBook.LearnedAcidSpells.Add(acidcard);
                spellBook.UnLearnedAcidSpells.Remove(acidcard);
            }
            foreach (var aircard in aircards)
            {
                spellBook.LearnedAirSpells.Add(aircard);
                spellBook.UnLearnedAirSpells.Remove(aircard);
            }
            foreach (var firecard in firecards)
            {
                spellBook.LearnedFireSpells.Add(firecard);
                spellBook.UnLearnedFireSpells.Remove(firecard);
            }
            foreach (var icecard in icecards)
            {
                spellBook.LearnedIceSpells.Add(icecard);
                spellBook.UnLearnedIceSpells.Remove(icecard);
            }
            foreach (var specialcard in specialcards)
            {
                spellBook.LearnedSpecialSpells.Add(specialcard);
                spellBook.UnLearnedSpecialSpells.Remove(specialcard);
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
