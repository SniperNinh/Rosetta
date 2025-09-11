using Nanoray.PluginManager;
using Nickel;
using HarmonyLib;
using System.Reflection;
using Rosseta.External;

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
                            ModEntry.Instance.Package.PackageRoot.GetRelativeFile("assets/ManaSpillStatus.png"))
                        .Sprite,
                },
                Name = ModEntry.Instance.AnyLocalizations.Bind(["status", "ManaSpill", "name"]).Localize,
                Description = ModEntry.Instance.AnyLocalizations.Bind(["status", "ManaSpill", "desc"]).Localize
            });
        
        ModEntry.Instance.Harmony.Patch(
            original: AccessTools.DeclaredMethod(typeof(AOverheat), nameof(AOverheat.Begin)),
            postfix: new HarmonyMethod(MethodBase.GetCurrentMethod()!.DeclaringType!, nameof(AOverheat_Begin_Postfix))
        );
        
        ModEntry.Instance.KokoroApi.StatusLogic.RegisterHook(new StatusLogicHook());
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
    private sealed class StatusLogicHook : IKokoroApi.IV2.IStatusLogicApi.IHook
    {
        public bool HandleStatusTurnAutoStep(IKokoroApi.IV2.IStatusLogicApi.IHook.IHandleStatusTurnAutoStepArgs args)
        {
            if (args.Status != ManaSpillStatus.Status)
                return false;
            if (args.Timing != IKokoroApi.IV2.IStatusLogicApi.StatusTurnTriggerTiming.TurnStart)
                return false;
            
            
            if (args.Ship.Get(ManaStatusManager.ManaStatus.Status) > 0)
            {
                args.Combat.QueueImmediate(
                    new AStatus
                    {
                        status = ManaStatusManager.ManaStatus.Status,
                        statusAmount = -args.Ship.Get(ManaSpillStatus.Status),
                        targetPlayer = args.Ship.isPlayerShip,
                        timer = 0
                    }
                );
            }
            
            if (args.Amount == 0)
            {
                args.Combat.QueueImmediate(
                    new AStatus
                    {
                        status = ManaSpillStatusManager.ManaSpillStatus.Status,
                        statusAmount = -1,
                        targetPlayer = args.Ship.isPlayerShip,
                        timer = 0
                    }
                );
            }
            
            return false;
        }
    }
}