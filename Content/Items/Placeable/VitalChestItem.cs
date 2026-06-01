using ElementalHearts.Common.Configs;
using ElementalHearts.Content.Tiles.Vital;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Items.Placeable;

/// <summary>
/// Placeable item form of <see cref="VitalChestTile"/>. Drops when the chest is broken,
/// so players can relocate a Vital Chest like any other container.
/// </summary>
public sealed class VitalChestItem : ModItem
{
	public override bool IsLoadingEnabled(Mod mod) => ElementalHeartsServerConfig.Instance.VitalTiles.SystemEnabled;

	public override void SetStaticDefaults()
	{
		Item.ResearchUnlockCount = 1;
	}

	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<VitalChestTile>());
		Item.width = 26;
		Item.height = 22;
		Item.value = Item.sellPrice(silver: 1);
		Item.rare = ItemRarityID.Blue;
	}
}
