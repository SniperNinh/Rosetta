using HarmonyLib;
using Nanoray.PluginManager;
using Nickel;
using System.Reflection;

namespace Rosseta.StatusManagers;

internal sealed class ManaSpillStatusManager : IRegisterable
{
    internal static IStatusEntry ManaSpillStatus { get; private set; } = null!;

    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        ManaSpillStatus = ModEntry.Instance.Helper.Content.Statuses.RegisterStatus("ManaSpill",
            new StatusConfiguration
            {
                Definition = new StatusDef
                {
                    isGood = true,
                    affectedByTimestop = true,
                    color = new("23EEB6"),
                    icon = ModEntry.Instance.Helper.Content.Sprites
                        .RegisterSprite(
                            ModEntry.Instance.Package.PackageRoot.GetRelativeFile("assets/Artifact/LexiconA.png"))
                        .Sprite,
                },
                Name = ModEntry.Instance.AnyLocalizations.Bind(["status", "ManaSpill", "name"]).Localize,
                Description = ModEntry.Instance.AnyLocalizations.Bind(["status", "ManaSpill", "desc"]).Localize
            });
        
        ModEntry.Instance.Harmony.Patch(
            original: AccessTools.DeclaredMethod(typeof(AOverheat), nameof(AOverheat.Begin)),
            postfix: new HarmonyMethod(MethodBase.GetCurrentMethod()!.DeclaringType!, nameof(AOverheat_Begin_Postfix))
        );
    }

    /*
     * To add functiality that activates on non turn start/end events you make a method like the one below.
     * This one activates when a ship overheats.
     * it checks what ship overheated and adds "BasicStatus" to the damage
     */
    private static void AOverheat_Begin_Postfix(AOverheat __instance, State s, Combat c)
    {
        if (s.ship.Get(ManaStatusManager.ManaStatus.Status) <= 0) 
            return;
        
        var action = new AStatus
        {
            targetPlayer = __instance.targetPlayer,
            status = ManaSpillStatus.Status,
            statusAmount = 1
        };

        ModEntry.Instance.Helper.ModData.CopyAllModData(__instance, action);
        c.QueueImmediate(action);
    }
}