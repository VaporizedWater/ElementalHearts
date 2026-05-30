# Modded Boss Support
BossChecklist provides `Mod.Call` functionality to allow modders to provide info about their bosses, minibosses, and events to BossChecklist. `Mod.Call` allows for mods you communicate with each other and cooperate, more info on `Mod.Call` can be learned from the [tModLoader wiki](https://github.com/tModLoader/tModLoader/wiki/Expert-Cross-Mod-Content#call-aka-modcall-intermediate). If your mod has a boss, miniboss, or event that you would like to add to Boss Checklist's boss log, please use the appropriate `Mod.Call` detailed below. There are a few different Mod.Call patterns that are supported, but they use similar arguments. This guide will go through each `Mod.Call` pattern and corresponding arguments in detail below.

# Mod.Calls
All Mod.Call listed are expected to be used before Mod.AddRecipes, which means you should integrate with BossChecklist in Mod.PostSetupContent. See the complete examples below to see this in action.    
Some patterns have optional parameters, notated with `[parameter]`.

## Current
These `Mod.Call` patterns are from v1.0 update.
### AddBoss
`"AddBoss", float progression, int/List<int> BossNPCIDs, Mod mod, string bossName, Func<bool> downedBoss, int/List<int> SpawnItemIDs, int/List<int> CollectionItemIDs, int/List<int> LootItemIDs, [string spawnInfo], [string despawnMessage], [string texture], [string overrideHeadIconTexture], [Func<bool> bossAvailable]`
Adds a Boss to the listing. Read below for specifics on the parameters.
### AddMiniBoss
`"AddMiniBoss", float progression, int/List<int> miniBossNPCIDs, Mod mod, string minibossName, Func<bool> downedMiniBoss, int/List<int> SpawnItemIDs, int/List<int> CollectionItemIDs, int/List<int> LootItemIDs, [string spawnInfo], [string despawnMessage], [string texture], [string overrideHeadIconTexture], [Func<bool> miniBossAvailable]`
Adds a MiniBoss to the listing. 
### AddEvent
`"AddEvent", float progression, int/List<int> EventNPCIDs, Mod mod, string eventName, Func<bool> downedEvent, int/List<int> SpawnItemIDs, int/List<int> CollectionItemIDs, int/List<int> LootItemIDs, [string spawnInfo], [string despawnMessage], [string texture], [string overrideHeadIconTexture], [Func<bool> eventAvailable]`
Adds an Event to the listing. 
### AddToBossLoot
`"AddToBossLoot", string modName*, string bossName, List<int> itemIDs`    
Adds the provided items to the displayed drops for the specified boss.
### AddToBossCollection
`"AddToBossCollection", string modName*, string bossName, List<int> itemIDs`    
Adds the provided items to the displayed collection items for the specified boss.
### AddToBossSpawnItems
`"AddToBossSpawnItems", string modName*, string bossName, List<int> itemIDs`    
Adds the provided items to the list of spawn items for the specified boss.
### AddToEventNPCs
`"AddToEventNPCs", string modName*, string bossName, List<int> npcIDs`    
Adds the provided npcs to the displayed npcs for the specified event.

### GetBossInfoDictionary
`"GetBossInfoDictionary", Mod mod, "1.1"`
Returns a Dictionary that can be used to query various data about all registered bosses. See [BossChecklistIntegrationExample.cs](https://github.com/JavidPack/BossChecklist/blob/master/BossChecklistIntegrationExample.cs) for more information.

***If adding to vanilla Terraria, use the string "Terraria".**

## Obsolete 
These `Mod.Call` patterns are from v0.2.1 and earlier and should be avoided. You should update to the newer patterns to support more features of BossChecklist, your users will appreciate it. If you are updating to newer patterns, you can consult these to see how arguments match up. Take note that the order of arguments differs for the updated version of these patterns.

### AddBoss
`"AddBoss", string bossName, float progression, Func<bool> bossDowned`    
### AddBossWithInfo
`"AddBossWithInfo", string bossName, float progression, Func<bool> bossDowned, string bossInfo, [Func<bool> bossAvailable]`
### AddMiniBossWithInfo
`"AddMiniBossWithInfo", string minibossName, float progression, Func<bool> minibossDowned, string minibossInfo, [Func<bool> minibossAvailable]`
### AddEventWithInfo
`"AddEventWithInfo", string eventName, float progression, Func<bool> eventCompleted, string eventInfo, [Func<bool>eventAvailable]`

# Arguments

## 1.) Entry Type - `string`
The first argument needed is what type of entry you are submitting.
For bosses use **AddBoss**, for mini-bosses use **AddMiniBoss**, and for events use **AddEvent**.

## 2.) Progression - `float`
Next, determine how what part of the game you boss should be fought at and how difficult is it to fight. Is it pre-hardmode? Post-Plantera? You can use these numbers as reference to vanilla boss progression:
```cs
KingSlime = 1f;
EyeOfCthulhu = 2f;
EaterOfWorlds = 3f;
QueenBee = 4f;
Skeletron = 5f;
WallOfFlesh = 6f;
TheTwins = 7f;
TheDestroyer = 8f;
SkeletronPrime = 9f;
Plantera = 10f;
Golem = 11f;
DukeFishron = 12f;
LunaticCultist = 13f;
Moonlord = 14f;
```

## 3.) Boss ID / List of IDs - `int` or `List<int>`
Your boss's ID number(s) is needed. Use `ModContent.NPCType<>()` to submit your ID. If a boss has multiple segments, multiple transformations (into other NPCs), or you are submitting an event, use a List of integers.
```cs
List<int> IDList = new List<int>(){
	ModContent.NPCType<NPCs.ZombieWerewolf>(),
	ModContent.NPCType<NPCs.ZombieVampire>()
}
```

## 4.) Mod Instance - `Mod`
Fairly straight-forward. Since PostSetupContent is within the Mod Class, just use `this` for the argument.

## 5.) Boss Name / Translation - `string`
This will determine what your boss's name will be displayed as. If you have a translation key, submit it so your names will appear in the appropriate language. Not every Boss Name will be equal to the NPC Name of the boss NPC, but much of the time it will be. An example of a boss name not sharing the npc name is "The Twins". See [Localization](#Localization) for more information on translation keys.

## 6.) Downed Boolean - `Func<bool>`
All bosses should have a downed boolean within you ModWorld class. This argument should be a delegate cast as a Func<bool> `(Func<bool>)(() => ModdedWorld.downedWolfBoss)` as this would need to be constantly checked, not just on submission. Proper saving and syncing of the downed bool is up to your mod to implement. Follow ExampleMod if you do not know how to do that.

## 7.) Spawn Items - `int` or `List<int>`
Any items that spawn your boss will go into this next argument. Similar to Boss IDs, you can pass a single integer, or pass in a list of integers if your mod contains multiple spawning items.

## 8.) Collection - `int` or `List<int>`
A list of items will be represented as collectibles. Examples include Trophies, Masks, and Music Boxes. Feel free to add any special item that all your bosses have their own version of.

## 9.) Loot - `int` or `List<int>`
A list of items will be represented as loot. Anything that can be dropped from your boss should be included in this list. DO NOT include any items from the collection. The Treasure Bag should be supplied here as well.

## 10.) Spawn Info _(optional)_ - `string`
You may want to provide additional info to how your boss naturally spawns or the conditions that have to be met in order to fight it. Don't be afraid to make it long and descriptive. This can be a localization key if you support multiple languages. Feel free to use the chat tags system for color and items: [Chat Tags](http://terraria.gamepedia.com/Chat#Tags)

## 11.) Despawn Message _(optional)_ - `string`
For even more feature support, consider making a despawn message for your boss when it defeats all players! This message only appears once a boss leaves after its killed all players. This can be a localization key if you support multiple languages. Use `{0}` in place of where you want the boss name to go.

## 12.) Boss Log Display Texture _(optional)_ - `string`
An image of your boss is automatically detected for the boss log. However, if the auto-generated image looks funny, is too big for the page, or just doesn't work at all, you may want to considering passing in your own png file. A string of the namespace path must be passed in. Ex. `"YOURMODNAME/NPCs/WolfBossLogReplacement"`.

## 13.) Head Texture Icon _(optional)_ - `string`
Boss Checklist attempts to find your boss's head texture. Although this argument is mostly used for event icons, as events do not have "head" textures, you can also use this argument to change a bosses head to an alternate phase/transformation or adjust it if need be.

## 14.) Availability _(optional)_ - `Func<bool>`
If you don't want to show a boss for whatever reason, make a Func<bool> of the conditions need to be met for this boss to be available. This is useful for secret or surprise bosses, but be aware that users usually expect to be able to see all bosses in the listing, so use sparingly.

# Localization
One thing to note is that BossChecklist needs a language-agnostic identifier for modded boss names. In your Mod.Call code, don't pass in localized names for Boss Name. Boss Name needs to be consistent for cross mod compatibility and even multiplayer compatibility. Also try not to change the Boss Name too much as you update your mod, as users will lose their boss records if you change the Boss Name you provide to BossChecklist between versions. If your mod is localizing boss names, passing in `"$" + GetNPC("PuritySpirit").DisplayName.Key` or `"$Mods.ExampleMod.NPCName.PuritySpirit"` rather than `"Spirit of Purity"` will allow correct localized display names in the boss listing.

# Weak Reference Approach
Yet to be implemented.

# Examples
Here is a complete example of how to get your boss in the boss log:
```cs
public override void PostSetupContent() {
	Mod bossChecklist = ModLoader.GetMod("BossChecklist");
	if (bossChecklist != null) {
		bossChecklist.Call(
			"AddBoss",
			1.5f,
			ModContent.NPCType<NPCs.WolfBoss>(),
			this, // Mod
			"Big Bad Wolf",
			(Func<bool>)(() => ModdedWorld.downedWolfBoss),
			ModContent.ItemType<Items.WolfSpawner>(),
			new List<int> {ModContent.ItemType<Items.WolfTrophy>(), ModContent.ItemType<Items.WolfMask>()},
			new List<int> {ModContent.ItemType<Items.WolfItem>(), ModContent.ItemType<Items.WolfItem2>()},
			"Sometimes spawns at midnight",
			"Big Bad Wolf has tainted the heros",
			"ShepherdMod/NPCs/Bosses/WolfBoss/WolfBossTexture",
			"ShepherdMod/NPCs/Bosses/WolfBoss/WolfBossHeadAlt",
			(Func<bool>)(() => WorldGen.crimson));
		// Additional bosses here
	}
}
```
### Localization Example
If you are localizing boss names and spawn info, you should use localization keys. Never retrieve translations yourself in your `Mod.Call` code, it will break BossChecklist for your users. Here is a complete example of that approach:

`Mod.Load` code:
```cs
Mod Translation text = CreateTranslation("BossSpawnInfo.Abomination");
text.SetDefault("Use a [i:" + ModContent.ItemType<Items.Abomination.FoulOrb>() + "] in the underworld after Plantera has been defeated");
text.AddTranslation(GameCulture.Chinese, "???");
AddTranslation(text);
```

`ModNPC.SetStaticDefaults` code:
```cs
public override void SetStaticDefaults() {
	DisplayName.SetDefault("The Abomination");
	DisplayName.AddTranslation(GameCulture.Chinese, "The Abomination in Chinese");
```
Using .lang files is also an option, but passing in the spawn item into the spawn info translation is a little more complex that way.

`Mod.PostSetupContent` code:
```cs
bossChecklist.Call(
	"AddBoss",
	10.5f,
	ModContent.NPCType<NPCs.Abomination.Abomination>(),
	this, // Mod
	"$Mods.ExampleMod.NPCName.Abomination",
	(Func<bool>)(() => ExampleWorld.downedAbomination),
	ModContent.ItemType<Items.Abomination.FoulOrb>(),
	new List<int> { ModContent.ItemType<Items.Armor.AbominationMask>(), ModContent.ItemType<Items.Placeable.AbominationTrophy>() },
	new List<int> { ModContent.ItemType<Items.Abomination.SixColorShield>(), ModContent.ItemType<Items.Abomination.MoltenDrill>() },
	"$Mods.ExampleMod.BossSpawnInfo.Abomination");
```

### If you are still having some trouble
Check out our [FAQs](https://github.com/JavidPack/BossChecklist/wiki#faqs-for-mod-developers) or asks us on the [forums](https://forums.terraria.org/index.php?threads/boss-checklist-in-game-progression-checklist.50668/) or on [Discord](https://discord.gg/rJ2DpNY)!