# ElementalHearts — Project Rules

A Terraria **tModLoader 1.4+** mod (C#). Adds consumable "hearts" that permanently raise max HP, themed on materials/bosses, with deep cross-mod support.

## The core principle (obey this above all else)

**The base class owns all behavior. A concrete heart is pure declaration — it states *what it is*, never *how it works*.** `ElementalHeartItem` (~500 lines) handles FX, sync, tooltips, glow, value, and gating; a real heart like `AstralHeart` is 14 lines. Keep that asymmetry. Anything new must preserve it, or it isn't consistent with this codebase.

This principle produces eight concrete rules. When you add code, check each one:

1. **Tier is the single source of truth.** HP, rarity, effect color, sell value, sound pitch, dust counts, camera shake, glow size — *all* derive from `HeartTier` via `HeartTierExtensions` / switch expressions. Never hand-tune look or feel per heart. A new heart picks a tier and inherits a coherent identity for free. **Every *pure* per-tier constant lives in `HeartTierExtensions` as an extension method** (`GetEffectColor`, `GetRarityType`, `GetRarityScale`, `GetWorldGlowDampen`, …) — the item class consumes them, never defines its own per-tier `switch`. If you add a tier-keyed look/feel value, add it there so the ladder stays in one file.
2. **Declaration-only subclasses.** A vanilla heart overrides exactly `Tier` + `AddRecipes()`. A cross-mod family declares only `SourceMod`. If a concrete heart needs to override `SetDefaults`, `UseItem`, or FX, **stop** — add a documented `virtual` hook to the base instead and override that. The exception lives in the base; the leaf stays a declaration.
3. **Virtual hooks with sensible defaults, never required overrides.** New content must appear everywhere correctly (Munchies, sound, tooltip, HP) with zero extra work. Give every new base hook a default; overriding is the rare, documented exception (e.g. `PotionHeartItem` returns `HpGain = 0`).
4. **`abstract` base, `sealed` concrete.** Every leaf heart is `sealed`; every shared base is `abstract`. Keep the hierarchy shallow: `ElementalHeartItem → {BossHeartItem, CrossModHeartItem} → per-mod family → concrete`.
5. **Namespace mirrors folder path exactly, and organization MUST reflect rarity.** File-scoped namespace, always; folder taxonomy is Source → Tier → Category and the namespace matches it character-for-character. The folder is a heart's rarity made physical, so **a tier-named folder (`Common/` … `Mythic/`) contains *only* hearts of that tier** — a heart must never sit in a folder that claims a rarity it isn't. **If a heart's `Tier` changes, its physical location and namespace change with it** (move the `.cs` *and* its `.png`, update the namespace). A themed family that deliberately spans tiers lives in a **category folder named for the theme, never a tier** — `Potions/` and `Pacified/` are the sanctioned examples; each class still declares its own `Tier`. Cross-mod hearts are organized by source mod (`CrossModHearts/<Mod>/`). Identity is the class name (not the namespace), so these moves are always save-safe.
6. **Config-driven, never hardcoded gameplay constants.** Recipe amounts go through `RecipeCost()`; HP and FX strength come from config. No magic numbers for balance in a concrete heart.
7. **Multiplayer-safe by construction.** Guard client-only visual code with an early `if (Main.netMode == NetmodeID.Server) return;`. Run consumption only for `Main.myPlayer`. The server re-derives HP and never trusts the client.
8. **Comments explain *intent and game-feel*, not mechanics.** Every non-trivial base member gets an XML `<summary>` with `<see cref>` links, and inline comments say *why* ("a dull thud reads as 'nope'"; "kept exclusive so it never stops feeling rare"). Use expression-bodied members for one-liners.
9. **ONLY passive hearts give HP.** Active ability hearts are not passive and MUST override `HpGain` to return `0`. *(Note: Active ability hearts still require entries in `ElementalPowerRegistry` and `HeartEffectRegistry` just like passive ones).*

## Hard rules (C# / tML)

- **Always inherit from TML classes** — `ModItem`, `ModPlayer`, `ModProjectile`, `ModSystem`, `GlobalNPC`, `ModConfig`, etc. Never write raw XNA/MonoGame update or draw logic without going through a tML hook.
- **PascalCase** for public types, properties, and methods. `camelCase` for locals/params. Match the casing of surrounding code.
- `Nullable`, `ImplicitUsings`, and `LangVersion latest` are **enabled** (see `.csproj`) — don't re-import the usings ImplicitUsings already provides, and respect nullable annotations.
- **Exactly one `ModPlayer` for the consumption ledger** (`HeartConsumptionPlayer`). A duplicate ModPlayer previously crashed with Calamity — never add a second one.
- **Never store HP on the player.** HP is read live from the heart definition via `HeartRegistry.GetHp` so HP-config changes apply retroactively. The MP packet carries `(int itemType, int consumerWhoAmI)`; the **server re-derives HP** from the registered singleton and never trusts client-sent values.
- **Load gates run once *per heart*; hot paths run every frame.** `IsLoadingEnabled` / `ShouldLoadHeartsFor` fire ~once per cross-mod heart during load — never do per-call disk I/O or JSON parsing there; read the file once and cache it for the load (a config change is `[ReloadRequired]`, which resets statics for you). Likewise, code on per-frame paths (`ModifyTooltips`, `PreDrawInWorld`/`InInventory`, `ModPlayer.PreUpdate`, projectile/NPC `AI`) must not allocate per call — memoize world-derived values (e.g. the world-GUID key prefix in `HeartConsumptionPlayer`) and cache `ModContent.*Type<T>()` lookups, fully-qualify nothing the `using`s already cover, and early-out cheaply.

## Adding a new heart

New heart classes should **only** override `Tier` and `AddRecipes()` — everything else (consume FX, sync, stat bonus) flows from the base `ElementalHeartItem`. Pattern (`Content/Items/Hearts/Vanilla/Common/Organic/FlynxHeart.cs`):

```csharp
public sealed class FlynxHeart : ElementalHeartItem
{
    public override HeartTier Tier => HeartTier.Rare;

    public override void AddRecipes() =>
        CreateRecipe()
            .AddIngredient(ItemID.FlinxFur, RecipeCost(30))
            .AddTile(TileID.Loom)
            .Register();
}
```

A cross-mod heart inherits its `SourceMod` from its family base and uses `RegisterModRecipe` — typically a single expression-bodied line (`Content/Items/Hearts/CrossMod/Calamity/Rare/Ores/AstralHeart.cs`):

```csharp
public sealed class AstralHeart : CalamityHeartItem
{
    public override HeartTier Tier => HeartTier.Rare;

    public override void AddRecipes() =>
        RegisterModRecipe("AstralBar", 20, TileID.MythrilAnvil, ModContent.ItemType<RareLifeShard>(), 3);
}
```

- Use `RecipeCost(...)` so recipe quantities respect `ElementalHeartsRecipeConfig`.
- Class hierarchy: `ElementalHeartItem` → `BossHeartItem` (vanilla boss) / `CrossModHeartItem` (cross-mod craftable). Cross-mod boss hearts: `{Calamity,Thorium,Consolaria}BossHeartItem : BossHeartItem`. Per-mod family bases (e.g. `CalamityHeartItem`) exist only to declare `SourceMod` once — concrete hearts extend the family base, never `CrossModHeartItem` directly.
- `RegisterModRecipe` silently no-ops if the source mod or ingredient is missing, so cross-mod hearts never crash when their mod is absent.
- **Checklist for every new heart** — a **`<ClassName>.png` beside the `.cs`** (tML derives the texture from the namespace+class path; this is a RULE — `build.ps1` auto-creates a blank placeholder and names any heart still missing real art, so the build never dies on a missing texture, but a placeholder must never ship); the right tier (and the matching tier/category folder per rule #5); a recipe via `RecipeCost`/`RegisterModRecipe`; an entry in `HeartEffectRegistry` (outer ring = material color, inner = tier color) — **≥3 sprite-derived colours, curated per the color-palette rule below**; a power name in `ElementalPowerRegistry` (the word in the "elemental power activated" tooltip — see the **element-name rule** below; it is mandatory and curated, never the fallback); and a name/tooltip in `Localization/en-US_Mods.ElementalHearts.hjson` *only if* the auto-generated name is wrong (display names derive from the class name otherwise). Both registries fall back silently to a generic value, so a **DEBUG** build runs `HeartContentValidator` and logs a warning for any heart missing an effect or power entry (and for any active-ability heart that violates rule #9 by granting HP) — check the log after adding one. That's the whole job — if you need more, extend the base (see core principle #2).

**The element-name rule (mandatory).** Every heart MUST have a hand-written `ElementalPowerRegistry` entry — a *custom, lighthearted, genuinely funny* element name that fits the heart's theme. There is **no such thing as a placeholder power word**, and the class-name fallback is never acceptable in shipping content; a `HeartContentValidator` warning about a missing power entry is a build-blocker, not a suggestion. Match the house style: short, punny, and apt — e.g. the lunar bricks are moon-phase jokes (`moonlighting`, `cold open`, `overtime`), the Pacified hearts climb an anger-management arc (`deep breaths` → `anger management` → `inner peace` → `zen mode` → `enlightenment`), and the Magnification Heart's is `enhance` (the CSI "zoom… enhance"). If you can't think of a good one yet, the heart isn't done.

**The color-palette rule (mandatory).** Every heart's `HeartEffectRegistry` entry MUST be hand-curated from its **own sprite's palette** — the consume burst has to read as the heart's actual material, never the hash-derived fallback. **At least three colours** (think base + highlight + shadow); a monochrome material still needs three distinct *shades*. Default to three, and use four or five only when the sprite is genuinely multi-hued (gemcorn, prismatic-ish materials). Source candidates with `tools/Get-HeartPalette.ps1` (dumps a `.png`'s dominant hex colours and a paste-ready `Eff(...)`), then **curate** — the most frequent colour is usually the black outline or antialiasing, so drop those and pick the hues that actually say *what the heart is made of*. Prismatic hearts (`HeartEffect.Prismatic`) are the only exemption — they cycle the whole wheel on purpose. A DEBUG build flags any non-prismatic heart with fewer than three colours (`HeartContentValidator`), so treat that warning as a build-blocker too.

## Layout

- `Common/` — shared systems: `Hearts/` (registries, tiers, `HeartContentValidator`), `Players/`, `Systems/`, `Configs/`, `Network/`, `CrossMod/`, `UI/`, `Worldgen/`, `Biomes/`, `Camera/`.
- `Content/Items/Hearts/` — the heart items, organized by source/rarity/category.
- `Mod.PostSetupContent` builds the runtime registries in order — `HeartRegistry.Build()`, `PotionHeartRegistry.Build()`, `BossHeartDropRegistry.Build()` — then registers the cross-mod integrations. `HeartContentValidator.Validate` runs last (DEBUG-only, compiled out of release). `HeartEffectRegistry` / `ElementalPowerRegistry` are static lookup tables, not built. (Boss drops are centralized in `BossHeartDropRegistry`.)
- `notes/` — dev scratch (design docs, material/sprite lists, a reference copy of Munchies' `CenteredUIImage`). Excluded from the packaged mod via `build.txt` (`notes\*`) and from compilation via the csproj (`<Compile Remove="notes\**\*.cs" />`). Never put compiled source here, and keep non-mod files out of the project root.
- `tools/` — dev-only PowerShell helpers (e.g. `Get-HeartPalette.ps1`, the sprite→hex palette extractor for the color-palette rule). No compiled source here; `*.ps1` is buildIgnored.
- `build.ps1` — CLI compile-check that filters CS1705 noise (close tModLoader first; see Gotchas). Before compiling it runs `Ensure-HeartTextures`: the *texture half* of the heart validator, which creates a 1×1 transparent placeholder for any concrete heart missing its `.png` and lists what still needs art. The *content half* (effect/power/HP rules) is the DEBUG-only `HeartContentValidator`.

## Gotchas (from changelog — don't repeat)

- Don't auto-place hostile boss spawners in worldgen (Menacing Statue griefing).
- Avoid heavy 3rd-party mod dependencies (WebmilioCommons was removed).
- **Building from the CLI:** run `./build.ps1`. `dotnet build` emits **CS1705** errors that are environment-only (the script filters them); and tML returns **TML003** while tModLoader is open, so close it or disable the mod first. The in-game tML build is the final word.
- **Repo hygiene:** keep non-mod files out of the source root — a loose `.cs` there is compiled straight into the assembly (an old `test.cs` shipped a `Program.Main`; a copied `CenteredUIImage.cs` baked a foreign `Munchies.UIElements` type in). Dev scratch lives in `notes/`. Never commit `.claude/worktrees/` or `.claude/settings.local.json` — a stray agent worktree was once committed and doubled the repo's file count; both are now gitignored.

## Cross-mod targets

Calamity, Thorium, Consolaria. Toggles live in `ElementalHeartsCrossModConfig`.
