using Nanoray.PluginManager;
using Nickel;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Rosseta.Artifacts;
using Rosseta.External;
using System.Linq;


namespace Rosseta.StatusManagers;

internal sealed class ManaStatusManager : IRegisterable
{
    internal static IStatusEntry ManaStatus { get; private set; } = null!;

    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
    ManaStatus = ModEntry.Instance.Helper.Content.Statuses.RegisterStatus("Mana", new StatusConfiguration
        {
            Definition = new StatusDef
            {
                isGood = true,
                affectedByTimestop = true,
                color = new("23EEB6"),
                icon = ModEntry.Instance.Helper.Content.Sprites.RegisterSprite(ModEntry.Instance.Package.PackageRoot.GetRelativeFile("assets/Artifact/LexiconA.png")).Sprite,
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["status", "Mana", "name"]).Localize,
            Description = ModEntry.Instance.AnyLocalizations.Bind(["status", "Mana", "desc"]).Localize
        });
        
        ModEntry.Instance.KokoroApi.StatusRendering.RegisterHook(new StatusRenderingHook());
        
        ModEntry.Instance.KokoroApi.StatusLogic.RegisterHook(new StatusLogicHook());
        
    }

    private sealed class StatusRenderingHook : IKokoroApi.IV2.IStatusRenderingApi.IHook
    {
        public (IReadOnlyList<Color> Colors, int? BarSegmentWidth)? OverrideStatusRenderingAsBars(IKokoroApi.IV2.IStatusRenderingApi.IHook.IOverrideStatusRenderingAsBarsArgs args)
        {
            if (args.Status != ManaStatus.Status) return null;
            
            var ship = args.Ship;
            var s = args.State;
            var expected = GetManaLimit(ship, s);
            var current = ship.Get(ManaStatus.Status);
            
            var filled = Math.Min(expected, current);
            var empty = Math.Max(expected - current, 0);
            var overflow = Math.Max(current - expected, 0);
            
            return (Enumerable.Repeat(new Color("fbb954"), filled)
                .Concat(Enumerable.Repeat(new Color("7a3045"), empty)
                .Concat(Enumerable.Repeat(new Color("f78716"), overflow)))
                .ToImmutableList(),
                null);
        }
    }

    /*
     * To add functiality that activates on non turn start/end events you make a method like the one below.
     * This one activates when a ship overheats.
     * it checks what ship overheated and adds "ManaStatus" to the damage
     */
    
    private sealed class StatusLogicHook : IKokoroApi.IV2.IStatusLogicApi.IHook
    {
        public bool HandleStatusTurnAutoStep(IKokoroApi.IV2.IStatusLogicApi.IHook.IHandleStatusTurnAutoStepArgs args)
        {
            if (args.Status != ManaStatus.Status)
                return false;
            if (args.Timing != IKokoroApi.IV2.IStatusLogicApi.StatusTurnTriggerTiming.TurnStart)
                return false;
            if (args.Ship.Get(ManaSpillStatusManager.ManaSpillStatus.Status) <= 0)
                return false;
            
            
            if (args.Amount > 0)
            {
                args.Combat.QueueImmediate(
                    new AStatus
                    {
                        status = ManaStatus.Status,
                        statusAmount = -args.Ship.Get(ManaSpillStatusManager.ManaSpillStatus.Status),
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
    public static int GetManaLimit(Ship ship, State s)
    {
        return 10 + ship.Get(ManaMaxStatusManager.ManaMax.Status) - (s.EnumerateAllArtifacts().Contains(new ManaShelf()) ? 4 : 0);
    }
}