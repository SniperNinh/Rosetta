using System.Reflection;
using Nickel;
using Rosseta.Actions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Nanoray.PluginManager;

namespace Rosseta.Artifacts;

/*
 * Artifacts are a nice way to accentuate a character's potential.
 * They can be simple effects that occur at simple times, they can modify an existing mechanic or one introduced by the character.
 * Similarly to cards, ensure you add this type to your ModEntry for registration.
 */
public class SpellBook : Artifact, IRegisterable
{
    public List<Card> LearnedSpells = new List<Card>();
    public List<Card> UnLearnedSpells = new List<Card>();
    public List<Card> DebugSpells = new List<Card>();
    
    
    
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        helper.Content.Artifacts.RegisterArtifact("SpellBook", new()
        {
            ArtifactType = MethodBase.GetCurrentMethod()!.DeclaringType!,
            Meta = new()
            {
                owner = ModEntry.Instance.RossetaDeck.Deck,
                pools = [ArtifactPool.Boss],
                unremovable = true
            },
            Sprite = helper.Content.Sprites.RegisterSprite(ModEntry.Instance.Package.PackageRoot.GetRelativeFile("assets/Artifact/LexiconA.png")).Sprite,
            Name = ModEntry.Instance.AnyLocalizations.Bind(["artifact", "SpellBook", "name"]).Localize,
            Description = ModEntry.Instance.AnyLocalizations.Bind(["artifact", "SpellBook", "desc"]).Localize
        });
    }
    public override void OnReceiveArtifact(State state)
    {
        List<Card> spellCards = AddCards(ModEntry.RossetaSpellCardTypes.ToList());
        
        DebugSpells = AddCards(ModEntry.RossetaDebugSpellCardTypes.ToList());
        
        foreach (var spellCard in spellCards)
        {
            if (spellCard is IIsDebugSpell) continue;
            if (spellCard is IIsStarterSpell)
            {
                LearnedSpells.Add(spellCard);
            } else {
                UnLearnedSpells.Add(spellCard);
            }
        }
    }

    public override void OnCombatStart(State state, Combat combat)
    {
        if (DebugSpells.Count <= 0)
            DebugSpells = AddCards(ModEntry.RossetaDebugSpellCardTypes.ToList());

        if (LearnedSpells.Count <= 0)
        {
            List<Card> spellCards = AddCards(ModEntry.RossetaSpellCardTypes.ToList());

            foreach (var spellCard in spellCards)
            {
                if (spellCard is IIsDebugSpell) continue;
                
                if (spellCard is IIsStarterSpell)
                {
                    LearnedSpells.Add(spellCard);
                } else {
                    UnLearnedSpells.Add(spellCard);
                }
            }
        }
        
        ModEntry.Instance.Logger.LogInformation(UnLearnedSpells.Count().ToString());
    }

    public override void OnCombatEnd(State state)
    {
        if (state.map.markers[state.map.currentLocation].contents is MapBattle mapBattle)
        {
            if (mapBattle.battleType == BattleType.Elite || mapBattle.battleType == BattleType.Boss)
            {
                state.rewardsQueue.Add(new ALearnSpell()
                {
                    Amount = 3,
                    battleType = mapBattle.battleType
                });
            }
        }
    }

    private List<Card> AddCards(List<Type> initCards)
    {
        var list = new List<Card>();

        foreach (var spell in initCards)
        {
            Card? card = (Card?)Activator.CreateInstance(spell);
            if (card != null)
            {
                list.Add(card);
            }
        }

        return list;
    }
};