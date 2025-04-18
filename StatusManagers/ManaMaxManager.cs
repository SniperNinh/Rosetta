using HarmonyLib;
using Nanoray.PluginManager;
using Nickel;
using System;
using System.Reflection;
using Rosseta.External;

namespace Rosseta.StatusManagers;

internal sealed class ManaMaxStatusManager : IRegisterable
{
    internal static IStatusEntry ManaMax { get; private set; } = null!;

    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
    ManaMax = ModEntry.Instance.Helper.Content.Statuses.RegisterStatus("ManaMax", new StatusConfiguration
        {
            Definition = new StatusDef
            {
                isGood = true,
                affectedByTimestop = true,
                color = new("23EEB6"),
                icon = ModEntry.Instance.Helper.Content.Sprites.RegisterSprite(ModEntry.Instance.Package.PackageRoot.GetRelativeFile("assets/MaxManaStatus.png")).Sprite,
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["status", "ManaMax", "name"]).Localize,
            Description = ModEntry.Instance.AnyLocalizations.Bind(["status", "ManaMax", "desc"]).Localize
        });
    }
}