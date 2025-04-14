using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using static CardBrowse;

namespace Rosseta.StatusManagers
{
    [HarmonyPatch]
    internal class StatusController
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Ship), nameof(Ship.MiscEffects.Overheat))]
        public static void ManaSpillHandler(Ship ship, Combat c)
        {
            if (ship.Get(ModEntry.Instance.ManaSpill.Status) > 0)
            {
                int currentManaSpill = ship.Get(ModEntry.Instance.ManaSpill.Status);
                if (ship.Get(ModEntry.Instance.Mana.Status) > 0)
                {
                    int currentMana = ship.Get(ModEntry.Instance.Mana.Status);
                    ship.Set(ModEntry.Instance.Mana.Status, currentMana - currentManaSpill);
                }
                
            }
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Ship), nameof(Ship.OnBeginTurn))]
        public static void ManaHandler(Ship ship, Combat c)
        {
            if (ship.Get(ModEntry.Instance.ManaSpill.Status) > 0)
            {
                int currentMana = ship.Get(ModEntry.Instance.Mana.Status);
                int currentManaSpill = ship.Get(ModEntry.Instance.ManaSpill.Status);
                ship.Set(ModEntry.Instance.Mana.Status, currentMana - currentManaSpill);
            }
            
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Ship), nameof(Ship.OnBeginTurn))]
        public static void StirHandler(Ship ship, Combat c)
        {
            if (ship.Get(ModEntry.Instance.Stir.Status) > 0)
            {
                int currentStir = ship.Get(ModEntry.Instance.Stir.Status);
                ship.Set(ModEntry.Instance.Mana.Status, currentStir - 1);
                int currentHeat = ship.Get(Status.heat);
                ship.Set(ModEntry.Instance.Mana.Status, currentHeat - 1);
            }
        }
        
        
    }
}

