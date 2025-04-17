using Nanoray.PluginManager;
using Nickel;

namespace Rosseta;

public class SpellCard : Card 
{
    public enum Element
    {
        FIRE,
        ICE,
        ACID,
        AIR
    }
    
    public required bool IsSpecialSpell = false;
    
}