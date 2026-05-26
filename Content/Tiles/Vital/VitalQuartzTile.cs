using ElementalHearts.Common.Configs;
using ElementalHearts.Content.Items.Tiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Tiles.Vital;

/// <summary>
/// Stone-family Vital block: peach and lime green, palette-matched to Life Fruit. Breaks
/// very fast (like Desert Fossil) and spreads chlorophyte-style to adjacent Stone.
/// </summary>
public sealed class VitalQuartzTile : ModTile
{
	public override bool IsLoadingEnabled(Mod mod) => VitalTilesConfig.Instance.SystemEnabled;

	public override void SetStaticDefaults()
	{
		Main.tileSolid[Type] = true;
		Main.tileBlockLight[Type] = true;
		Main.tileLighted[Type] = true;
		Main.tileMergeDirt[Type] = true;
		Main.tileStone[Type] = true;
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

		// Mirrors Desert Fossil's "any pick breaks it quickly" feel rather than gating
		// behind a specific pickaxe tier.
		MinPick = 0;
		MineResist = 0.5f;
		DustType = DustID.JungleSpore;
		HitSound = SoundID.Tink;

		RegisterItemDrop(ModContent.ItemType<VitalQuartzItem>());

		AddMapEntry(new Color(110, 140, 110), CreateMapEntryName());
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		// Slower, smoother breathing effect with natural variation
		float time = Main.GlobalTimeWrappedHourly * 0.8f;
		float phase = (i * 0.05f) + (j * 0.05f);
		
		// Add static noise so individual tiles pulse slightly out of sync
		float noise = ((i * 37 + j * 19) % 100) / 100f * 3f; 
		float intensity = (float)System.Math.Sin(time + phase + noise);
		
		// Map from [-1, 1] to [0, 1] and use a power curve for a subtle peak
		float glow = (intensity * 0.5f + 0.5f);
		glow *= glow; glow *= glow; // glow^4 without the Math.Pow call
		
		r = 0.08f * glow + 0.01f; // Base very dim glow so it's not pitch black
		g = 0.12f * glow + 0.01f;
		b = 0.02f * glow + 0.005f;
	}

	public override void RandomUpdate(int i, int j)
	{
		if (Main.rand.NextBool(60))
		{
			Dust dust = Dust.NewDustDirect(new Microsoft.Xna.Framework.Vector2(i * 16f, j * 16f), 16, 4, Terraria.ID.DustID.JungleSpore,
				0f, -0.3f, 100, default, 0.7f);
			dust.noGravity = true;
		}

		// Naturally grow Jungle Vines downwards from the ceiling
		if (Main.rand.NextBool(15))
		{
			Tile tileBelow = Main.tile[i, j + 1];
			Tile tile = Main.tile[i, j];
			if (!tileBelow.HasTile && !tile.BottomSlope && !tile.IsHalfBlock)
			{
				tileBelow.HasTile = true;
				tileBelow.TileType = Terraria.ID.TileID.JungleVines;
				WorldGen.SquareTileFrame(i, j + 1, true);
				if (Main.netMode == Terraria.ID.NetmodeID.Server)
				{
					NetMessage.SendTileSquare(-1, i, j + 1);
				}
			}
		}

		float spreadChance = VitalTilesConfig.Instance.VitalQuartzSpreadChance;
		if (spreadChance > 0f && Main.rand.NextFloat() < spreadChance)
		{
			int s_dx = 0, s_dy = 0;
			switch (Main.rand.Next(4))
			{
				case 0: s_dx = -1; break;
				case 1: s_dx = 1; break;
				case 2: s_dy = -1; break;
				default: s_dy = 1; break;
			}

			int nx = i + s_dx;
			int ny = j + s_dy;
			if (nx >= 0 && ny >= 0 && nx < Main.maxTilesX && ny < Main.maxTilesY)
			{
				Tile target = Main.tile[nx, ny];
				if (target.HasTile && target.TileType == Terraria.ID.TileID.Stone)
				{
					target.TileType = (ushort)Type;
					WorldGen.SquareTileFrame(nx, ny);
					if (Main.netMode == Terraria.ID.NetmodeID.Server)
						NetMessage.SendTileSquare(-1, nx, ny);
				}
			}
		}

		// Radiate life: stimulate nearby plant growth (Spores and Life Fruit)
		for (int k = 0; k < 2; k++)
		{
			int radius = 30;
			int targetX = i + Main.rand.Next(-radius, radius + 1);
			int targetY = j + Main.rand.Next(-radius, radius + 1);

			if (targetX >= 5 && targetX < Main.maxTilesX - 5 && targetY >= 5 && targetY < Main.maxTilesY - 5)
			{
				int ddx = targetX - i;
				int ddy = targetY - j;
				if (ddx * ddx + ddy * ddy <= radius * radius)
				{
					Tile target = Main.tile[targetX, targetY];
					Tile tileAbove = Main.tile[targetX, targetY - 1];

					if (target.HasTile && target.TileType == Terraria.ID.TileID.JungleGrass && !tileAbove.HasTile && target.Slope == SlopeType.Solid && !target.IsHalfBlock)
					{
						if (Main.rand.NextBool(40)) 
						{
							tileAbove.HasTile = true;
							tileAbove.TileType = Terraria.ID.TileID.JunglePlants;
							tileAbove.TileFrameX = 162; 
							tileAbove.TileFrameY = 0;
							if (Main.netMode == Terraria.ID.NetmodeID.Server)
								NetMessage.SendTileSquare(-1, targetX, targetY - 1);
						}
						else if (Main.rand.NextBool(80)) 
						{
							if (Terraria.NPC.downedMechBoss1 || Terraria.NPC.downedMechBoss2 || Terraria.NPC.downedMechBoss3)
							{
								Tile tRight = Main.tile[targetX + 1, targetY];
								Tile tAboveRight = Main.tile[targetX + 1, targetY - 1];
								
								if (tRight.HasTile && tRight.TileType == Terraria.ID.TileID.JungleGrass && !tAboveRight.HasTile)
								{
									WorldGen.PlaceTile(targetX, targetY - 1, Terraria.ID.TileID.LifeFruit, mute: true);
									if (Main.tile[targetX, targetY - 1].TileType == Terraria.ID.TileID.LifeFruit)
									{
										if (Main.netMode == Terraria.ID.NetmodeID.Server)
											NetMessage.SendTileSquare(-1, targetX, targetY - 1, 2);
									}
								}
							}
						}
					}
				}
			}
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
