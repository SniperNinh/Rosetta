using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;
using Rosseta.Actions;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Rosseta.External;
using Nanoray.PluginManager;
using Nickel;
using Rosseta.Actions;
using Rosseta.Artifacts;

namespace Rosseta.Artifacts;

/*
 * Artifacts are a nice way to accentuate a character's potential.
 * They can be simple effects that occur at simple times, they can modify an existing mechanic or one introduced by the character.
 * Similarly to cards, ensure you add this type to your ModEntry for registration.
 */
public class BookShelf : Artifact, IRegisterable
{
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        helper.Content.Artifacts.RegisterArtifact(new ArtifactConfiguration
        {
            ArtifactType = MethodBase.GetCurrentMethod()!.DeclaringType!,
            Meta = new ArtifactMeta
            {
                pools = [ArtifactPool.Common],
                owner = ModEntry.Instance.RossetaDeck.Deck
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["artifact", "BuriedKnowledge", "name"]).Localize,
            Description = ModEntry.Instance.AnyLocalizations.Bind(["artifact", "BuriedKnowledge", "desc"]).Localize,
            /*
             * For Artifacts with just one sprite, registering them at the place of usage helps simplify things.
             */
            Sprite = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("assets/Artifact/Ashtray.png")).Sprite
        });
    }
    
    /*
     * Unlike Cards, Artifacts have no required methods. Implement the ones you need, and leave the rest unimplemented.
     * By default, Artifacts have everything implemented with methods that do nothing, so there is no need to call the super.
     */
    public override void OnCombatStart(State state, Combat combat)
    {
        List<CardAction> actions = new List<CardAction>();
        
        if (state.EnumerateAllArtifacts().OfType<SpellBook>().FirstOrDefault() is { } spellBook)
        {
            actions.Add(
                new ASpecificCardTypeOffering()
                {
                    Cards = GetSpellTypeCardsFromSpellBook(spellBook),
                    Destination = CardDestination.Hand
                }
            );
        }
        
        combat.QueueImmediate(actions);
    }
    private List<Card> GetSpellTypeCardsFromSpellBook(SpellBook spellBook)
    {
        List<Card> elementCardList = new List<Card>();
        foreach (var elementCard in spellBook.LearnedSpells)
        {
            if (elementCard is ISpecialCard) continue;
            elementCardList.Add(elementCard);
        }
        return elementCardList.Count > 0 ? elementCardList : spellBook.DebugSpells;
    }
}