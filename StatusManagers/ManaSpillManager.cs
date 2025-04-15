using HarmonyLib;
using Nanoray.PluginManager;
using Nickel;
using System;
using System.Reflection;
using Rosseta.External;

namespace Rosseta.StatusManagers;

internal sealed class ManaSpillManager : IRegisterable
{
    internal static IStatusEntry ManaStatus { get; private set; } = null!;

    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
    ManaStatus = ModEntry.Instance.Helper.Content.Statuses.RegisterStatus("Mana", new()
        {
            Definition = new()
            {
                icon = ModEntry.Instance.Helper.Content.Sprites.RegisterSprite(ModEntry.Instance.Package.PackageRoot.GetRelativeFile("assets/Statuses/Entanglement.png")).Sprite,
                color = new("23EEB6"),
                isGood = true,
                affectedByTimestop = true,
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["status", "Mana", "name"]).Localize,
            Description = ModEntry.Instance.AnyLocalizations.Bind(["status", "Mana", "description"]).Localize
        });

        ModEntry.Instance.KokoroApi.StatusLogic.RegisterHook(new StatusLogicHook());

        ModEntry.Instance.Harmony.Patch(
            original: AccessTools.DeclaredMethod(typeof(AMove), nameof(AMove.Begin)),
            postfix: new HarmonyMethod(MethodBase.GetCurrentMethod()!.DeclaringType!, nameof(AMove_Begin_Postfix))
        );
    }

    private static void AMove_Begin_Postfix(AMove __instance, State s, Combat c)
    {
        if (__instance.dir == 0)
            return;

        var ManaDepth = ModEntry.Instance.Helper.ModData.GetModDataOrDefault<int>(__instance, "ManaDepth");
        if (ManaDepth >= 2)
            return;

        var ship = __instance.targetPlayer ? s.ship : c.otherShip;
        var Mana = ship.Get(ManaStatus.Status);
        if (Mana == 0)
            return;

        var action = new AMove { targetPlayer = !__instance.targetPlayer, dir = -__instance.dir * Math.Sign(Mana), statusPulse = ManaStatus.Status };
        ModEntry.Instance.Helper.ModData.CopyAllModData(__instance, action);
        ModEntry.Instance.Helper.ModData.SetModData(action, "ManaDepth", ManaDepth + 1);
        c.QueueImmediate(action);
    }

    private sealed class StatusLogicHook : IKokoroApi.IV2.IStatusLogicApi.IHook
    {
        public bool HandleStatusTurnAutoStep(IKokoroApi.IV2.IStatusLogicApi.IHook.IHandleStatusTurnAutoStepArgs args)
        {
            if (args.Status != ManaStatus.Status)
                return false;
            if (args.Timing != IKokoroApi.IV2.IStatusLogicApi.StatusTurnTriggerTiming.TurnStart)
                return false;
            if (args.Amount <= 0)
                return false;
            
            args.Amount--;
            return false;
        }
    }
}