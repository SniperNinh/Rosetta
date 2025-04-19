﻿using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Rosseta.External;
using Nanoray.PluginManager;
using Nickel;
using Rosseta.Actions;
using Rosseta.Artifacts;

namespace Rosseta.Cards.Rosseta;

public class DrawFireSpell : Card, IRegisterable
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
            Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "DrawFireSpell", "name"]).Localize,
            // Art = ModEntry.RegisterSprite(package, "assets/Cards/Ponder.png").Sprite
        });
    }

    public override List<CardAction> GetActions(State s, Combat c)
    {
        
        List<CardAction> actions = new List<CardAction>();
        if (s.EnumerateAllArtifacts().OfType<SpellBook>().FirstOrDefault() is { } spellBook)
            actions.Add(
                new ASpecificCardTypeOffering()
                {
                    Cards = GetSpellTypeCardsFromSpellBook(spellBook),
                    Destination = CardDestination.Hand
                }
            );
        
        return actions;
    }

    public override CardData GetData(State state)
    {
        return new CardData
        {
            artOverlay = ModEntry.RegisterSprite(ModEntry.Instance.Package, "assets/Border_Common_Rosseta.png").Sprite,
            cost = 1,
            description = string.Format(ModEntry.Instance.Localizations.Localize(["card", "DrawFireSpell", "desc"]))
        };
    }
    private List<Card> GetSpellTypeCardsFromSpellBook(SpellBook spellBook)
    {
        List<Card> elementCardList = new List<Card>();
        foreach (var elementCard in spellBook.LearnedSpells)
        {
            if (elementCard is not IFireCard) continue;
            elementCardList.Add(elementCard);
        }
        return elementCardList.Count > 0 ? elementCardList : spellBook.DebugSpells;
    }
}
