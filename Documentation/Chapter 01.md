# Chapter 01 - The ModEntry and Registration

## Purpose of ModEntry

To create a mod, you must create a class that inherits from SimpleMod.
Its constructor acts as the entry point to all of your code.

Within the constructor, several important actions take place:
* The statically available instance is assigned
* Harmony, required APIs, and (some) optional APIs are obtained from the helper
* Localization is initialized
* Things that must be registered, are registered

## Harmony and APIs

Harmony allows you to patch existing methods in the base game, allowing you to add new behavior, or even modify existing behavior.
Instantiating an instance of Harmony in the ModEntry allows for easy modification of all of your Harmony uses, should you want to change its key or the like.
Harmony is covered in more detail in Chapter ??.

Many mods provide an API, with which you can interact with them.
Dependencies can be set on the nickel.json, ensuring that they will be loaded before your mod.
In doing so, you will then be able to put them into a variable immediately, which is good if they are used widely within your mod.
Kokoro, DemoMod's only dependency, is a library mod offering a variety of useful things.

## Localization

Localization allows for easy translations of mods.
No user-facing strings are baked into the code, instead references to a localization file are used.
These references then go to the currently loaded locale.

AnyLocalizations allows for a Locale -> String mapping.
Localizations allows for () -> String mapping, using the current Locale.

## Registration

Some things must be registered, because they are used by the game's database (DB).
Using something not registered to the database will either result in a crash, or other undefined behavior.

Sprites are what allows things to be seen on the screen.
They are essential.

Decks are used to tie cards, artifacts, and other things to an identity.
A deck is not inherently a character, but one typically follows the other.
Examples of decks that do not have a character are the ship cards (Ares and Jupiter), some event cards (Ephemeral, Soggins, Dracula), trash, and Abyssal Visions even has its own deck.

Animations must also be registered, and two of them even must either be registered before, or during, character registration.
The "neutral" and "mini" animations must be registered, else the game will not allow it.
The "squint" and "gameover" animations are optional, but highly advised, as they are used by the base game.
All other animations are optional, being invoked only when dialogue dictates.

Characters allow you to play with the cards and artifacts they've been tied to.
In addition to a starting deck of cards, artifacts may be defined as part of their starting kit.

Statuses are very helpful for allowing the creation of mechanics.
However, they only act as numbers visible to the player, with code and meaning managed elsewhere.