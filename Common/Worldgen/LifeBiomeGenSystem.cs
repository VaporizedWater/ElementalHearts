using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ElementalHearts.Common.Configs;
using ElementalHearts.Content.Items.Hearts.Common;
using ElementalHearts.Content.Items.Hearts.Uncommon;
using ElementalHearts.Content.Tiles.Vital;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace ElementalHearts.Common.Worldgen;

/// <summary>
/// Inserts the two Life Mini-Biome generation passes: a surface pass scattering organic
/// patches of <see cref="VitalSoilTile"/> across most overworld biomes, and a jungle pass
/// laying out heart-shaped <see cref="VitalQuartzTile"/> formations with bonus Life
/// Crystals tucked inside.
/// </summary>
public sealed class LifeBiomeGenSystem : ModSystem
{
	public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
	{
		ElementalHeartsWorldConfig worldCfg = ElementalHeartsWorldConfig.Instance;
		VitalTilesConfig vitalCfg = VitalTilesConfig.Instance;

		// Both gates are honoured: the world toggle disables this independently of the
		// tile system, and disabling the tile system disables the biome too (placing
		// non-existent tiles would crash worldgen).
		if (!worldCfg.GenerateLifeBiomes || !vitalCfg.SystemEnabled)
			return;

		int settleIndex = tasks.FindIndex(p => p.Name == "Settle Liquids");
		if (settleIndex != -1)
		{
			tasks.Insert(settleIndex + 1, new PassLegacy("Vital Soil Biomes", SurfaceBiomePass, 2.0));
		}

		int jungleIndex = tasks.FindIndex(p => p.Name == "Jungle Trees");
		if (jungleIndex != -1)
		{
			tasks.Insert(jungleIndex + 1, new PassLegacy("Vital Quartz Biomes", JungleBiomePass, 2.0));
		}
	}

	// ── Surface pass ─────────────────────────────────────────────────────────

	private void SurfaceBiomePass(GenerationProgress progress, GameConfiguration config)
	{
		progress.Message = Language.GetTextValue("Mods.ElementalHearts.WorldGen.VitalSoilBiomes");

		int multiplier = ElementalHeartsWorldConfig.Instance.SurfaceBiomeCountMultiplier;
		if (multiplier <= 0)
			return;

		// Scale with world size: ~3 small / ~5 medium / ~7 large at multiplier 1.
		int baseCount = System.Math.Max(2, Main.maxTilesX / 1200);
		int target = baseCount * multiplier;

		// Track placement X coordinates to enforce minimum spacing between biomes.
		List<int> placedXs = new(target);
		const int MinSpacing = 200;

		int placed = 0;
		int attempts = 0;
		int maxAttempts = target * 30;

		while (placed < target && attempts < maxAttempts)
		{
			attempts++;
			progress.Set((float)placed / target);

			int x = WorldGen.genRand.Next(300, Main.maxTilesX - 300);

			if (TooClose(placedXs, x, MinSpacing))
				continue;

			int y = FindSurfaceY(x);
			if (y < 0)
				continue;

			if (IsExcludedSurface(x, y))
				continue;

			PlaceVitalSoilBlob(x, y);
			PlaceBonusLifeCrystals(x, y, 2);

			placedXs.Add(x);
			placed++;
		}
	}

	private static bool TooClose(List<int> placedXs, int x, int minSpacing)
	{
		foreach (int px in placedXs)
		{
			if (System.Math.Abs(px - x) < minSpacing)
				return true;
		}
		return false;
	}

	private static int FindSurfaceY(int x)
	{
		// Scan downward from a bit above the recorded surface; first solid tile wins.
		int start = System.Math.Max(50, (int)Main.worldSurface - 80);
		int end = System.Math.Min(Main.maxTilesY - 200, (int)Main.worldSurface + 80);

		for (int y = start; y < end; y++)
		{
			Tile t = Main.tile[x, y];
			if (t.HasTile && Main.tileSolid[t.TileType])
				return y;
		}

		return -1;
	}

	private static bool IsExcludedSurface(int x, int y)
	{
		// Ocean buffer keeps the biome off beaches and out of the ocean's tile types,
		// matching the user's "any reasonable overworld place except Jungle and Hell".
		if (x < 380 || x > Main.maxTilesX - 380)
			return true;

		// Hell sits in the bottom band of the world.
		if (y > Main.maxTilesY - 200)
			return true;

		Tile t = Main.tile[x, y];

		// Jungle and Glowing Mushroom are explicitly out; they each get their own
		// distinct progression paths and shouldn't host the standard surface biome.
		if (t.TileType == TileID.JungleGrass || t.TileType == TileID.MushroomGrass)
			return true;

		return false;
	}

	private static void PlaceVitalSoilBlob(int originX, int originY)
	{
		int vitalSoilType = ModContent.TileType<VitalSoilTile>();
		int radius = WorldGen.genRand.Next(6, 11);
		int boxRadius = (int)(radius * 1.5f) + 4; // Safely expand bounding box

		for (int y = -boxRadius; y <= boxRadius; y++)
		{
			for (int x = -boxRadius; x <= boxRadius; x++)
			{
				float nx = (float)x / radius;
				float ny = -(float)y / radius * 1.3f; // Squish y-axis so it fits the box and looks like a proper heart

				// Add smooth noise to coordinates
				float noise = (float)System.Math.Sin(x * 0.4f) * (float)System.Math.Cos(y * 0.4f) * 0.2f;
				nx += noise;
				ny += noise;

				float equation = (nx * nx + ny * ny - 1);
				if (equation * equation * equation - nx * nx * ny * ny * ny <= 0)
				{
					int cx = originX + x;
					int cy = originY + y;
					if (cx > 5 && cx < Main.maxTilesX - 5 && cy > 5 && cy < Main.maxTilesY - 5)
					{
						Tile t = Main.tile[cx, cy];
						if (t.HasTile) // Only replace existing terrain, don't fill air pockets!
						{
							t.TileType = (ushort)vitalSoilType;
						}
					}
				}
			}
		}

		// Post-pass: Encapsulate any Vital Soil that is exposed to air
		int checkRadius = boxRadius + 2;
		for (int i = originX - checkRadius; i <= originX + checkRadius; i++)
		{
			for (int j = originY - checkRadius; j <= originY + checkRadius; j++)
			{
				if (i < 5 || i > Main.maxTilesX - 5 || j < 5 || j > Main.maxTilesY - 5)
					continue;

				Tile t = Main.tile[i, j];
				if (t.HasTile && t.TileType == vitalSoilType)
				{
					// Check 8-way neighbors for air
					if (!Main.tile[i - 1, j].HasTile || 
						!Main.tile[i + 1, j].HasTile || 
						!Main.tile[i, j - 1].HasTile || 
						!Main.tile[i, j + 1].HasTile ||
						!Main.tile[i - 1, j - 1].HasTile || 
						!Main.tile[i + 1, j - 1].HasTile || 
						!Main.tile[i - 1, j + 1].HasTile || 
						!Main.tile[i + 1, j + 1].HasTile)
					{
						// Revert to a surrounding block type to form a natural shell
						ushort replaceType = TileID.Dirt;
						if (Main.tile[i - 1, j].HasTile && Main.tile[i - 1, j].TileType != vitalSoilType) replaceType = Main.tile[i - 1, j].TileType;
						else if (Main.tile[i + 1, j].HasTile && Main.tile[i + 1, j].TileType != vitalSoilType) replaceType = Main.tile[i + 1, j].TileType;
						else if (Main.tile[i, j + 1].HasTile && Main.tile[i, j + 1].TileType != vitalSoilType) replaceType = Main.tile[i, j + 1].TileType;
						else if (Main.tile[i, j - 1].HasTile && Main.tile[i, j - 1].TileType != vitalSoilType) replaceType = Main.tile[i, j - 1].TileType;
						
						t.TileType = replaceType;
					}
				}
			}
		}
	}

	// ── Jungle pass ──────────────────────────────────────────────────────────

	private void JungleBiomePass(GenerationProgress progress, GameConfiguration config)
	{
		progress.Message = Language.GetTextValue("Mods.ElementalHearts.WorldGen.VitalQuartzBiomes");

		int multiplier = ElementalHeartsWorldConfig.Instance.JungleBiomeCountMultiplier;
		if (multiplier <= 0)
			return;

		// Scale with world size, guarantee at least 3 at 1x multiplier
		int baseCount = System.Math.Max(3, Main.maxTilesX / 1400); 
		int target = baseCount * multiplier;

		int placed = 0;
		int attempts = 0;
		// Drastically increase max attempts because of strict eligibility requirements
		int maxAttempts = target * 10000;

		List<Microsoft.Xna.Framework.Vector2> placedPositions = new();
		const int MinSpacing = 150; // Ensure biomes aren't right next to each other

		while (placed < target && attempts < maxAttempts)
		{
			attempts++;
			progress.Set((float)placed / target);

			int x = WorldGen.genRand.Next(300, Main.maxTilesX - 300);
			int y = WorldGen.genRand.Next((int)Main.worldSurface + 40, Main.maxTilesY - 250);

			bool tooClose = false;
			foreach (var pos in placedPositions)
			{
				if (Microsoft.Xna.Framework.Vector2.Distance(pos, new Microsoft.Xna.Framework.Vector2(x, y)) < MinSpacing)
				{
					tooClose = true;
					break;
				}
			}

			if (tooClose)
				continue;

			if (!IsInJungle(x, y))
				continue;

			if (PlaceJungleHeart(x, y))
			{
				placedPositions.Add(new Microsoft.Xna.Framework.Vector2(x, y));
				placed++;
			}
		}
	}

	private static bool IsInJungle(int x, int y)
	{
		// To ensure we are actually DEEP inside the jungle and not just hitting a random
		// mud patch in the cavern layer, we scan a large area and require a substantial
		// amount of jungle blocks. Step by 3 for optimization.
		int jungleBlocks = 0;
		for (int dx = -30; dx <= 30; dx += 3)
		{
			for (int dy = -30; dy <= 30; dy += 3)
			{
				int nx = x + dx;
				int ny = y + dy;
				if (nx < 0 || nx >= Main.maxTilesX || ny < 0 || ny >= Main.maxTilesY)
					continue;

				Tile t = Main.tile[nx, ny];
				if (!t.HasTile)
					continue;

				if (t.TileType == TileID.MushroomGrass || t.TileType == TileID.LihzahrdBrick)
					return false; // Strongly reject glowing mushroom biomes and the Lihzahrd Temple

				if (t.TileType == TileID.JungleGrass || t.TileType == TileID.Mud)
				{
					jungleBlocks++;
					// We need at least 40 solid jungle hits (since we step by 3, this is roughly 360 actual blocks)
					if (jungleBlocks >= 40)
						return true;
				}
			}
		}

		return false;
	}

	private static void PlaceJungleHeartWall(int x, int y)
	{
		// Smooth noise method to mix Stone, Mud, and Jungle Grass walls
		float noise = (float)System.Math.Sin(x * 0.15f) * (float)System.Math.Cos(y * 0.15f) + 
					  (float)System.Math.Sin(x * 0.07f + y * 0.11f);
		
		Tile t = Main.tile[x, y];
		if (noise > 0.4f)
		{
			t.WallType = WallID.Stone;
		}
		else if (noise < -0.4f)
		{
			t.WallType = WallID.JungleUnsafe;
		}
		else
		{
			t.WallType = WallID.MudUnsafe;
		}
	}

	private static bool PlaceJungleHeart(int centerX, int centerY)
	{
		int Width = WorldGen.genRand.Next(30, 46);
		int Height = Width;
		bool[,] mask = HeartShape.Get(Width, Height);
		int vitalQuartzType = ModContent.TileType<VitalQuartzTile>();

		int originX = centerX - Width / 2;
		int originY = centerY - Height / 2;

		int radius = Width;

		// Create a smooth, wavy noise map for the organic blob outline
		float[] angles = new float[360];
		int numNodes = WorldGen.genRand.Next(5, 10);
		float[] nodeOffsets = new float[numNodes];
		for (int i = 0; i < numNodes; i++) 
			nodeOffsets[i] = WorldGen.genRand.NextFloat(0.7f, 1.4f);

		for(int i = 0; i < 360; i++)
		{
			float t = (i / 360f) * numNodes;
			int index1 = (int)t % numNodes;
			int index2 = (index1 + 1) % numNodes;
			float lerp = t - (int)t;
			
			// Smoothstep interpolation for natural blob shapes
			float smoothLerp = lerp * lerp * (3f - 2f * lerp);
			float multi = nodeOffsets[index1] * (1f - smoothLerp) + nodeOffsets[index2] * smoothLerp;
			
			// Add a tiny bit of roughness so it looks like Terraria generation
			angles[i] = radius * multi + WorldGen.genRand.NextFloat(-1.5f, 1.5f);
		}

		// 1. Pre-scan to ensure the area is suitable (embedded in terrain, not eating a floating island)
		int edgeCount = 0;
		int solidEdgeCount = 0;

		for (int dx = 0; dx < Width; dx++)
		{
			for (int dy = 0; dy < Height; dy++)
			{
				if (!mask[dx, dy])
					continue;

				bool isEdge = dx == 0 || dx == Width - 1 || dy == 0 || dy == Height - 1 ||
							  !mask[dx - 1, dy] || !mask[dx + 1, dy] || 
							  !mask[dx, dy - 1] || !mask[dx, dy + 1];

				if (isEdge)
				{
					edgeCount++;
					int tx = originX + dx;
					int ty = originY + dy;

					bool touchesSolid = false;
					for (int ox = -1; ox <= 1; ox++)
					{
						for (int oy = -1; oy <= 1; oy++)
						{
							if (ox == 0 && oy == 0) continue;
							int nx = tx + ox;
							int ny = ty + oy;
							
							if (nx >= 0 && nx < Main.maxTilesX && ny >= 0 && ny < Main.maxTilesY)
							{
								int ndx = dx + ox;
								int ndy = dy + oy;
								bool inMask = ndx >= 0 && ndx < Width && ndy >= 0 && ndy < Height && mask[ndx, ndy];
								
								// If the neighboring tile is OUTSIDE the heart mask
								if (!inMask)
								{
									Tile nTile = Main.tile[nx, ny];
									if (nTile.HasTile && Main.tileSolid[nTile.TileType])
									{
										touchesSolid = true;
									}
								}
							}
						}
					}
					
					if (touchesSolid)
						solidEdgeCount++;
				}
			}
		}

		// Require at least 75% of the heart's exterior boundary to be touching solid ground.
		// This guarantees it is fully embedded into the terrain and not hanging in an open cave.
		if (edgeCount == 0 || (float)solidEdgeCount / edgeCount < 0.75f)
			return false;

		// 2. Pre-scan organic blob to ensure enough blocks can be converted
		int validConvertCount = 0;
		int bounds = (int)(radius * 1.5f);
		for (int x = centerX - bounds; x <= centerX + bounds; x++)
		{
			for (int y = centerY - bounds; y <= centerY + bounds; y++)
			{
				if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
					continue;
				
				float dx = x - centerX;
				float dy = y - centerY;
				float dist = (float)System.Math.Sqrt(dx * dx + dy * dy);
				
				double angleRaw = System.Math.Atan2(dy, dx) * 180.0 / System.Math.PI;
				if (angleRaw < 0) angleRaw += 360.0;
				int angle = (int)angleRaw % 360;

				if (dist <= angles[angle])
				{
					Tile t = Main.tile[x, y];
					if (t.HasTile && (t.TileType == TileID.Stone || t.TileType == TileID.Mud || t.TileType == TileID.Dirt))
					{
						validConvertCount++;
					}
				}
			}
		}

		if (validConvertCount < 30)
			return false; // Not eligible, bail before modifying any tiles

		// 3. Actually convert the mud/dirt/stone using a curving vine/tendril algorithm
		float vineTwist = WorldGen.genRand.NextFloat(0.3f, 0.8f) * (WorldGen.genRand.NextBool() ? 1 : -1);
		int numVines = WorldGen.genRand.Next(10, 20);

		for (int x = centerX - bounds; x <= centerX + bounds; x++)
		{
			for (int y = centerY - bounds; y <= centerY + bounds; y++)
			{
				if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
					continue;
				
				float dx = x - centerX;
				float dy = y - centerY;
				float dist = (float)System.Math.Sqrt(dx * dx + dy * dy);
				
				double angleRaw = System.Math.Atan2(dy, dx) * 180.0 / System.Math.PI;
				if (angleRaw < 0) angleRaw += 360.0;
				int angle = (int)angleRaw % 360;

				float maxDist = angles[angle];
				
				if (dist <= maxDist)
				{
					float normalizedDist = dist / maxDist;
					
					// Calculate vine intensity based on angle and twisted distance
					float vineVal = (float)System.Math.Sin((angleRaw + dist * vineTwist) * numVines * System.Math.PI / 180f);
					
					bool convert = false;
					if (normalizedDist < 0.6f)
					{
						// Solid base around the heart, adding a little noise as it reaches 0.6
						if (WorldGen.genRand.NextFloat() > (normalizedDist - 0.4f) * 2.5f)
							convert = true;
						else if (vineVal > 0f)
							convert = true;
					}
					else
					{
						// Tendrils thinning out
						float threshold = MathHelper.Lerp(-0.2f, 0.9f, (normalizedDist - 0.6f) / 0.4f);
						threshold += WorldGen.genRand.NextFloat(-0.4f, 0.4f); // noise
						if (vineVal > threshold)
							convert = true;
					}

					if (convert)
					{
						Tile t = Main.tile[x, y];
						if (t.HasTile && (t.TileType == TileID.Stone || t.TileType == TileID.Mud || t.TileType == TileID.Dirt))
						{
							// Smooth noise to mix Stone (50%), Mud (25%), and Vital Quartz (25%)
							float noise = (float)System.Math.Sin(x * 0.12f) * (float)System.Math.Cos(y * 0.12f) + 
										  (float)System.Math.Sin(x * 0.08f + y * 0.11f);
							
							if (noise > 0f)
							{
								t.TileType = TileID.Stone;
							}
							else if (noise > -0.6f)
							{
								t.TileType = TileID.Mud;
							}
							else
							{
								t.TileType = (ushort)vitalQuartzType;
							}
						}
					}
				}
			}
		}

		int floorY = (int)(Height * 0.65f);

		for (int dx = 0; dx < Width; dx++)
		{
			for (int dy = 0; dy < Height; dy++)
			{
				if (!mask[dx, dy])
					continue;

				int tx = originX + dx;
				int ty = originY + dy;
				if (tx < 0 || tx >= Main.maxTilesX || ty < 0 || ty >= Main.maxTilesY)
					continue;

				bool isEdge = dx == 0 || dx == Width - 1 || dy == 0 || dy == Height - 1 ||
							  !mask[dx - 1, dy] || !mask[dx + 1, dy] || 
							  !mask[dx, dy - 1] || !mask[dx, dy + 1];

				Tile t = Main.tile[tx, ty];

				if (isEdge)
				{
					// Draw the heart boundary in life quartz
					t.HasTile = true;
					t.TileType = (ushort)vitalQuartzType;
					t.Slope = SlopeType.Solid;
					// Check if this edge touches outside air (existing caves)
					bool touchesOutsideAir = false;
					for (int ox = -1; ox <= 1; ox++)
					{
						for (int oy = -1; oy <= 1; oy++)
						{
							if (ox == 0 && oy == 0) continue;
							int nx = tx + ox;
							int ny = ty + oy;
							
							if (nx >= 0 && nx < Main.maxTilesX && ny >= 0 && ny < Main.maxTilesY)
							{
								int ndx = dx + ox;
								int ndy = dy + oy;
								bool inMask = ndx >= 0 && ndx < Width && ndy >= 0 && ndy < Height && mask[ndx, ndy];
								
								// If the neighboring tile is OUTSIDE the heart mask and is Air
								if (!inMask && !Main.tile[nx, ny].HasTile)
								{
									touchesOutsideAir = true;
									break;
								}
							}
						}
						if (touchesOutsideAir) break;
					}

					if (touchesOutsideAir)
					{
						// Remove any quartz/edge that is exposed to outside air.
						// This prevents artificial protruding shells and opens the heart up smoothly to existing caves!
						t.HasTile = false;
					}
					else
					{
						// Draw the heart boundary in life quartz
						t.HasTile = true;
						t.TileType = (ushort)vitalQuartzType;
						t.Slope = SlopeType.Solid;
						t.IsHalfBlock = false;
					}
					PlaceJungleHeartWall(tx, ty);
				}
				else if (dy >= floorY)
				{
					// Fill the entire bottom of the heart mask with mud so the floor connects naturally
					t.HasTile = true;
					t.TileType = TileID.Mud;
					t.Slope = SlopeType.Solid;
					t.IsHalfBlock = false;
					
					if (dy == floorY)
					{
						t.TileType = TileID.JungleGrass;
					}
					PlaceJungleHeartWall(tx, ty);
				}
				else
				{
					// A lot of air
					t.HasTile = false;
					t.LiquidAmount = 0; 
					PlaceJungleHeartWall(tx, ty);
				}
			}
		}

		// 5. Add vines hanging from the ceiling structure to add growth and life vibes
		for (int dx = 0; dx < Width; dx++)
		{
			for (int dy = 0; dy < floorY; dy++) // Only look above the floor
			{
				if (!mask[dx, dy]) continue;
				int tx = originX + dx;
				int ty = originY + dy;
				
				Tile t = Main.tile[tx, ty];
				Tile tAbove = Main.tile[tx, ty - 1];
				
				// If this tile is air, and the tile above is a solid tile
				if (!t.HasTile && tAbove.HasTile && Main.tileSolid[tAbove.TileType] && !tAbove.BottomSlope)
				{
					// High chance to spawn a vine
					if (WorldGen.genRand.NextBool(3))
					{
						int vineLength = WorldGen.genRand.Next(3, 15);
						for (int v = 0; v < vineLength; v++)
						{
							int vy = ty + v;
							if (vy >= originY + floorY || Main.tile[tx, vy].HasTile)
								break;
							
							Tile vineTile = Main.tile[tx, vy];
							vineTile.HasTile = true;
							vineTile.TileType = TileID.JungleVines;
						}
					}
				}
			}
		}

		// 6. Spawn jungle spores (3x likely) and life fruit (2x likely) in a 30-block radius
		int plantRadius = Width / 2 + 30;
		for (int x = centerX - plantRadius; x <= centerX + plantRadius; x++)
		{
			for (int y = centerY - plantRadius; y <= centerY + plantRadius; y++)
			{
				if (x < 5 || x >= Main.maxTilesX - 5 || y < 5 || y >= Main.maxTilesY - 5)
					continue;
				
				float dx = x - centerX;
				float dy = y - centerY;
				if (System.Math.Sqrt(dx * dx + dy * dy) <= plantRadius)
				{
					Tile t = Main.tile[x, y];
					Tile tAbove = Main.tile[x, y - 1];

					if (t.HasTile && t.TileType == TileID.JungleGrass && !tAbove.HasTile)
					{
						// Worldgen spawning: highly boosted chances compared to normal jungle
						// (Life Fruits are intentionally excluded from worldgen to respect Hardmode progression)
						if (WorldGen.genRand.NextBool(15)) // Spores
						{
							tAbove.HasTile = true;
							tAbove.TileType = TileID.JunglePlants;
							tAbove.TileFrameX = 162; // Spore frame
							tAbove.TileFrameY = 0;
						}
					}
				}
			}
		}

		PlaceGuaranteedLifeCrystalsInHeart(originX, originY, Width, Height, floorY);
		PlaceImpossibleHeartChest(originX, originY, Width, Height, floorY);
		PlaceRadiatingVitalSoil(originX, originY);
		return true;
	}

	private static void PlaceGuaranteedLifeCrystalsInHeart(int originX, int originY, int width, int height, int floorY)
	{
		int count = WorldGen.genRand.Next(3, 6);
		int placed = 0;
		int attempts = 0;

		while (placed < count && attempts < 500)
		{
			attempts++;

			int cx = originX + WorldGen.genRand.Next(5, width - 6);
			
			// Directly target the exact mud floor level instead of guessing with a drop-down loop.
			// This makes it physically impossible to spawn floating in midair.
			int cy = originY + floorY; 

			Tile t3 = Main.tile[cx, cy - 2];
			Tile t4 = Main.tile[cx + 1, cy - 2];
			Tile t1 = Main.tile[cx, cy - 1];
			Tile t2 = Main.tile[cx + 1, cy - 1];
			
			Tile f1 = Main.tile[cx, cy];
			Tile f2 = Main.tile[cx + 1, cy];

			// We need a flat 2-block wide floor
			if (f1.HasTile && Main.tileSolid[f1.TileType] && f2.HasTile && Main.tileSolid[f2.TileType])
			{
				// Ensure space is empty to avoid overlapping other life crystals
				if (t1.HasTile || t2.HasTile || t3.HasTile || t4.HasTile)
					continue;

				// Place top half
				t3.HasTile = true; t3.TileType = TileID.Heart; t3.TileFrameX = 0; t3.TileFrameY = 0;
				t4.HasTile = true; t4.TileType = TileID.Heart; t4.TileFrameX = 18; t4.TileFrameY = 0;
				
				// Place bottom half
				t1.HasTile = true; t1.TileType = TileID.Heart; t1.TileFrameX = 0; t1.TileFrameY = 18;
				t2.HasTile = true; t2.TileType = TileID.Heart; t2.TileFrameX = 18; t2.TileFrameY = 18;

				placed++;
			}
		}
	}

	// ── Life Crystal placement ───────────────────────────────────────────────

	private static void PlaceBonusLifeCrystals(int centerX, int centerY, int count)
	{
		int placed = 0;
		int attempts = 0;

		while (placed < count && attempts < 60)
		{
			attempts++;

			int cx = centerX + WorldGen.genRand.Next(-30, 31);
			int cy = centerY + WorldGen.genRand.Next(0, 40);

			if (TryPlaceLifeCrystal(cx, cy))
				placed++;
		}
	}

	private static bool TryPlaceLifeCrystal(int x, int y)
	{
		// Life Crystal is TileID.Heart, a 2-wide × 2-tall multitile that needs solid
		// ground beneath both halves. PlaceTile handles the multitile placement and
		// returns false if conditions aren't met, so we can just retry on failure.
		if (x < 5 || x >= Main.maxTilesX - 5 || y < 5 || y >= Main.maxTilesY - 5)
			return false;

		WorldGen.PlaceTile(x, y, TileID.Heart, mute: true, forced: false);

		Tile placed = Main.tile[x, y];
		return placed.HasTile && placed.TileType == TileID.Heart;
	}

	private static void PlaceImpossibleHeartChest(int originX, int originY, int width, int height, int floorY)
	{
		// 1. Build list of impossible hearts for this world
		System.Collections.Generic.List<int> impossibleHearts = new System.Collections.Generic.List<int>();

		// Ores
		if (WorldGen.SavedOreTiers.Copper == TileID.Copper)
			impossibleHearts.Add(ModContent.ItemType<TinHeart>());
		else
			impossibleHearts.Add(ModContent.ItemType<CopperHeart>());

		if (WorldGen.SavedOreTiers.Iron == TileID.Iron)
			impossibleHearts.Add(ModContent.ItemType<LeadHeart>());
		else
			impossibleHearts.Add(ModContent.ItemType<IronHeart>());

		if (WorldGen.SavedOreTiers.Silver == TileID.Silver)
			impossibleHearts.Add(ModContent.ItemType<TungstenHeart>());
		else
			impossibleHearts.Add(ModContent.ItemType<SilverHeart>());

		if (WorldGen.SavedOreTiers.Gold == TileID.Gold)
			impossibleHearts.Add(ModContent.ItemType<PlatinumHeart>());
		else
			impossibleHearts.Add(ModContent.ItemType<GoldHeart>());

		// Evils
		if (WorldGen.crimson)
		{
			// World is Crimson -> Corruption is impossible
			impossibleHearts.Add(ModContent.ItemType<EbonstoneHeart>());
			impossibleHearts.Add(ModContent.ItemType<EbonwoodHeart>());
			impossibleHearts.Add(ModContent.ItemType<EbonsandHeart>());
			impossibleHearts.Add(ModContent.ItemType<VileMushroomHeart>());
		}
		else
		{
			// World is Corruption -> Crimson is impossible
			impossibleHearts.Add(ModContent.ItemType<CrimstoneHeart>());
			impossibleHearts.Add(ModContent.ItemType<ShadewoodHeart>());
			impossibleHearts.Add(ModContent.ItemType<CrimsandHeart>());
			impossibleHearts.Add(ModContent.ItemType<ViciousMushroomHeart>());
		}

		int selectedHeart = impossibleHearts[WorldGen.genRand.Next(impossibleHearts.Count)];

		// 2. Find a spot on the mud floor to place the chest
		// Place near the center of the mud floor
		int chestX = originX + width / 2;
		int chestY = originY + floorY - 1; // Chest sits ON the floor

		// Ensure 2x2 area is clear for the chest
		for (int x = chestX; x < chestX + 2; x++)
		{
			for (int y = chestY - 1; y <= chestY; y++)
			{
				Tile t = Main.tile[x, y];
				t.HasTile = false;
				t.LiquidAmount = 0;
			}
		}

		// Place Ivy Chest (TileID 21, Style 10)
		int chestIndex = WorldGen.PlaceChest(chestX, chestY, 21, false, 10);
		if (chestIndex != -1)
		{
			Chest chest = Main.chest[chestIndex];
			int slot = 0;

			// Slot 0: Impossible Heart
			chest.item[slot].SetDefaults(selectedHeart);
			slot++;

			// Slot 1: Primary Jungle Loot
			int[] primaries = { ItemID.FeralClaws, ItemID.AnkletoftheWind, ItemID.StaffofRegrowth, ItemID.Boomstick, ItemID.FiberglassFishingPole };
			chest.item[slot].SetDefaults(primaries[WorldGen.genRand.Next(primaries.Length)]);
			slot++;

			// Slot 2: Secondary Jungle Loot (Jungle Spores / Stinger)
			if (WorldGen.genRand.NextBool())
			{
				chest.item[slot].SetDefaults(ItemID.JungleSpores);
				chest.item[slot].stack = WorldGen.genRand.Next(2, 6);
				slot++;
			}
			else
			{
				chest.item[slot].SetDefaults(ItemID.Stinger);
				chest.item[slot].stack = WorldGen.genRand.Next(2, 6);
				slot++;
			}

			// Slot 3: Potions
			int[] potions = { ItemID.HealingPotion, ItemID.SpelunkerPotion, ItemID.HunterPotion, ItemID.SwiftnessPotion };
			chest.item[slot].SetDefaults(potions[WorldGen.genRand.Next(potions.Length)]);
			chest.item[slot].stack = WorldGen.genRand.Next(2, 5);
			slot++;

			// Slot 4: Torches or Glowsticks
			if (WorldGen.genRand.NextBool())
				chest.item[slot].SetDefaults(ItemID.Torch);
			else
				chest.item[slot].SetDefaults(ItemID.Glowstick);
			chest.item[slot].stack = WorldGen.genRand.Next(15, 30);
			slot++;

			// Slot 5: Gold Coins
			chest.item[slot].SetDefaults(ItemID.GoldCoin);
			chest.item[slot].stack = WorldGen.genRand.Next(1, 4);
		}
	}

	private static void PlaceRadiatingVitalSoil(int originX, int originY)
	{
		int vitalSoilType = ModContent.TileType<VitalSoilTile>();
		int radius = 100;

		for (int x = -radius; x <= radius; x++)
		{
			for (int y = -radius; y <= radius; y++)
			{
				float distance = (float)System.Math.Sqrt(x * x + y * y);
				if (distance <= radius)
				{
					int cx = originX + x;
					int cy = originY + y;

					if (cx > 5 && cx < Main.maxTilesX - 5 && cy > 5 && cy < Main.maxTilesY - 5)
					{
						Tile t = Main.tile[cx, cy];
						// Only target Dirt and Mud to spread through the Jungle terrain
						if (t.HasTile && (t.TileType == TileID.Dirt || t.TileType == TileID.Mud))
						{
							// Calculate fade-out factor (1.0 at center, 0.0 at edge)
							float fade = 1f - (distance / radius);

							// Smooth noise to generate organic flowing veins
							float noise = (float)System.Math.Sin(cx * 0.12f) * (float)System.Math.Cos(cy * 0.12f) + 
										  (float)System.Math.Sin(cx * 0.07f + cy * 0.09f);

							// Threshold determines the density of the veins.
							// At the center (fade=1), threshold=0 -> 50% coverage.
							// As distance increases (fade->0), threshold increases up to 2.0 -> 0% coverage.
							// This causes the veins to naturally taper off and dissipate at the edges.
							float threshold = (1f - fade) * 2.0f;

							if (noise > threshold)
							{
								t.TileType = (ushort)vitalSoilType;
							}
						}
					}
				}
			}
		}
	}
}
