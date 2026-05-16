# Elemental Hearts Mod

> A comprehensive tModLoader modification for Terraria that introduces a robust elemental heart system, enabling players to consume thematically-resonant hearts sourced from diverse environmental materials and boss encounters to permanently increase their maximum health.

**Version:** 1.0.2  
**Author:** Vincent Jenei  
**Platform:** tModLoader (Terraria Mod Loader)  
**Terraria Version:** 1.4.x  
**License:** Open Source  

---

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Technical Architecture](#technical-architecture)
- [Installation & Setup](#installation--setup)
- [Core Systems](#core-systems)
- [Content & Items](#content--items)
- [Configuration](#configuration)
- [Multiplayer & Networking](#multiplayer--networking)
- [Development Guide](#development-guide)
- [Cross-Mod Compatibility](#cross-mod-compatibility)
- [Contributing](#contributing)
- [Support & Community](#support--community)

---

## Overview

**Elemental Hearts** transforms Terraria's progression system by introducing a material-based health expansion mechanic. Rather than limiting health gains to the vanilla life fruit system, this mod allows players to craft or find hearts corresponding to virtually every material in Terraria—from basic dirt and wood to hardmode ores and exotic cross-mod materials—each granting permanent health increases when consumed.

### Design Philosophy

- **Thematic Consistency:** Each heart's material is paired with a distinct elemental power identity (e.g., CrimstoneHeart grants "carnage" power, IceHeart grants "ice" power)
- **Progression-Aligned:** Hearts are tiered by rarity, allowing controlled progression through game stages
- **Configurability:** Server-side settings enable customization of HP gains per tier, making difficulty and progression adjustable
- **Cross-Mod Ready:** Native support for Calamity mod materials and boss drops with extensible architecture for additional mods

---

## Features

### 🎯 Core Mechanics

1. **Elemental Heart System**
   - 60+ unique common and uncommon heart variants
   - Tiered rarity system (Common → Uncommon → Rare → Epic → Legendary → Exotic → Mythic)
   - Each heart grants configurable permanent HP increases
   - Thematic elemental power names tied to materials

2. **Boss Heart Drops**
   - Automatic drops from boss defeats
   - **First-kill bonus:** Guaranteed drop on initial defeat
   - **Repeat drops:** 10% chance on subsequent kills
   - Vanilla boss compatibility (all vanilla Terraria bosses)
   - Calamity mod boss support with 50+ unique Calamity heart variants

3. **Multiplayer Support**
   - Network synchronization for consumed hearts
   - World-wide first-kill tracking across all players
   - Proper item drop distribution on server

4. **Configurable Progression**
   - Per-tier HP gain adjustments (0-1000 HP per heart)
   - Server-side configuration for balanced multiplayer
   - Default progression: Common (2 HP) → Mythic (50 HP)

### 📦 Material Variety

**Common Tiers (35+ variants):**
- Block materials: Dirt, Stone, Sand, Glass, Marble, Granite
- Wood variants: Wood, Ebonwood, Shadewood, Pearlwood, Rich Mahogany, Palm Wood, Boreal Wood
- Ice variants: Snow, Ice, Pink Ice, Purple Ice, Red Ice
- Biome materials: Corruption (Ebonstone), Crimson (Crimstone), Hallow (Pearlstone)
- Special materials: Slime, Honey, Cloud, Mud, Fossil, Obsidian
- And more: Hay, Candy Cane, Dynasty, Cactus, Lava, Coral, etc.

**Uncommon & Rare Tiers (30+ variants):**
- Precious stones: Ruby, Sapphire, Emerald, Topaz, Amethyst, Diamond
- Ores: Copper, Iron, Silver, Gold, Lead, Tin, Tungsten, Platinum
- Hardmode ores: Cobalt, Mythril, Orichalcum, Adamantite
- Boss drops: Soul of Light, Soul of Night, Soul of Flight, Soul of Sight, Soul of Might, Soul of Fright
- Special: Demonite, Crimtane, Meteor, Hellstone, Cursed Flame, Ichor
- Corruption/Crimson: Dark Hearts, Brain Hearts, Worm Hearts

**Calamity Cross-Mod Support (50+ variants):**
- Astral variants: Astral, Astral Clay, Astral Dirt, Astral Ice, Astral Stone, Astral Sand
- Post-ML progression: Auric, Exodium, Cosmilite, Draconium
- Environmental: Sulphurous Sand, Abyssal Gravel, Navystone
- Specialized: Calamitous, Celestial Remains, Corpus, and more

---

## Technical Architecture

### Project Structure

```
ElementalHeartsMod/
├── Common/
│   ├── Configs/
│   │   └── ElementalHeartsConfig.cs         # Server-side configuration
│   ├── Hearts/
│   │   ├── ElementalPowerRegistry.cs        # Power name mappings (60+ entries)
│   │   ├── HeartTier.cs                     # Enum: Common, Uncommon, Rare, Epic, Legendary, Exotic, Mythic
│   │   └── HeartTierExtensions.cs           # Tier utility extensions
│   ├── NPCs/
│   │   ├── BossHeartDropGlobalNPC.cs        # Boss loot hook and drop logic
│   │   └── FirstKillBonusDropRule.cs        # First-kill detection and reward
│   ├── Players/
│   │   └── HeartConsumptionPlayer.cs        # Per-player heart consumption tracking
│   ├── Systems/
│   │   ├── BossFirstKillWorld.cs            # Persistent world-state: first kills
│   │   ├── BossHeartDropRegistry.cs         # Heart-to-boss mappings
│   │   └── HeartConsumptionWorld.cs         # World-level consumption management
│   └── Network/
│       └── MessageType.cs                   # Multiplayer message definitions
└── Content/
    └── Items/
        └── Hearts/
            ├── BossHeartItem.cs             # Base heart item class
            ├── Common/
            │   ├── *Heart.cs (35+ files)    # Common tier implementations
            │   └── *.png (35+ files)        # Sprite assets
            └── CrossMod/
                └── Calamity/
                    ├── *Heart.cs (50+ files) # Calamity cross-mod hearts
                    └── CalamityHeartItem.cs  # Calamity-specific base class
```

### Key Classes & Responsibilities

#### **ElementalHeartsConfig** (`Common/Configs/`)
- **Scope:** Server-side (multiplayer-safe)
- **Purpose:** Configurable HP gain per tier
- **Interface:** tModLoader Config API with slider UI
- **Default Values:**
  ```
  Common: 2 HP, Uncommon: 4 HP, Rare: 6 HP, Epic: 8 HP
  Legendary: 10 HP, Exotic: 10 HP, Mythic: 50 HP
  ```

#### **ElementalPowerRegistry** (`Common/Hearts/`)
- **Purpose:** Curated mapping of heart item names to elemental power identifiers
- **Scale:** 60+ registered powers (e.g., BubbleHeart → "bubble", IceHeart → "ice")
- **Usage:** Tooltip generation, thematic consistency

#### **HeartTier** (`Common/Hearts/`)
- **Type:** Enum with 7 values
- **Integer Values:** Used as sell-price multipliers (Common=1x, Mythic=30x)
- **References:** HeartTierExtensions provide utility methods

#### **BossHeartDropGlobalNPC** (`Common/NPCs/`)
- **Hook:** `GlobalNPC.OnKill(NPC npc)`
- **Logic:**
  1. Checks if boss defeat is first-time via `BossFirstKillWorld.IsFirstKill(npc.type)`
  2. Records defeat in persistent world data
  3. Queries `BossHeartDropRegistry.GetDrops(npc.type)` for associated hearts
  4. **First kill:** Guarantees drop of all associated hearts
  5. **Repeat kills:** 10% per-heart drop chance
- **Multiplayer:** Item spawning respects server-side loot rules

#### **BossHeartDropRegistry** (`Common/Systems/`)
- **Purpose:** Bidirectional NPC-to-Heart mapping
- **Load Timing:** ModSystem, loaded at mod initialization
- **Registry Structure:** `Dictionary<int npcType, List<int> heartItemTypes>`
- **Initialization:** Auto-discovers heart items via reflection/attribute scanning

#### **BossFirstKillWorld** (`Common/Systems/`)
- **Purpose:** Persistent world-state tracking for first boss defeats
- **Storage:** Serialized in world files via tModLoader's SaveData system
- **Scope:** Global across all players on a world
- **Methods:**
  - `IsFirstKill(int npcType)` → bool: Check if boss has been defeated once
  - `RecordBossDefeat(int npcType)` → void: Mark boss as defeated

#### **HeartConsumptionPlayer** (`Common/Players/`)
- **Scope:** Per-player tracking
- **Purpose:** Monitor heart consumption and HP increases
- **Hooks:** PostConsume on item usage
- **Data Sync:** Network messages ensure multiplayer consistency

#### **HeartConsumptionWorld** (`Common/Systems/`)
- **Purpose:** World-level consumption state management
- **Coordination:** Interfaces between player consumption and world progression

### Design Patterns

1. **Registry Pattern:** `BossHeartDropRegistry` maintains central heart-to-boss mappings
2. **GlobalNPC Hook:** Centralized boss loot distribution via single `BossHeartDropGlobalNPC` class
3. **Persistent State:** `BossFirstKillWorld` serializes via tModLoader's SaveData
4. **Network Synchronization:** `MessageType` definitions for MP-safe consumption tracking
5. **Configuration API:** tModLoader's `ModConfig` system for server-side customization

---

## Installation & Setup

### Prerequisites

- **Terraria** 1.4.x (latest version)
- **tModLoader** (stable build for 1.4.x)
- For Calamity hearts: **Calamity Mod** (optional)

### Installation Steps

1. **Clone or Download**
   ```bash
   git clone https://github.com/VincentJenei/ElementalHeartsMod.git
   cd ElementalHeartsMod
   ```

2. **Navigate to tModLoader Directory**
   ```
   Windows: %APPDATA%\tModLoader\ModSources\
   Linux: ~/.local/share/Terraria/tModLoader/ModSources/
   Mac: ~/Library/Application Support/Terraria/tModLoader/ModSources/
   ```

3. **Place Mod Folder**
   - Copy the `ElementalHeartsMod` folder to `ModSources/`

4. **Build via tModLoader**
   - Launch tModLoader's Mod Browser
   - Click "Reload Mods" or "Compile Mods"
   - Select "Elemental Hearts" and enable

5. **Configure (Optional)**
   - In-game: Mod Config → Elemental Hearts
   - Adjust HP gains per tier as desired
   - Changes apply to new worlds and ongoing playthroughs

### Calamity Integration (Optional)

If Calamity mod is installed and enabled:
- Calamity hearts automatically register
- 50+ Calamity-specific hearts become available
- Boss drop registry includes all Calamity bosses
- No additional configuration required

---

## Core Systems

### 1. Heart Tier System

**HeartTier Enumeration:**
```csharp
public enum HeartTier {
    Common = 1,         // Base tier, basic materials
    Uncommon = 3,       // Improved materials, early hardmode
    Rare = 5,           // Hardmode progression
    Epic = 7,           // Deep hardmode, specialty items
    Legendary = 10,     // Post-ML boss drops
    Exotic = 15,        // Cross-mod rarities
    Mythic = 30         // Ultimate hearts, max rarity
}
```

**Tier Purpose:**
- Sell price multiplier: A Common heart sells for base value × 1, Mythic × 30
- Cosmetic/UI indicator of power and rarity
- Separated from actual HP gain (controlled by config)

### 2. Boss Heart Drop System

**Trigger:** `BossHeartDropGlobalNPC.OnKill(NPC npc)`

**Flow:**
```
Boss defeated → Is this first kill?
├─ YES → Drop all associated hearts (guaranteed)
└─ NO → Drop each heart with 10% probability

First-kill state persisted to world save
```

**Supported Bosses (Vanilla):**
- King Slime, Eater of Worlds, Brain of Cthulhu
- Queen Bee, Skeletron, Wall of Flesh
- The Twins, The Destroyer, Skeletron Prime
- Plantera, Golem, Duke Fishron
- Empress of Light, Queen Slime, Mechanical Bosses
- All hardmode progression bosses

**Supported Bosses (Calamity):**
- Desert Scourge, Crabulon, The Hive Mind, Perforators
- The Slime God, Cryogen, Aquatic Scourge
- Brimstone Elemental, Astrageldon Slime
- Calamitas, Astrum Aureus, Astrum Deus
- Signus, Polterghast, Devourer of Gods
- Yharon, Supreme Calamitas, Exo Mechs
- And 30+ others

### 3. Player Consumption Tracking

**System:** `HeartConsumptionPlayer`

**On Heart Consumption:**
1. Detects item use via player hook
2. Validates item is registered heart
3. Increases `PlayerStats.MaxHealth`
4. Broadcasts to other players via network message
5. Records to player save data

**Persistence:** Per-character, saved in player files

### 4. World First-Kill State

**System:** `BossFirstKillWorld`

**Persistence:**
```csharp
// Stored as part of world metadata
Dictionary<int npcType, bool> firstKillRecords
```

**Lifecycle:**
1. Fresh world: No bosses marked as defeated
2. Boss defeated: `RecordBossDefeat(npcType)` called
3. Subsequent loads: `IsFirstKill(npcType)` returns recorded state
4. Multiplayer: State shared across all players on world

---

## Content & Items

### 70+ Registered Hearts

#### Common Hearts (Base Materials, 35+)
Directly correspond to Terraria blocks and materials:
- **Blocks:** Dirt, Stone, Sand, Glass, Marble, Granite, Mud, Ice
- **Woods:** Wood, Ebonwood, Shadewood, Pearlwood, Rich Mahogany, Palm Wood, Boreal Wood
- **Biome:** Ebonstone (Corruption), Crimstone (Crimson), Pearlstone (Hallow)
- **Special:** Slime, Honey, Cloud, Moss, Fossil, Obsidian
- **Rare Materials:** Candy Cane, Dynasty, Cactus, Flesh, Lesion, Lava, Hay

#### Uncommon/Rare Hearts (Gemstones & Ores, 30+)
Precious materials and valuable ores:
- **Gemstones:** Ruby, Sapphire, Emerald, Topaz, Amethyst, Diamond, Amber, Crystal
- **Ores:** Copper, Iron, Silver, Gold, Lead, Tin, Tungsten, Platinum
- **Hardmode:** Cobalt, Mythril, Orichalcum, Palladium, Adamantite
- **Boss Souls:** Soul of Light, Night, Flight, Might, Sight, Fright
- **Corruption/Crimson:** Demonite, Crimtane, Dark, Brain, Worm
- **Special:** Meteor, Hellstone, Cursed Flame, Ichor, Cloud

#### Calamity Cross-Mod (50+)
When Calamity is installed:
- **Astral Variants:** Astral, Astral Clay, Astral Dirt, Astral Ice, Astral Stone, Astral Sand, Astral Sandstone, Astral Monolith
- **Biome Materials:** Sulphurous Sand, Abyssal Gravel, Navystone, Eutrophic Sand
- **Progression Ores:** Aerialite, Charred Ore, Scrap Iron, Essence of Might/Eldritch
- **Post-ML:** Auric Ore, Exodium Cluster, Cosmilite, Draconium
- **Boss Drops:** Calamitous, Celestial Remains, Corpus, Dark Plasmic, Gravistar
- **Specialty:** Fungal, Gehenna, Mutated, Novae Slug, Affected, Armored
- **And more:** Blazing, Brimstone Slag, Cryogenic, Cryonic, Enriched, Fungal, etc.

### BossHeartItem Base Class

**Properties:**
```csharp
public class BossHeartItem : ModItem {
    public virtual HeartTier Tier => HeartTier.Common;
    
    public override void SetDefaults() {
        Item.width = 16;
        Item.height = 16;
        Item.maxStack = 99;
        Item.value = Item.sellPrice(0, 0, 10, 0) * (int)Tier;
        Item.rare = GetRarityFromTier(Tier);
    }
    
    public override void AddRecipes() { /* Auto-register as consumable */ }
}
```

**Per-Heart Implementation Pattern:**
```csharp
public sealed class BubbleHeart : BossHeartItem {
    public override HeartTier Tier => HeartTier.Common;
}
// Minimal boilerplate; most behavior inherited
```

### CalamityHeartItem (Calamity-Specific)

Extends `BossHeartItem` with Calamity-specific boss drop registrations:
```csharp
public class CalamityHeartItem : BossHeartItem {
    public override int AssociatedBossNPC => ModContent.NPCType<CalamityBoss>();
}
```

---

## Configuration

### ElementalHeartsConfig

**Scope:** Server-side (safe for multiplayer)

**Configurable Values:**

| Tier | Config Field | Default HP | Min | Max | Purpose |
|------|--------------|-----------|-----|-----|---------|
| Common | `Common` | 2 | 0 | 1000 | Entry-level health boost |
| Uncommon | `Uncommon` | 4 | 0 | 1000 | Early progression |
| Rare | `Rare` | 6 | 0 | 1000 | Mid-game advancement |
| Epic | `Epic` | 8 | 0 | 1000 | Advanced challenges |
| Legendary | `Legendary` | 10 | 0 | 1000 | Boss-tier rewards |
| Exotic | `Exotic` | 10 | 0 | 1000 | Cross-mod integration |
| Mythic | `Mythic` | 50 | 0 | 1000 | Ultimate progression |

**UI Features:**
- Slider controls for each tier
- Real-time preview of values
- Dark background for visibility (40, 40, 40, 220 alpha)
- Range validation (0-1000 HP per heart)

**Access in Code:**
```csharp
int hpGain = ElementalHeartsConfig.Instance.Common;
```

**Customization Scenarios:**
- **Hardcore Mode:** Set all values to 1 HP for minimal advantage
- **Progression Servers:** Increase Mythic to 100+ HP for endgame
- **Casual Play:** Set all to 5+ HP for forgiving difficulty
- **Rebalance:** Increase Common/Uncommon, decrease Legendary for flat progression

---

## Multiplayer & Networking

### Network Message System

**MessageType.cs Definitions:**
```csharp
public enum MessageType {
    HeartConsumption,      // Player consumed a heart
    BossDefeated,          // Boss defeated, sync first-kill state
    HeartDropConfirm,      // Confirm heart item drop
    PlayerStatSync         // Sync player HP/stats
}
```

### Synchronization Points

1. **Heart Consumption**
   - Client sends `HeartConsumption` message with heart item type
   - Server validates item registry
   - Server broadcasts HP change to all players

2. **Boss Defeat**
   - Boss defeated on any client
   - `BossHeartDropGlobalNPC.OnKill()` fires
   - Server checks `BossFirstKillWorld`
   - Server drops items via `Item.NewItem()` (server-authoritative)

3. **World Load**
   - On world join, client receives `BossFirstKillWorld` data
   - All first-kill states synced
   - Subsequent boss defeats use updated state

### Multiplayer Compatibility

✅ **Fully Supported Features:**
- Multiple players consuming hearts simultaneously
- Shared first-kill bonus (applies once per world)
- Proper item drop distribution
- World-save persistence across server restarts
- Cross-client state synchronization

⚠️ **Considerations:**
- Heart consumption is per-character (not shared between players)
- First-kill drops are server-authoritative (no client-side prediction)
- Multiplayer lagspikes may delay heart effect visibility

---

## Development Guide

### Adding a New Heart

#### Step 1: Create Heart Class

```csharp
// Content/Items/Hearts/Common/MyHeart.cs
using ElementalHearts.Common.Hearts;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Items.Hearts.Common;

public sealed class MyHeart : BossHeartItem
{
    public override HeartTier Tier => HeartTier.Common;
}
```

#### Step 2: Register Elemental Power (Optional)

In `Common/Hearts/ElementalPowerRegistry.cs`:
```csharp
["MyHeart"] = "my_elemental_power"
```

#### Step 3: Create Sprite Asset

- Place `Content/Items/Hearts/Common/MyHeart.png` (16×16 pixels)
- Follow existing sprite aesthetic
- Ensure transparency (PNG alpha channel)

#### Step 4: Register Boss Drop (If Boss-Specific)

In `Common/Systems/BossHeartDropRegistry.cs`:
```csharp
registry.Register(
    ModContent.NPCType<MyBoss>(),
    ModContent.ItemType<MyHeart>()
);
```

#### Step 5: Build & Test

```bash
tModLoader → Reload Mods
In-game: Check item exists and consumes properly
```

### Extending for Cross-Mod Support

#### Example: Adding Calamity Hearts

```csharp
// Content/Items/Hearts/CrossMod/Calamity/MyCalamityHeart.cs
using CalamityMod.NPCs.BossesPostML;
using ElementalHearts.Content.Items.Hearts.CrossMod.Calamity;

namespace ElementalHearts.Content.Items.Hearts.CrossMod.Calamity;

public sealed class MyCalamityHeart : CalamityHeartItem
{
    public override HeartTier Tier => HeartTier.Rare;
    
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        // Calamity boss registration happens automatically
    }
}
```

### Code Style & Conventions

- **Namespace:** Follow directory structure exactly
  - `ElementalHearts.Common.Hearts` → `Common/Hearts/`
  - `ElementalHearts.Content.Items.Hearts` → `Content/Items/Hearts/`
- **Sealing Classes:** All implementations should be `sealed` to prevent unintended inheritance
- **Comments:** Use XML documentation (`///`) for public APIs only
- **Naming:** PascalCase for classes, camelCase for fields/parameters
- **Tier Assignment:** Match tier to difficulty/progression (see tier definitions)

### Common Pitfalls

| Issue | Solution |
|-------|----------|
| Heart not appearing in-game | Check namespace and class naming match file path |
| Boss drops not working | Verify NPC type is registered in `BossHeartDropRegistry` |
| Multiplayer desync | Ensure `HeartConsumptionPlayer` network messages are sent |
| Config changes not applying | Restart world or re-enable mod after config change |
| Sprite not showing | Verify PNG is 16×16, PNG format, not transparent background |

---

## Cross-Mod Compatibility

### Calamity Mod Support

**Automatic Detection:**
- On mod load, scans for Calamity mod
- If found, auto-loads all Calamity heart items
- Registers 50+ boss-heart associations

**Calamity Hearts Included:**

**Early Progression:**
- Desert Scourge, Crabulon, Hive Mind, Perforators, Slime God
- Associated with early-game Calamity bosses

**Mid Progression:**
- Cryogen, Aquatic Scourge, Brimstone Elemental, Astrageldon Slime

**Late Progression & Post-ML:**
- Calamitas, Astrum Aureus, Astrum Deus, Signus, Polterghast
- Devourer of Gods, Yharon, Supreme Calamitas, Exo Mechs

**Asset Materials:**
- Astral variants (6 types)
- Sulphurous & Abyssal biome materials
- Post-ML ores (Auric, Exodium, Cosmilite)
- Specialty drops (Calamitous, Celestial Remains, etc.)

### Extensibility

To add support for another mod:

1. **Create Cross-Mod Folder**
   ```
   Content/Items/Hearts/CrossMod/MyMod/
   ```

2. **Create Heart Items**
   ```csharp
   public sealed class MyModHeart : BossHeartItem {
       public override HeartTier Tier => HeartTier.Rare;
   }
   ```

3. **Register Bosses**
   ```csharp
   if (ModLoader.TryGetMod("MyMod", out var myMod)) {
       // Register boss-to-heart mappings
   }
   ```

### Mod Load Order

- **Before:** None (independent mod)
- **After:** (optional) Calamity, Spirit Mod, Thorium, etc. for cross-mod content
- **Compatibility:** Can be loaded in any order

---

## API Reference

### Public Classes

#### `ElementalHeartsConfig`
```csharp
namespace ElementalHearts.Common.Configs;

public sealed class ElementalHeartsConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;
    
    public static ElementalHeartsConfig Instance { get; }
    
    public int Common { get; set; }      // Common tier HP (default: 2)
    public int Uncommon { get; set; }    // Uncommon tier HP (default: 4)
    public int Rare { get; set; }        // Rare tier HP (default: 6)
    public int Epic { get; set; }        // Epic tier HP (default: 8)
    public int Legendary { get; set; }   // Legendary tier HP (default: 10)
    public int Exotic { get; set; }      // Exotic tier HP (default: 10)
    public int Mythic { get; set; }      // Mythic tier HP (default: 50)
}
```

#### `HeartTier`
```csharp
namespace ElementalHearts.Common.Hearts;

public enum HeartTier
{
    Common = 1,
    Uncommon = 3,
    Rare = 5,
    Epic = 7,
    Legendary = 10,
    Exotic = 15,
    Mythic = 30
}
```

#### `BossHeartItem`
```csharp
namespace ElementalHearts.Content.Items.Hearts;

public class BossHeartItem : ModItem
{
    public virtual HeartTier Tier { get; }
    
    public override void SetDefaults();
    public override void AddRecipes();
}
```

#### `BossFirstKillWorld`
```csharp
namespace ElementalHearts.Common.Systems;

public sealed class BossFirstKillWorld : ModSystem
{
    public static bool IsFirstKill(int npcType);
    public static void RecordBossDefeat(int npcType);
}
```

#### `BossHeartDropRegistry`
```csharp
namespace ElementalHearts.Common.Systems;

public sealed class BossHeartDropRegistry : ModSystem
{
    public List<int> GetDrops(int npcType);
    public void Register(int npcType, int itemType);
}
```

---

## Building from Source

### Prerequisites

- Visual Studio 2022 (Community Edition is free)
- tModLoader source/SDK installed
- .NET 6.0+ (for C# 11 features)

### Build Steps

1. **Clone Repository**
   ```bash
   git clone https://github.com/VincentJenei/ElementalHeartsMod.git
   cd ElementalHeartsMod
   ```

2. **Open Project**
   ```bash
   # Option A: Open in Visual Studio
   start ElementalHearts.csproj
   
   # Option B: Build via CLI
   dotnet build
   ```

3. **Configure tModLoader References**
   - Project should auto-detect tModLoader installation
   - If not, set `TMAPI` environment variable to tModLoader directory

4. **Build**
   ```
   Visual Studio: Build → Build Solution (F7)
   CLI: dotnet build -c Release
   ```

5. **Output**
   - Compiled mod: `bin/Release/ElementalHearts.dll`
   - tModLoader auto-detects and reloads

### Project File Structure

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="tModLoader.CodeAssist" Version="..." />
  </ItemGroup>
</Project>
```

---

## Contributing

We welcome contributions! Here's how to get involved:

### Bug Reports

1. Check [GitHub Issues](https://github.com/VincentJenei/ElementalHeartsMod/issues) for duplicates
2. Provide detailed reproduction steps
3. Include mod version, Terraria version, and any other mods loaded
4. Attach logs if available (`Documents/My Games/Terraria/tModLoader/Logs/`)

### Feature Requests

1. Describe the desired feature clearly
2. Explain the motivation and use case
3. Consider cross-mod implications (e.g., Calamity compatibility)
4. Open a GitHub discussion for community feedback

### Code Contributions

1. **Fork** the repository
2. **Create feature branch** (`git checkout -b feature/your-feature`)
3. **Follow code style:** See [Code Style & Conventions](#code-style--conventions)
4. **Test thoroughly:** Include vanilla and Calamity scenarios
5. **Commit clearly:**
   ```
   Add new heart type: MyHeart
   - Implements Common tier
   - Associated with MyBoss
   - Follows ElementalPowerRegistry
   ```
6. **Push & open PR** with detailed description

### Development Workflow

```bash
# 1. Clone and set up
git clone https://github.com/VincentJenei/ElementalHeartsMod.git
cd ElementalHeartsMod

# 2. Create feature branch
git checkout -b feature/add-xyz-heart

# 3. Make changes, commit
git add .
git commit -m "Add xyz heart with tier and registry entry"

# 4. Push to your fork
git push origin feature/add-xyz-heart

# 5. Open PR on GitHub
# → Describe changes, reference issues, request review
```

### Testing Checklist

- [ ] Item appears in creative/inventory
- [ ] Item consumes properly (HP gain matches config)
- [ ] Boss drop registered (appears in drops if configured)
- [ ] First-kill bonus works (guaranteed drop on first defeat)
- [ ] Multiplayer sync works (if applicable)
- [ ] No console errors or warnings
- [ ] Sprite renders correctly in-game
- [ ] Config reload doesn't break state

---

## Support & Community

### Getting Help

- **Discord:** [Join Community Server](https://discord.gg/7WmrGXdQWD)
  - `#elemental-hearts` channel for mod discussion
  - `#bug-reports` for issues
  - `#feature-requests` for suggestions

- **GitHub Issues:** [Report bugs and request features](https://github.com/VincentJenei/ElementalHeartsMod/issues)

- **Discussions:** [Share ideas and feedback](https://github.com/VincentJenei/ElementalHeartsMod/discussions)

### Frequently Asked Questions

**Q: Can I increase HP per heart beyond 1000?**
A: Yes, edit `ElementalHeartsConfig` and change range validation, or modify in-code if compiling from source.

**Q: Do hearts work with Expert/Master Mode?**
A: Yes! Configuration values apply regardless of difficulty. Adjust accordingly for balance.

**Q: Can I disable specific hearts or boss drops?**
A: Remove the class file and recompile, or create a config option to skip specific drops (open issue to request).

**Q: Are hearts compatible with Thorium/Spirit Mod/[other mod]?**
A: Base hearts work with any mod. To add cross-mod hearts, follow [Extensibility](#extensibility) guide.

**Q: How do I uninstall the mod?**
A: Delete the mod folder from `ModSources/` and recompile mods in tModLoader, or disable via Mod Browser.

---

## Version History

### v1.0.2 (Current)
- Stable release with 70+ hearts
- Calamity full support (50+ hearts)
- Configurable HP per tier
- Multiplayer networking
- Boss first-kill bonus system

### v1.0.1
- Initial cross-mod framework
- Calamity integration foundation

### v1.0.0
- Initial release
- 35+ common hearts
- Vanilla boss drops
- Base progression system

---

## License

This project is open-source and available under the [MIT License](LICENSE).

**You are free to:**
- Use, modify, and distribute this mod
- Create derivative works and mods based on it
- Use in modpacks and distributions

**You must:**
- Include the original license notice
- Retain the license in distributions
- Credit the original author (Vincent Jenei)

---

## Roadmap & Future Plans

- [ ] Custom heart crafting recipes
- [ ] Heart augmentation system (combine for stronger variants)
- [ ] Biome-specific heart drops
- [ ] Achievement system
- [ ] Heart transmutation (trade hearts of one type for another)
- [ ] Deep integration with Thorium and Spirit Mod
- [ ] Visual effects on consumption
- [ ] Boss-specific elemental power bonuses (e.g., CrimstoneHeart grants fire resistance)

---

## Credits & Acknowledgments

**Author:** Vincent Jenei

**tModLoader Community:** For excellent mod development framework and documentation

**Calamity Dev Team:** For the Calamity Mod framework enabling cross-mod integration

**Contributors:** All community members who've reported issues and suggested improvements

---

**Questions or issues?** Reach out on [Discord](https://discord.gg/7WmrGXdQWD) or [GitHub Issues](https://github.com/VincentJenei/ElementalHeartsMod/issues).

**Enjoy your elemental heart adventure!** ✨🎮
