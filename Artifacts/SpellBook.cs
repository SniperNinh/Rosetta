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
    public List<Card> LearnedFireSpells = new List<Card>();
    public List<Card> UnLearnedFireSpells = new List<Card>();
    public List<Card> LearnedAirSpells = new List<Card>();
    public List<Card> UnLearnedAirSpells = new List<Card>();
    public List<Card> LearnedIceSpells = new List<Card>();
    public List<Card> UnLearnedIceSpells = new List<Card>();
    public List<Card> LearnedAcidSpells = new List<Card>();
    public List<Card> UnLearnedAcidSpells = new List<Card>();
    public List<Card> LearnedSpecialSpells = new List<Card>();
    public List<Card> UnLearnedSpecialSpells = new List<Card>();
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
        LearnedFireSpells = addCards(ModEntry.RossetaStarterFireSpellCardTypes.ToList());
        UnLearnedFireSpells = addCards(ModEntry.RossetaFireSpellCardTypes.ToList());
        LearnedAirSpells = addCards(ModEntry.RossetaStarterAirSpellCardTypes.ToList());
        UnLearnedAirSpells = addCards(ModEntry.RossetaAirSpellCardTypes.ToList());
        UnLearnedIceSpells = addCards(ModEntry.RossetaIceSpellCardTypes.ToList());
        UnLearnedAcidSpells = addCards(ModEntry.RossetaAcidSpellCardTypes.ToList());
        UnLearnedSpecialSpells = addCards(ModEntry.RossetaSpecialSpellCardTypes.ToList());
        DebugSpells = addCards(ModEntry.RossetaDebugSpellCardTypes.ToList());
    }

    public override void OnCombatStart(State state, Combat combat)
    {
        
        if (LearnedFireSpells.Count <= 0)
            LearnedFireSpells = addCards(ModEntry.RossetaStarterFireSpellCardTypes.ToList());
        
        if (LearnedAirSpells.Count <= 0)
            LearnedAirSpells = addCards(ModEntry.RossetaStarterAirSpellCardTypes.ToList());
        
        if (UnLearnedFireSpells.Count <= 0)
            UnLearnedFireSpells = addCards(ModEntry.RossetaFireSpellCardTypes.ToList());
        
        if (UnLearnedAcidSpells.Count <= 0)
            UnLearnedAcidSpells = addCards(ModEntry.RossetaAcidSpellCardTypes.ToList());
        
        if (UnLearnedIceSpells.Count <= 0)
            UnLearnedIceSpells = addCards(ModEntry.RossetaIceSpellCardTypes.ToList());
        
        if (UnLearnedAirSpells.Count <= 0)
            UnLearnedAirSpells = addCards(ModEntry.RossetaAirSpellCardTypes.ToList());
        
        if (UnLearnedSpecialSpells.Count <= 0)
            UnLearnedSpecialSpells = addCards(ModEntry.RossetaSpecialSpellCardTypes.ToList());
        
        ModEntry.Instance.Logger.LogInformation(UnLearnedAcidSpells.Count().ToString());
        ModEntry.Instance.Logger.LogInformation(UnLearnedIceSpells.Count().ToString());
        ModEntry.Instance.Logger.LogInformation(UnLearnedAirSpells.Count().ToString());
        ModEntry.Instance.Logger.LogInformation(UnLearnedFireSpells.Count().ToString());
        ModEntry.Instance.Logger.LogInformation(UnLearnedSpecialSpells.Count().ToString());
    }

    public override void OnCombatEnd(State state)
    {
        if (state.map.markers[state.map.currentLocation].contents is MapBattle mapBattle)
        {
            if (mapBattle.battleType == BattleType.Elite || mapBattle.battleType == BattleType.Boss)
            {
                state.rewardsQueue.Add(new ALearnSpell());
            }
        }
    }

    private List<Card> addCards(List<Type> initCards)
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