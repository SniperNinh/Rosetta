using System;
using System.Collections.Generic;
using System.Reflection;
using Rosseta.External;
using Nanoray.PluginManager;
using Nickel;
using Rosseta.StatusManagers;

namespace Rosseta.Cards.Spells;

public class FireBall : Card, IRegisterable, IFireCard, ISpecialCard
{
    private static IKokoroApi.IV2.IConditionalApi Conditional => ModEntry.Instance.KokoroApi.Conditional;

    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        helper.Content.Cards.RegisterCard(new CardConfiguration
        {
            
            CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
            Meta = new CardMeta
            {
                deck = ModEntry.Instance.RossetaSpellDeck.Deck,
                rarity = Rarity.rare,
                dontOffer = true,
                upgradesTo = [Upgrade.A, Upgrade.B]
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "FireBall", "name"]).Localize,
            // Art = ModEntry.RegisterSprite(package, "assets/Cards/Ponder.png").Sprite
        });
    }

    public override List<CardAction> GetActions(State s, Combat c)
    {
        return
        [
            new AVariableHint()
            {
                status = ManaStatusManager.ManaStatus.Status
            },
            new AAttack()
            {
                damage = s.ship.Get(ManaStatusManager.ManaStatus.Status),
                xHint = 1
            },
            new AStatus()
            {
                status = Status.heat,
                statusAmount = s.ship.Get(ManaStatusManager.ManaStatus.Status) % 2 == 1 ? (s.ship.Get(ManaStatusManager.ManaStatus.Status) - 1) / 2 : s.ship.Get(ManaStatusManager.ManaStatus.Status) / 2,
                targetPlayer = s.ship.isPlayerShip,
                xHint = 1
            },
            new AStatus()
            {
                status = ManaStatusManager.ManaStatus.Status,
                statusAmount = 0,
                mode = AStatusMode.Set,
                targetPlayer = s.ship.isPlayerShip
            }
        ];
    }

    public override CardData GetData(State state)
    {
        return new CardData
        {
            cost = 0,
            exhaust = true,
            temporary = true
        };
    }
}
