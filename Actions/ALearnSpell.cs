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

        public int amount = 3;

        private static Random rng = new Random();

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
            List<Card> Cards = new List<Card>();
            foreach (var artifact in g.state.EnumerateAllArtifacts())
            {
                if (artifact is SpellBook spellBook)
                {
                    Cards = spellBook.UnLearnedAcidSpells;
                    Cards.AddRange(spellBook.UnLearnedFireSpells);
                    Cards.AddRange(spellBook.UnLearnedAirSpells);
                    Cards.AddRange(spellBook.UnLearnedSpecialSpells);
                    Cards.AddRange(spellBook.UnLearnedIceSpells);
                }
            }
            List<Card> randomized = Cards.OrderBy(_ => rng.Next()).ToList();
            ModEntry.Instance.Logger.LogInformation(randomized.Count().ToString());
            List<Card> options = new List<Card>();

            if(amount > randomized.Count())
            {
                amount = randomized.Count();
            }

            for (int i = 0; i < amount ; i++)
            {
                options.Add(randomized[i].CopyWithNewId());
            }

            return new LearnSpell
            {
                cards = options.Select(c =>
                {
                    c.drawAnim = 1;
                    c.flipAnim = 1;
                    return c;
                }).ToList(),
                canSkip = CanSkip,
            };
        }

        public override List<Tooltip> GetTooltips(State s)
            => [
                new TTGlossary("action.cardOffering")
            ];

        private static void CardReward_TakeCard_Postfix(CardReward __instance, G g, Card card)
        {
            if (__instance is not LearnSpell custom)
                return;

            foreach (var artifact in g.state.EnumerateAllArtifacts())
            {
                if (artifact is SpellBook spellBook)
                {
                    if (ModEntry.RossetaFireSpellCardTypes.Contains(card.GetType()))
                    {
                        spellBook.LearnedFireSpells.Add(card);
                        for (int i = 0; i < spellBook.UnLearnedFireSpells.Count; i++)
                        {
                            if (spellBook.UnLearnedFireSpells.ElementAt(i).GetType() == card.GetType())
                            {
                                spellBook.UnLearnedFireSpells.RemoveAt(i); break;
                            }
                        }
                    }
                    if (ModEntry.RossetaAcidSpellCardTypes.Contains(card.GetType())) 
                    {
                        spellBook.LearnedAcidSpells.Add(card);
                        for (int i = 0; i < spellBook.UnLearnedAcidSpells.Count; i++)
                        {
                            if (spellBook.UnLearnedAcidSpells.ElementAt(i).GetType() == card.GetType())
                            {
                                spellBook.UnLearnedAcidSpells.RemoveAt(i); break;
                            }
                        }
                    }
                    if (ModEntry.RossetaAirSpellCardTypes.Contains(card.GetType())) 
                    {
                        spellBook.LearnedAirSpells.Add(card);
                        for (int i = 0; i < spellBook.UnLearnedAirSpells.Count; i++)
                        {
                            if (spellBook.UnLearnedAirSpells.ElementAt(i).GetType() == card.GetType())
                            {
                                spellBook.UnLearnedAirSpells.RemoveAt(i); break;
                            }
                        }
                    }
                    if (ModEntry.RossetaIceSpellCardTypes.Contains(card.GetType())) 
                    {
                        spellBook.LearnedIceSpells.Add(card);
                        for (int i = 0; i < spellBook.UnLearnedIceSpells.Count; i++)
                        {
                            if (spellBook.UnLearnedIceSpells.ElementAt(i).GetType() == card.GetType())
                            {
                                spellBook.UnLearnedIceSpells.RemoveAt(i); break;
                            }
                        }
                    }
                    if (ModEntry.RossetaSpecialSpellCardTypes.Contains(card.GetType())) 
                    {
                        spellBook.LearnedSpecialSpells.Add(card);
                        for (int i = 0; i < spellBook.UnLearnedSpecialSpells.Count; i++)
                        {
                            if (spellBook.UnLearnedSpecialSpells.ElementAt(i).GetType() == card.GetType())
                            {
                                spellBook.UnLearnedSpecialSpells.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }
            }
            
        }
        public sealed class LearnSpell : CardReward
        {

        }
    }
}