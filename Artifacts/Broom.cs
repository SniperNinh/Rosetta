using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Rosseta.Artifacts;

/*
 * Some Artifacts do not use any of the many hooks available. It may not even utilize the one for setting its sprite.
 * In these cases, they are instead checked at the site of usage.
 * Lexicon is used in AGainPonder.GetNextUpgrade
 */
public class Broom : Artifact, IRegisterable
{
    
    public int Counter = 0;
    
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
            Name = ModEntry.Instance.AnyLocalizations.Bind(["artifact", "CrystalPouch", "name"]).Localize,
            Description = ModEntry.Instance.AnyLocalizations.Bind(["artifact", "CrystalPouch", "desc"]).Localize,
            
            Sprite = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("assets/Artifact/LexiconA.png")).Sprite
        });
    }
    
    public override void OnPlayerPlayCard(
        int energyCost,
        Deck deck,
        Card card,
        State state,
        Combat combat,
        int handPosition,
        int handCount)
    {
        if (!(card.GetType() == new ShardCard().GetType()))
            return;
        
        Counter++;
        
        if (Counter < 10)
            return;
        
        Counter = 0;
        combat.Queue(new AStatus()
        {
            targetPlayer = true,
            status = Status.maxShard,
            statusAmount = 1,
            artifactPulse = Key()
        });
    }

    public override void OnCombatEnd(State state)
    {
        Counter = 0;
    }

    public override int? GetDisplayNumber(State s) => new int?(Counter);
}