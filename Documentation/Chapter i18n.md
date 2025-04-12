# Chapter ?? - Internationalization (i18n)

Internationalization allows for easy translations of mods.
Mods that utilize Nickel's available internationalization features typically have an i18n folder.

Within this folder, each locale is given a json file - most of the time, only an en.json is present, as most mods are untranslated.

## File Structure

Localization files are one large JSON object, with many objects nested within.
When using localization, you are asked to provide an array of strings, which acts as the reference of how to look into this JSON object.

Let's say you are looking for the key `["card", "Ponder", "name"]`. You would look at the JSON object as a whole, then find the "card" object.
From within the "card" object, you would look for the "Ponder" object, and from it, the "name" field.
Since the "name" is the last thing being looked for, and it is a string, it is the value obtained.

Localizations can be as many, or as few, keys in the array. It is therefore advised to organize it in a reasonable fashion.