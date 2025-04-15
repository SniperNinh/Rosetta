using HarmonyLib;
using Nanoray.PluginManager;
using Nickel;
using System;
using System.Reflection;
using Rosseta.External;

namespace Rosseta.StatusManagers;

internal sealed class BasicStatusManager : IRegisterable
{
    internal static IStatusEntry BasicStatus { get; private set; } = null!;

    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
    BasicStatus = ModEntry.Instance.Helper.Content.Statuses.RegisterStatus("BasicStatus", new()
        {
            Definition = new()
            {
                icon = ModEntry.Instance.Helper.Content.Sprites.RegisterSprite(ModEntry.Instance.Package.PackageRoot.GetRelativeFile("assets/Artifact/LexiconA.png")).Sprite,
                color = new("23EEB6"),
                isGood = true,
                affectedByTimestop = true,
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["status", "BasicStatus", "name"]).Localize,
            Description = ModEntry.Instance.AnyLocalizations.Bind(["status", "BasicStatus", "description"]).Localize
        });

        
        ModEntry.Instance.KokoroApi.StatusLogic.RegisterHook(new StatusLogicHook());
        
        
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
        var ship = __instance.targetPlayer ? s.ship : c.otherShip;
        var BasicStatusAmount = ship.Get(BasicStatus.Status);
        
        var action = new AStatus
        {
            targetPlayer = !__instance.targetPlayer,
            status = BasicStatus.Status,
            statusAmount = BasicStatusAmount
        };
        
        ModEntry.Instance.Helper.ModData.CopyAllModData(__instance, action);
        c.QueueImmediate(action);
    }
    
    private sealed class StatusLogicHook : IKokoroApi.IV2.IStatusLogicApi.IHook
    { }
}