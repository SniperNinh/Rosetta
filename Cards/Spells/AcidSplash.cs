using System;
using System.Collections.Generic;
using System.Reflection;
using Rosseta.External;
using Nanoray.PluginManager;
using Nickel;
using Rosseta.StatusManagers;

namespace Rosseta.Cards.Spells;

public class AcidSplash : Card, IRegisterable, IAcidCard
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
                rarity = Rarity.uncommon,
                dontOffer = true,
                upgradesTo = [Upgrade.A, Upgrade.B]
                
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "AcidSplash", "name"]).Localize,
            // Art = ModEntry.RegisterSprite(package, "assets/Cards/Ponder.png").Sprite
        });
    }

    public override List<CardAction> GetActions(State s, Combat c)
    {
        return
        [
            ModEntry.Instance.KokoroApi.ActionCosts.MakeCostAction(
                ModEntry.Instance.KokoroApi.ActionCosts.MakeResourceCost(
                    ModEntry.Instance.KokoroApi.ActionCosts.MakeStatusResource(ManaStatusManager.ManaStatus.Status),
                    5),
                new AStatus()
                {
                    status = Status.corrode,
                    statusAmount = 2,
                    targetPlayer = !s.ship.isPlayerShip
                }).AsCardAction
        ];
    }

    public override CardData GetData(State state)
    {
        return new CardData
        {
            artOverlay = ModEntry.Instance.RossetaSpellUncommonOverlay,
            cost = 0,
            exhaust = true,
            temporary = true
        };
    }
}
