# Chapter 02 - Card Basics

## Overview

Most cards are defined by their name, actions, traits, and description, should they have one.
Due to the relationship between cards and their actions, actions will be covered first in this chapter.

## CardActions

CardActions act as a singular unit of something happening.
Attacking, deploying a midrow object, gaining a status, and instantly moving are some examples of actions.
Custom CardActions can be created, should the desired effect not exist from existing actions.
Custom CardActions have the option to change the screen the game is on, such as for a card browse or card selection.

AGainPonder is an example of a custom action in DemoMod.
It adds Ponders to the requested deck, along with the iconography and tooltips.

## Cards

Cards must be registered, otherwise the game will crash. However, the crash only occurs after all mods have loaded, allowing for some liberty in the order in which things are done.
In a card's registration, its name, deck, and upgradesTo should be defined.
It is advised to give every card upgrades, due to the potential for them to be permanently added via CAT's Summon Control, or mid-combat via Johnson's cards.

Cards must also provide a list of actions.
These actions are what are rendered on the card, as well as what is put on the queue when it is played.

Cards must also define its CardData.
This data includes the card's cost, if it exhausts, retains, is temporary, is flippable, is floppable, its art and tint, etc.
Flippable means that a card may be flipped, flipping the direction of its instant movements and offset midrow deployments.
Floppable means that a card has two halves.
Both assign the flipped field of the card, so it may be detected in GetActions.

By default, a card's art is tinted with the color of the deck it is assigned to.

The middle line in floppable cards is a lie - it is a combination of having the line baked into the art, and having an ADummyAction to take up space.

## Supplementary Reading

It is advised to read the following cards, in the following order:
* [LessonPlan.cs](./../Cards/LessonPlan.cs) - very simple card
* [Ponder.cs](./../Cards/Ponder.cs) - different simple card
* [DeepStudy.cs](./../Cards/DeepStudy.cs) - X-hinted card

More advanced topics, which may be moved to another chapter:
* [PatternBlock.cs](./../Cards/PatternBlock.cs) - Conditional card

Niche knowledge, which is very rarely employed:
* [ExtractKnowledge.cs](./../Cards/ExtractKnowledge.cs) - On kill effect