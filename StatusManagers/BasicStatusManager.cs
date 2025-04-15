using HarmonyLib;
using Nanoray.PluginManager;
using Nickel;
using System.Reflection;
using Rosseta.External;

namespace Rosseta.StatusManagers;

internal sealed class BasicStatusManager : IRegisterable
{
    internal static IStatusEntry BasicStatus { get; private set; } = null!;

    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
    BasicStatus = ModEntry.Instance.Helper.Content.Statuses.RegisterStatus("BasicStatus", new StatusConfiguration
        {
            Definition = new StatusDef
            {
                isGood = true,
                affectedByTimestop = true,
                color = new("23EEB6"),
                icon = ModEntry.Instance.Helper.Content.Sprites.RegisterSprite(ModEntry.Instance.Package.PackageRoot.GetRelativeFile("assets/Artifact/LexiconA.png")).Sprite,
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["status", "BasicStatus", "name"]).Localize,
            Description = ModEntry.Instance.AnyLocalizations.Bind(["status", "BasicStatus", "desc"]).Localize
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
    private static void AOverheat_Begin_Postfix(AOverheat __instance, Combat c)
    {
        
        var action = new AStatus
        {
            targetPlayer = __instance.targetPlayer,
            status = BasicStatus.Status,
            statusAmount = -1
        };
        
        ModEntry.Instance.Helper.ModData.CopyAllModData(__instance, action);
        c.QueueImmediate(action);
    }
    
    private sealed class StatusLogicHook : IKokoroApi.IV2.IStatusLogicApi.IHook
    {
        public bool HandleStatusTurnAutoStep(IKokoroApi.IV2.IStatusLogicApi.IHook.IHandleStatusTurnAutoStepArgs args)
        {
            if (args.Status != BasicStatus.Status)
                return false;
            if (args.Timing != IKokoroApi.IV2.IStatusLogicApi.StatusTurnTriggerTiming.TurnStart)
                return false;
            if (args.Amount <= 0)
                return false;
            
            args.Amount++;
            return false;
        }
    }
}