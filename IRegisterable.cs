using Nanoray.PluginManager;
using Nickel;

namespace Rosseta;

internal interface IRegisterable
{
    static abstract void Register(IPluginPackage<IModManifest> package, IModHelper helper);
}