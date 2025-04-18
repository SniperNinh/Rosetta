using HarmonyLib;
using Rosseta.Artifacts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rosseta.Actions
{
    public sealed class ALearnSpell : CardAction
    {
        public bool CanSkip { get; set; } = true;
        
        public int Amount = 3;
        
        private static Rand rng = new Rand();

        public BattleType battleType = BattleType.Normal;
        
        internal static void ApplyPatches(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.DeclaredMethod(typeof(CardReward), nameof(CardReward.TakeCard)),
                postfix: new HarmonyMethod(typeof(ALearnSpell), nameof(CardReward_TakeCard_Postfix))
            );
        }
        
        public override Route? BeginWithRoute(G g, State s, Combat c)
        {
            timer = 0;
            
            List<Card> cards = new List<Card>();
            
            if (s.EnumerateAllArtifacts().OfType<SpellBook>().FirstOrDefault() is { } spellBook)
            {
                cards = spellBook.UnLearnedSpells;
                if (Amount > spellBook.UnLearnedSpells.Count) Amount = spellBook.UnLearnedSpells.Count;
            }
            
            if (Amount <= 0) return null;
            
            List<Card> options = new List<Card>();
            
            int num1 = 100;
            int num2 = 0;
            
            while (options.Count < Amount && num2++ < num1) 
            {
                var card = cards[rng.NextInt() % cards.Count];
                
                Rarity rarity = CardReward.GetRandomRarity(rng, battleType);
                
                CardMeta meta = card.GetMeta();
                
                if (meta.rarity != rarity) continue;
                if (card is IIsDebugSpell) continue;
                if (options.Contains(card)) continue;
                
                options.Add(card);
            }
            
            
            return new LearnSpell
            {
                cards = options.Select(card =>
                {
                    card.drawAnim = 1;
                    card.flipAnim = 1;
                    return card.CopyWithNewId();
                }).ToList(),
                canSkip = CanSkip,
            };
        }
        
        public override List<Tooltip> GetTooltips(State s)
            =>
            [
                new TTGlossary("action.cardOffering")
            ];
        
        private static void CardReward_TakeCard_Postfix(CardReward __instance, G g, Card card)
        {
            if (__instance is not LearnSpell custom)
            {
                return;
            }
            
            if (g.state.EnumerateAllArtifacts().OfType<SpellBook>().FirstOrDefault() is { } spellBook)
            {
                spellBook.LearnedSpells.Add(card);
                for (int i = 0; i < spellBook.UnLearnedSpells.Count; i++)
                {
                    if (spellBook.UnLearnedSpells.ElementAt(i).GetType() == card.GetType())
                    {
                        spellBook.UnLearnedSpells.RemoveAt(i); break;
                    }
                }
                ModEntry.Instance.Logger.LogInformation("updated spellbook.LearnedSpells, it is now " + spellBook.LearnedSpells.Count);
                ModEntry.Instance.Logger.LogInformation("updated spellbook.UnLearnedSpells, it is now " + spellBook.UnLearnedSpells.Count);
            }
        }
        
    }
    
    public sealed class LearnSpell : CardReward
    {
        
    }
}