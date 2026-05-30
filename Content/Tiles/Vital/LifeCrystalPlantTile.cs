using ElementalHearts.Common.Configs;
using ElementalHearts.Content.Items.Placeable;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ElementalHearts.Content.Tiles.Vital;

/// <summary>
/// Three-stage plant grown from Life Crystal Seeds on Vital Quartz. Mirrors
/// <see cref="LifeFruitPlantTile"/> but yields a vanilla Life Crystal at ripe stage.
/// </summary>
public sealed class LifeCrystalPlantTile : ModTile
{
	public override bool IsLoadingEnabled(Mod mod) => VitalTilesConfig.Instance.SystemEnabled;

	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileLavaDeath[Type] = true;
		Main.tileCut[Type] = true;
		TileID.Sets.ReplaceTileBreakUp[Type] = true;
		TileID.Sets.IgnoredInHouseScore[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.StyleAlch);
		TileObjectData.newTile.AnchorValidTiles = new int[] { ModContent.TileType<VitalQuartzTile>() };
		TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
		TileObjectData.newTile.DrawYOffset = 2;
		TileObjectData.newTile.RandomStyleRange = 1;
		TileObjectData.newTile.StyleMultiplier = 1;
		TileObjectData.addTile(Type);

		DustType = DustID.PinkCrystalShard;
		HitSound = SoundID.Item27;
		RegisterItemDrop(ModContent.ItemType<LifeCrystalSeedItem>(), 0);
		AddMapEntry(new Color(255, 130, 150), CreateMapEntryName());
	}

	public override void RandomUpdate(int i, int j)
	{
		Tile tile = Main.tile[i, j];
		int stage = tile.TileFrameX / 18;

		if (stage >= 2 || !Main.rand.NextBool(20))
			return;

		tile.TileFrameX = (short)((stage + 1) * 18);
		if (Main.netMode != NetmodeID.SinglePlayer)
			NetMessage.SendTileSquare(-1, i, j);
	}

	public override bool RightClick(int i, int j)
	{
		Tile tile = Main.tile[i, j];
		int stage = tile.TileFrameX / 18;
		if (stage < 2)
			return false;

		Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, ItemID.LifeCrystal);
		tile.TileFrameX = 0;

		if (Main.netMode != NetmodeID.SinglePlayer)
			NetMessage.SendTileSquare(-1, i, j);

		return true;
	}
}
