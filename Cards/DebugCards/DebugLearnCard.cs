﻿using System.Collections.Generic;
using System.Reflection;
using Rosseta.External;
using Nanoray.PluginManager;
using Nickel;
using Rosseta.Actions;
using Rosseta.StatusManagers;

namespace Rosseta.Cards.DebugCards;

public class DebugLearnCard : Card, IRegisterable
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
                dontOffer = true,
                unreleased = true,
                upgradesTo = [Upgrade.A, Upgrade.B]
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "DebugLearnCard", "name"]).Localize,
            // Art = ModEntry.RegisterSprite(package, "assets/Cards/Ponder.png").Sprite
        });
    }

    public override List<CardAction> GetActions(State s, Combat c)
    {
        return
        [
            new ALearnSpell
            {
                Amount = 3
            }
        ];
    }

    public override CardData GetData(State state)
    {
        return new CardData
        {
            cost = 0,
            infinite = true,
            description = string.Format(ModEntry.Instance.Localizations.Localize(["card", "DebugLearnCard", "desc"]))
        };
    }
}
