using System.Collections.Generic;
using System.Reflection;
using Rosseta.External;
using Nanoray.PluginManager;
using Nickel;
using Rosseta.StatusManagers;

namespace Rosseta.Cards.Rosseta;

public class ManaFlask : Card, IRegisterable
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
            Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "ManaFlask", "name"]).Localize,
            // Art = ModEntry.RegisterSprite(package, "assets/Cards/Ponder.png").Sprite
        });
    }

    public override List<CardAction> GetActions(State s, Combat c)
    {
        return
        [
            new AAddCard()
            {
                destination = upgrade switch
                {
                    Upgrade.B => CardDestination.Discard,
                    _ => CardDestination.Hand,
                },
                card = new ManaBottle(),
                amount = upgrade switch
                {
                    Upgrade.B => 2,
                    _ => 1
                }
                
            }
        ];
    }

    public override CardData GetData(State state) 
        => upgrade switch
        {
            Upgrade.A => new CardData()
            {
                artOverlay = ModEntry.Instance.RossetaCommonOverlay,
                cost = 3,
                description = string.Format(ModEntry.Instance.Localizations.Localize(["card", "ManaFlask", "desc"]))
            },
            _ => new CardData()
            {
                artOverlay = ModEntry.Instance.RossetaCommonOverlay,
                cost = 1,
                exhaust = true,
                description = string.Format(ModEntry.Instance.Localizations.Localize(["card", "ManaFlask", "desc"]))
            }
        };
}
