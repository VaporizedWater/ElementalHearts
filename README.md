# Elemental Hearts Classic

A tModLoader mod that adds consumable, single-use hearts crafted from Terraria's blocks,
ores, and boss drops. Each heart permanently raises maximum life and is bound to the world
it was consumed in, so co-op players sharing a world all benefit from the same heart.

## Layout

| Path | Purpose |
| --- | --- |
| `ElementalHearts.cs` | Mod entry point, packet dispatch |
| `Common/Configs/` | Server-side `ModConfig` with per-tier HP values |
| `Common/Hearts/` | `HeartTier` enum + HP/rarity lookups |
| `Common/Network/` | Packet `MessageType` definitions |
| `Common/Players/` | Per-character consumption ledger (`HeartConsumptionPlayer`) |
| `Common/Systems/` | Per-world consumption ledger (`HeartConsumptionWorld`) |
| `Content/Rarities/` | `ModRarity` tier colors |
| `Content/Items/Hearts/` | `ElementalHeartItem` base + vanilla hearts grouped by tier |
| `Content/Items/Hearts/CrossMod/` | Hearts from other mods, one folder per source mod |

## Adding a vanilla heart

1. Drop the sprite into the matching tier folder (`Content/Items/Hearts/<Tier>/`).
2. Add a class next to it that inherits from `ElementalHeartItem`:

```csharp
public sealed class MyHeart : ElementalHeartItem
{
    public override HeartTier Tier => HeartTier.Common;

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.SomeBlock, 50)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}
```

## Adding a cross-mod heart

1. Drop the sprite into `Content/Items/Hearts/CrossMod/<SourceMod>/`.
2. Inherit from the matching `<SourceMod>HeartItem` base and call `RegisterModRecipe`:

```csharp
public sealed class MyCrossModHeart : CalamityHeartItem
{
    public override HeartTier Tier => HeartTier.Rare;

    public override void AddRecipes() =>
        RegisterModRecipe("SomeCalamityItem", 20, TileID.MythrilAnvil);
}
```

The base class auto-gates loading on the source mod, and `RegisterModRecipe` silently
no-ops if the source mod isn't loaded or the ingredient isn't found.
