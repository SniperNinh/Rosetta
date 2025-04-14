using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Rosseta.Cards;

public class CrystalSpell : Card, IRegisterable
{
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        helper.Content.Cards.RegisterCard(new CardConfiguration
        {
            CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
            Meta = new CardMeta
            {
                deck = ModEntry.Instance.RossetaDeck.Deck,
                rarity = Rarity.rare,
                upgradesTo = [Upgrade.A, Upgrade.B]
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "CrystalSpell", "name"]).Localize,
            // Art = ModEntry.RegisterSprite(package, "assets/Cards/Ponder.png").Sprite
        });
    }

    public override List<CardAction> GetActions(State s, Combat c)
    {
        List<CardAction> actions = new List<CardAction>()
        {
            new AStatus()
            {
                targetPlayer = true,
                status = Status.shard,
                statusAmount = (s.ship.Get(Status.shard) * -1)
            }
        };
        for (int index = 0; index < s.ship.Get(Status.shard); ++index)
        {
            List<CardAction> cardActionList = actions;
            AAttack shot = new AAttack()
            {
                damage = s.ship.Get(Status.maxShard),
                omitFromTooltips = true
            };
            
            cardActionList.Add(shot);
        }
        return actions;
    }

    public override CardData GetData(State state)
    {
        return new CardData
        {
            cost = 3,
            exhaust = true,
            description = string.Format(ModEntry.Instance.Localizations.Localize([
            "card",
            "CrystalSpell",
            "desc"], new { amount_color = state.ship.Get(Status.shard) > 0 ? "<c=textMain>" : "<c=redd>", amount = state.ship.Get(Status.shard), damage = state.ship.Get(Status.maxShard) }))
        };
    }
}