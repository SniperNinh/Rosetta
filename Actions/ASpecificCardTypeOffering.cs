using HarmonyLib;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rosseta.Actions
{
    public sealed class ASpecificCardTypeOffering : CardAction
    {
        public List<Card> Cards { get; set; } = [];
        public bool CanSkip { get; set; } = true;
        public CardDestination Destination { get; set; } = CardDestination.Hand;
        
        internal static void ApplyPatches(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.DeclaredMethod(typeof(CardReward), nameof(CardReward.TakeCard)),
                postfix: new HarmonyMethod(typeof(ASpecificCardTypeOffering), nameof(CardReward_TakeCard_Postfix))
            );
        }

        public override Route? BeginWithRoute(G g, State s, Combat c)
        {
            timer = 0;
            List<Card> options = new List<Card>();
            for (int i = 0; i < Cards.Count; i++)
            {
                options.Add(Cards[i].CopyWithNewId());
            }
            return new CustomCardReward
            {
                cards = options.Select(C =>
                {
                    C.drawAnim = 1;
                    C.flipAnim = 1;
                    return C;
                }).ToList(),
                canSkip = CanSkip,
                Destination = Destination
            };
        }

        public override List<Tooltip> GetTooltips(State s)
            => [
                new TTGlossary("action.cardOffering")
            ];

        private static void CardReward_TakeCard_Postfix(CardReward __instance, G g, Card card)
        {
            if (__instance is not CustomCardReward)
                return;
            if (g.state.route is not Combat combat)
                return;

            g.state.RemoveCardFromWhereverItIs(card.uuid);
            combat.QueueImmediate(new AAddCard
            {
                card = card,
                amount = 1,
                destination = ((CustomCardReward)__instance).Destination
            });
        }

        public sealed class CustomCardReward : CardReward
        {
            public CardDestination Destination { get; set; } = CardDestination.Hand;
        }
    }
}