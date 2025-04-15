using Nanoray.PluginManager;
using Nickel;
using Rosseta.External;

namespace Rosseta.StatusManagers;

internal sealed class StirStatusManager : IRegisterable
{
    internal static IStatusEntry StirStatus { get; private set; } = null!;

    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
    StirStatus = ModEntry.Instance.Helper.Content.Statuses.RegisterStatus("Stir", new StatusConfiguration
        {
            Definition = new StatusDef
            {
                isGood = true,
                affectedByTimestop = true,
                color = new("23EEB6"),
                icon = ModEntry.Instance.Helper.Content.Sprites.RegisterSprite(ModEntry.Instance.Package.PackageRoot.GetRelativeFile("assets/Artifact/LexiconA.png")).Sprite,
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["status", "Stir", "name"]).Localize,
            Description = ModEntry.Instance.AnyLocalizations.Bind(["status", "Stir", "desc"]).Localize
        });
        
        ModEntry.Instance.KokoroApi.StatusLogic.RegisterHook(new StatusLogicHook());
    }
    
    /*
     * To add functiality that activates on non turn start/end events you make a method like the one below.
     * This one activates when a ship overheats.
     * it checks what ship overheated and adds "BasicStatus" to the damage
     */
    private sealed class StatusLogicHook : IKokoroApi.IV2.IStatusLogicApi.IHook
    {
        
        public bool HandleStatusTurnAutoStep(IKokoroApi.IV2.IStatusLogicApi.IHook.IHandleStatusTurnAutoStepArgs args)
        {
            if (args.Timing != IKokoroApi.IV2.IStatusLogicApi.StatusTurnTriggerTiming.TurnEnd)
                return false;
            if (args.Amount <= 0)
                return false;
            if (args.Status != StirStatus.Status)
                return false;
            /*
             * TODO DOESNT CURRENTLY WORK, the heat gets changed after overheating
             * idea for fix?: changing the heatTrigger to += stirstatus
             */
            args.Combat.QueueImmediate(
                new AStatus
                {
                    status = Status.heat,
                    statusAmount = -args.Ship.Get(StirStatus.Status),
                    targetPlayer = args.Ship.isPlayerShip,
                    timer = 0
                }
            );
            
            args.Amount--;
            
            return false;
        }
    }
}