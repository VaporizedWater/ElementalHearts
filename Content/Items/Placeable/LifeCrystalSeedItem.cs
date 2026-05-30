using ElementalHearts.Common.Configs;
using ElementalHearts.Content.Tiles.Vital;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Items.Placeable;

/// <summary>
/// Seed dropped (chance-based) when a Life Crystal is extractinated. Planted on Vital
/// Quartz to grow a <see cref="LifeCrystalPlantTile"/>.
/// </summary>
public sealed class LifeCrystalSeedItem : ModItem
{
	public override bool IsLoadingEnabled(Mod mod) => VitalTilesConfig.Instance.SystemEnabled;

	public override void SetStaticDefaults()
	{
		Item.ResearchUnlockCount = 5;
	}

	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<LifeCrystalPlantTile>());
		Item.width = 14;
		Item.height = 14;
		Item.value = Item.sellPrice(silver: 3);
		Item.rare = ItemRarityID.Pink;
	}
}
