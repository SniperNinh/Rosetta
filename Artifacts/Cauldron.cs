using System.Reflection;
using Nanoray.PluginManager;
using Nickel;
using Rosseta.StatusManagers;

namespace Rosseta.Artifacts;

/*
 * Some Artifacts do not use any of the many hooks available. It may not even utilize the one for setting its sprite.
 * In these cases, they are instead checked at the site of usage.
 * Lexicon is used in AGainPonder.GetNextUpgrade
 */
public class Cauldron : Artifact, IRegisterable
{
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        helper.Content.Artifacts.RegisterArtifact(new ArtifactConfiguration
        {
            ArtifactType = MethodBase.GetCurrentMethod()!.DeclaringType!,
            Meta = new ArtifactMeta
            {
                pools = [ArtifactPool.Boss],
                owner = ModEntry.Instance.RossetaDeck.Deck
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["artifact", "CrystalPouch", "name"]).Localize,
            Description = ModEntry.Instance.AnyLocalizations.Bind(["artifact", "CrystalPouch", "desc"]).Localize,
            
            Sprite = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("assets/Artifact/LexiconA.png")).Sprite
        });
    }

    public override void OnTurnEnd(State state, Combat combat)
    {
        combat.QueueImmediate(
            new AStatus()
            {
                status = ManaStatusManager.ManaStatus.Status,
                statusAmount = state.ship.Get(Status.heat),
                targetPlayer = true,
                artifactPulse = Key()
            });
    }

    public override void AfterPlayerOverheat(State state, Combat combat)
    {
        combat.QueueImmediate(
            new AStatus()
            {
                status = ManaStatusManager.ManaStatus.Status,
                statusAmount = -state.ship.Get(ManaStatusManager.ManaStatus.Status),
                targetPlayer = true,
                artifactPulse = Key()
            });
    }
}