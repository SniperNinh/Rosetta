using HarmonyLib;
using Microsoft.Extensions.Logging;
using Nanoray.PluginManager;
using Nickel;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Rosseta.Actions;
using Rosseta.Artifacts;
using Rosseta.Cards.Rosseta;
using Rosseta.Cards.Spells;
using Rosseta.Cards.DebugCards;
using Rosseta.External;
using Rosseta.StatusManagers;

namespace Rosseta;

internal class ModEntry : SimpleMod
{
    internal static ModEntry Instance { get; private set; } = null!;
    internal Harmony Harmony;
    internal IKokoroApi.IV2 KokoroApi;
    internal IDeckEntry RossetaSpellDeck;
    internal IDeckEntry RossetaDeck;
    internal ILocalizationProvider<IReadOnlyList<string>> AnyLocalizations { get; }
    internal ILocaleBoundNonNullLocalizationProvider<IReadOnlyList<string>> Localizations { get; }
    internal Spr RossetaCommonOverlay;
    internal Spr RossetaUncommonOverlay;
    internal Spr RossetaRareOverlay;
    internal Spr RossetaSpecialOverlay;
    internal Spr RossetaSpellCommonOverlay;
    internal Spr RossetaSpellUncommonOverlay;
    internal Spr RossetaSpellRareOverlay;
    internal Spr RossetaSpellSpecialOverlay;
    
    /*
     * The following lists contain references to all types that will be registered to the game.
     * All cards and artifacts must be registered before they may be used in the game.
     * In theory only one collection could be used, containing all registrable types, but it is seperated this way for ease of organization.
     */
    internal static List<Type> RossetaCommonCardTypes = [
        typeof(BottleRepair),
        typeof(DistilledMana),
        typeof(DrawFireSpell),
        typeof(DrawIceSpell),
        typeof(DrawAirSpell),
        typeof(Equivalence),
        typeof(ManaFlask),
        typeof(ManaShield),
        typeof(WandStab)
    ];
    internal static List<Type> RossetaUncommonCardTypes = [
        typeof(BottleThrow),
        typeof(CrystallineBottle),
        typeof(DrawAcidSpell),
        typeof(GlassPack), 
        typeof(SpellSheet),
        typeof(Stir)
    ];
    internal static List<Type> RossetaRareCardTypes = [
        typeof(DrawSpecialSpell)
    ];
    internal static List<Type> RossetaSpecialCardTypes = [
        typeof(ManaBottle),
        typeof(Brooooom)
    ];
    internal static List<Type> RossetaDebugCardTypes = [
        typeof(SpellDrawTest),
        typeof(DebugLearnAllSpells),
        typeof(DebugLearnCard)
    ];
    
    internal static IEnumerable<Type> RossetaCardTypes =
        RossetaCommonCardTypes
            .Concat(RossetaUncommonCardTypes)
            .Concat(RossetaRareCardTypes)
            .Concat(RossetaSpecialCardTypes)
            .Concat(RossetaDebugCardTypes);
    
    internal static List<Type> RossetaCommonSpellCardTypes = [
        typeof(IceBolt),
        typeof(IceShield),
        typeof(FireBolt),
        typeof(FireryCrystals),
        typeof(ShockWave),
        typeof(AirDash)
    ];
    
    internal static List<Type> RossetaUncommonSpellCardTypes = [
        typeof(ConjureIceTea),
        typeof(AcidSplash),
        typeof(DragonArmor),
        typeof(CurePoison),
        typeof(Fakeout),
    ];
    
    internal static List<Type> RossetaRareSpellCardTypes = [
        typeof(FreezeRay),
        typeof(FireFlash),
        typeof(FireBall),
        typeof(AirSuperDash)
    ];
    
    internal static List<Type> RossetaDebugSpellCardTypes = [
        typeof(SpellDebugCard)
    ];

    internal static IEnumerable<Type> RossetaSpellCardTypes =
        RossetaCommonSpellCardTypes
            .Concat(RossetaUncommonSpellCardTypes)
            .Concat(RossetaRareSpellCardTypes)
            .Concat(RossetaDebugSpellCardTypes);
    
    internal static List<Type> RossetaCommonArtifacts = [
        typeof(Broom),
        typeof(ManaShelf)
    ];
    
    internal static List<Type> RossetaBossArtifacts = [
        typeof(Cauldron),
        typeof(SpellBook),
        typeof(SpellScroll),
        typeof(BookShelf)
    ];
    internal static IEnumerable<Type> RossetaArtifactTypes =
        RossetaCommonArtifacts
            .Concat(RossetaBossArtifacts);
    
    internal static IEnumerable<Type> AllRegisterableTypes = [
        .. RossetaCardTypes,
        .. RossetaSpellCardTypes,
        .. RossetaArtifactTypes,
        typeof(ManaStatusManager),
        typeof(ManaSpillStatusManager),
        typeof(ManaMaxStatusManager)
    ];
    
    public ModEntry(IPluginPackage<IModManifest> package, IModHelper helper, ILogger logger) : base(package, helper, logger)
    {
        Instance = this;
        Harmony = new Harmony("Sniperninh.Rosseta");
        
        ALearnSpell.ApplyPatches(Harmony);
        /*
         * Some mods provide an API, which can be requested from the ModRegistry.
         * The following is an example of a required dependency - the code would have unexpected errors if Kokoro was not present.
         * Dependencies can (and should) be defined within the nickel.json file, to ensure proper load mod load order.
         */
        KokoroApi = helper.ModRegistry.GetApi<IKokoroApi>("Shockah.Kokoro")!.V2;

        AnyLocalizations = new JsonLocalizationProvider(
            tokenExtractor: new SimpleLocalizationTokenExtractor(),
            localeStreamFunction: locale => package.PackageRoot.GetRelativeFile($"i18n/{locale}.json").OpenRead()
        );
        Localizations = new MissingPlaceholderLocalizationProvider<IReadOnlyList<string>>(
            new CurrentLocaleOrEnglishLocalizationProvider<IReadOnlyList<string>>(AnyLocalizations)
        );
        
        /*
         * A deck only defines how cards should be grouped, for things such as codex sorting and Second Opinions.
         * A character must be defined with a deck to allow the cards to be obtainable as a character's cards.
         */
        RossetaDeck = helper.Content.Decks.RegisterDeck("Rosseta", new DeckConfiguration
        {
            Definition = new DeckDef
            {
                /*
                 * This color is used in a few places:
                 * TODO On cards, it dictates the sheen on higher rarities, as well as influences the color of the energy cost.
                 * If this deck is given to a playable character, their name will be this color, and their mini will have this color as their border.
                 */
                color = new Color("999999"),
                titleColor = new Color("000000")
            },
            
            DefaultCardArt = StableSpr.cards_colorless,
            BorderSprite = RegisterSprite(package, "assets/RossetaDeck/Border_Rosseta.png").Sprite,
            Name = AnyLocalizations.Bind(["character", "name"]).Localize
        });
        
        RossetaCommonOverlay = RegisterSprite(package, "assets/RossetaDeck/Overlay_Rosseta_Common.png").Sprite;
        RossetaUncommonOverlay = RegisterSprite(package, "assets/RossetaDeck/Overlay_Rosseta_Uncommon.png").Sprite;
        RossetaRareOverlay = RegisterSprite(package, "assets/RossetaDeck/Overlay_Rosseta_Rare.png").Sprite;
        RossetaSpecialOverlay = RegisterSprite(package, "assets/RossetaDeck/Overlay_Rosseta_Special.png").Sprite;
        
        
        RossetaSpellDeck = helper.Content.Decks.RegisterDeck("RossetaSpells", new DeckConfiguration
        {
            
            Definition = new DeckDef
            {
                /*
                 * This color is used in a few places:
                 * TODO On cards, it dictates the sheen on higher rarities, as well as influences the color of the energy cost.
                 * If this deck is given to a playable character, their name will be this color, and their mini will have this color as their border.
                 */
                color = new Color("999999"),
                titleColor = new Color("000000")
            },
            DefaultCardArt = StableSpr.cards_colorless,
            ShineColorOverride = args => DB.decks[args.Card.GetMeta().deck].color.normalize().gain(1), /* TODO Working on removing card tint */
            BorderSprite = RegisterSprite(package, "assets/SpellDeck/Border_Rosseta_Spell.png").Sprite,
            Name = AnyLocalizations.Bind(["SpellDeck", "name"]).Localize
        });
        
        RossetaSpellCommonOverlay = RegisterSprite(package, "assets/SpellDeck/Overlay_Rosseta_Spell_Common.png").Sprite;
        RossetaSpellUncommonOverlay = RegisterSprite(package, "assets/SpellDeck/Overlay_Rosseta_Spell_Uncommon.png").Sprite;
        RossetaSpellRareOverlay = RegisterSprite(package, "assets/SpellDeck/Overlay_Rosseta_Spell_Rare.png").Sprite;
        RossetaSpellSpecialOverlay = RegisterSprite(package, "assets/SpellDeck/Overlay_Rosseta_Spell_Special.png").Sprite;
        
        
        /*
         * All the IRegisterable types placed into the static lists at the start of the class are initialized here.
         * This snippet invokes all of them, allowing them to register themselves with the package and helper.
         */
        foreach (var type in AllRegisterableTypes)
            AccessTools.DeclaredMethod(type, nameof(IRegisterable.Register))?.Invoke(null, [package, helper]);
        
        /*
         * Characters have required animations, recommended animations, and you have the option to add more.
         * In addition, they must be registered before the character themselves is registered.
         * The game requires you to have a neutral animation and mini animation, used for normal gameplay and the map and run start screen, respectively.
         * The game uses the squint animation for the Extra-Planar Being and High-Pitched Static events, and the gameover animation while you are dying.
         * You may define any other animations, and they will only be used when explicitly referenced (such as dialogue).
         */
        RegisterAnimation(package, "neutral", "assets/Animation/DaveNeutral", 4);
        RegisterAnimation(package, "squint", "assets/Animation/DaveSquint", 4);
        Instance.Helper.Content.Characters.V2.RegisterCharacterAnimation(new CharacterAnimationConfigurationV2
        {
            CharacterType = RossetaDeck.Deck.Key(),
            LoopTag = "gameover",
            Frames = [
                RegisterSprite(package, "assets/Animation/DaveGameOver.png").Sprite,
            ]
        });
        Instance.Helper.Content.Characters.V2.RegisterCharacterAnimation(new CharacterAnimationConfigurationV2
        {
            CharacterType = RossetaDeck.Deck.Key(),
            LoopTag = "mini",
            Frames = [
                RegisterSprite(package, "assets/Animation/DaveMini.png").Sprite,
            ]
        });

        helper.Content.Characters.V2.RegisterPlayableCharacter("Rosseta", new PlayableCharacterConfigurationV2
        {
            Deck = RossetaDeck.Deck,
            BorderSprite = RegisterSprite(package, "assets/char_frame_Rosseta.png").Sprite,
            Starters = new StarterDeck
            {
                cards = [
                    new WandStab(),
                    new DrawAirSpell()
                ],
                /*
                 * Some characters have starting artifacts, in addition to starting cards.
                 * This is where they would be added, much like their starter cards.
                 * This can be safely removed if you have no starting artifacts.
                 */
                artifacts = [
                    new SpellBook()
                ]
            },
            Description = AnyLocalizations.Bind(["character", "desc"]).Localize
        });
    }
    
    /*
     * assets must also be registered before they may be used.
     * Unlike cards and artifacts, however, they are very simple to register, and often do not need to be referenced in more than one place.
     * This utility method exists to easily register a sprite, but nothing prevents you from calling the method used yourself.
     */
    public static ISpriteEntry RegisterSprite(IPluginPackage<IModManifest> package, string dir)
    {
        return Instance.Helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile(dir));
    }

    /*
     * Animation frames are typically named very similarly, only differing by the number of the frame itself.
     * This utility method exists to easily register an animation.
     * It expects the animation to start at frame 0, up to frames - 1.
     * TODO It is advised to avoid animations consisting of 2 or 3 frames.
     */
    public static void RegisterAnimation(IPluginPackage<IModManifest> package, string tag, string dir, int frames)
    {
        Instance.Helper.Content.Characters.V2.RegisterCharacterAnimation(new CharacterAnimationConfigurationV2
        {
            CharacterType = Instance.RossetaDeck.Deck.Key(),
            LoopTag = tag,
            Frames = Enumerable.Range(0, frames)
                .Select(i => RegisterSprite(package, dir + i + ".png").Sprite)
                .ToImmutableList()
        });
    }
}

