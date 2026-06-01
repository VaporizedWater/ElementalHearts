using ElementalHearts.Common.Configs;
using ElementalHearts.Content.Tiles.Vital;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Items.Placeable;

/// <summary>
/// Seed dropped (chance-based) when a Life Fruit is extractinated. Planted on Vital Soil
/// to grow a <see cref="LifeFruitPlantTile"/>; placement restrictions are enforced by the
/// tile's <c>TileObjectData.AnchorValidTiles</c>.
/// </summary>
public sealed class LifeFruitSeedItem : ModItem
{
	public override bool IsLoadingEnabled(Mod mod) => ElementalHeartsServerConfig.Instance.VitalTiles.SystemEnabled;

	public override void SetStaticDefaults()
	{
		Item.ResearchUnlockCount = 5;
	}

	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<LifeFruitPlantTile>());
		Item.width = 14;
		Item.height = 14;
		Item.value = Item.sellPrice(silver: 5);
		Item.rare = ItemRarityID.LightRed;
	}
}
