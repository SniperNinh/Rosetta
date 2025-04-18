using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Rosseta.Artifacts;

/*
 * Artifacts are a nice way to accentuate a character's potential.
 * They can be simple effects that occur at simple times, they can modify an existing mechanic or one introduced by the character.
 * Similarly to cards, ensure you add this type to your ModEntry for registration.
 */
public class ManaShelf : Artifact, IRegisterable
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
}