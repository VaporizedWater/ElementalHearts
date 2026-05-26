using ElementalHearts.Common.Configs;
using ElementalHearts.Content.Items.Tiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Tiles.Vital;

/// <summary>
/// Soil-family Vital block: silt-coloured with a faint maroon glow and red dust particles.
/// Spreads chlorophyte-style to adjacent Dirt, Mud, and Silt at a config-driven rate so a
/// small seed bed can slowly establish a life-aspected patch.
/// </summary>
public sealed class VitalSoilTile : ModTile
{
	public override bool IsLoadingEnabled(Mod mod) => VitalTilesConfig.Instance.SystemEnabled;

	public override void SetStaticDefaults()
	{
		Main.tileSolid[Type] = true;
		Main.tileBlockLight[Type] = true;
		Main.tileLighted[Type] = true;
		Main.tileMergeDirt[Type] = true;
		Main.tileBlendAll[Type] = true; // Visually blend with everything to prevent slope air gaps
		
		int[] mergeTiles = {
			TileID.Dirt, TileID.Grass, TileID.CorruptGrass, TileID.CrimsonGrass, TileID.HallowedGrass, 
			TileID.JungleGrass, TileID.MushroomGrass, TileID.AshGrass, TileID.ClayBlock, TileID.Mud, 
			TileID.Stone, TileID.Ebonstone, TileID.Crimstone, TileID.Pearlstone,
			TileID.Sand, TileID.Ebonsand, TileID.Crimsand, TileID.Pearlsand,
			TileID.Ash, TileID.Silt, TileID.Slush, TileID.SnowBlock, TileID.IceBlock, TileID.CorruptIce,
			TileID.FleshIce, TileID.HallowedIce, TileID.Marble, TileID.Granite,
			TileID.Copper, TileID.Tin, TileID.Iron, TileID.Lead, TileID.Silver, TileID.Tungsten,
			TileID.Gold, TileID.Platinum, TileID.Demonite, TileID.Crimtane, TileID.Meteorite,
			TileID.Obsidian, TileID.Hellstone, TileID.Cobalt, TileID.Palladium, TileID.Mythril,
			TileID.Orichalcum, TileID.Adamantite, TileID.Titanium, TileID.Chlorophyte,
			TileID.Amethyst, TileID.Topaz, TileID.Sapphire, TileID.Emerald, TileID.Ruby, TileID.Diamond
		};
		foreach (int tileId in mergeTiles) {
			Main.tileMerge[Type][tileId] = true;
			Main.tileMerge[tileId][Type] = true;
		}

		MinPick = 0;
		DustType = DustID.Crimstone;
		HitSound = SoundID.Dig;

		RegisterItemDrop(ModContent.ItemType<VitalSoilItem>());

		AddMapEntry(new Color(140, 110, 120), CreateMapEntryName());
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		// Only glow on the outer edges (touching a different block or air). Cardinal-only
		// check — visually identical to a full 3x3 for solid masses, half the tile reads.
		ushort type = Type;
		bool isEdge =
			(i > 0 && (!Main.tile[i - 1, j].HasTile || Main.tile[i - 1, j].TileType != type)) ||
			(i < Main.maxTilesX - 1 && (!Main.tile[i + 1, j].HasTile || Main.tile[i + 1, j].TileType != type)) ||
			(j > 0 && (!Main.tile[i, j - 1].HasTile || Main.tile[i, j - 1].TileType != type)) ||
			(j < Main.maxTilesY - 1 && (!Main.tile[i, j + 1].HasTile || Main.tile[i, j + 1].TileType != type));

		if (isEdge)
		{
			// Breathing effect similar to vital quartz, but maroon/crimson
			float time = Main.GlobalTimeWrappedHourly * 0.8f;
			float phase = (i * 0.05f) + (j * 0.05f);
			float noise = ((i * 37 + j * 19) % 100) / 100f * 3f;
			float intensity = (float)System.Math.Sin(time + phase + noise);

			float glow = (intensity * 0.5f + 0.5f);
			glow *= glow; glow *= glow; // glow^4 without the Math.Pow call

			// Subtle, pulsing maroon glow on the edges
			r = 0.18f * glow + 0.02f;
			g = 0.04f * glow + 0.005f;
			b = 0.06f * glow + 0.005f;
		}
		else
		{
			// No glow inside the solid mass
			r = 0f;
			g = 0f;
			b = 0f;
		}
	}

	public override void RandomUpdate(int i, int j)
	{
		// Idle red dust drifting upward; cheap visual cue without overwhelming particle count.
		if (Main.rand.NextBool(40))
		{
			Dust dust = Dust.NewDustDirect(new Vector2(i * 16f, j * 16f), 16, 4, DustID.Crimstone,
				0f, -0.4f, 100, default, 0.7f);
			dust.noGravity = true;
			dust.fadeIn = 0.6f;
		}

		// Chlorophyte-style spread: a rare roll picks one cardinal neighbour and converts
		// it if it's a valid soft-soil tile. Mud, Dirt, and Silt are the eligible targets.
		float spreadChance = VitalTilesConfig.Instance.VitalSoilSpreadChance;
		if (spreadChance <= 0f || Main.rand.NextFloat() >= spreadChance)
			return;

		int dx = 0, dy = 0;
		switch (Main.rand.Next(4))
		{
			case 0: dx = -1; break;
			case 1: dx = 1; break;
			case 2: dy = -1; break;
			default: dy = 1; break;
		}

		int nx = i + dx;
		int ny = j + dy;
		if (nx < 0 || ny < 0 || nx >= Main.maxTilesX || ny >= Main.maxTilesY)
			return;

		Tile target = Main.tile[nx, ny];
		if (!target.HasTile)
			return;

		ushort t = target.TileType;
		if (t == TileID.Dirt || t == TileID.Mud || t == TileID.Silt)
		{
			target.TileType = (ushort)Type;
			WorldGen.SquareTileFrame(nx, ny);
			NetMessage.SendTileSquare(-1, nx, ny);
		}
	}

	public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
	{
		if (!fail && !effectOnly)
		{
			// Replicate Desert Fossil fragile breaking mechanics (higher chance to break adjacent tiles)
			if (Main.rand.Next(100) < 60) // 60% chance to break a neighbor
			{
				int dx = Main.rand.Next(-1, 2);
				int dy = Main.rand.Next(-1, 2);
				if (dx != 0 || dy != 0)
				{
					int nx = i + dx;
					int ny = j + dy;
					if (nx >= 0 && nx < Main.maxTilesX && ny >= 0 && ny < Main.maxTilesY)
					{
						if (Main.tile[nx, ny].HasTile && Main.tile[nx, ny].TileType == Type)
						{
							WorldGen.KillTile(nx, ny);
							if (Main.netMode == Terraria.ID.NetmodeID.Server)
								NetMessage.SendData(Terraria.ID.MessageID.TileManipulation, -1, -1, null, 0, nx, ny);
						}
					}
				}
			}
		}
	}
}
