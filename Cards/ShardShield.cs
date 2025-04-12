using System;
using System.Collections.Generic;
using System.Reflection;
using Rosseta.External;
using Nanoray.PluginManager;
using Nickel;

namespace Rosseta.Cards;

public class ShardShield : Card, IRegisterable
{
    private static IKokoroApi.IV2.IConditionalApi Conditional => ModEntry.Instance.KokoroApi.Conditional;
    
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        helper.Content.Cards.RegisterCard(new CardConfiguration
        {
            CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
            Meta = new CardMeta
            {
                deck = ModEntry.Instance.RossetaDeck.Deck,
                rarity = Rarity.common,
                upgradesTo = [Upgrade.A, Upgrade.B]
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "ShardShield", "name"]).Localize,
        });
    }

    /*
     * Some cards have actions that don't just differ by number on upgrade.
     * In these cases, a switch statement may be used.
     * It is more verbose, but allows for precisely describing what each upgrade's actions are.
     */
    public override List<CardAction> GetActions(State s, Combat c)
    {
        return
        [
            new AStatus
            {
                status = Status.shield,
                statusAmount = 1,
                targetPlayer = true,
                shardcost = 1
            },
            new AStatus
            {
                status = Status.shard,
                statusAmount = 1,
                targetPlayer = true
            }
        ];
    }

    public override CardData GetData(State state)
    {
        return new CardData
        {
            cost = 1
        };
    }
}