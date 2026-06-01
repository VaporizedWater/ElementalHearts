using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ElementalHearts.Common.Configs;
using ElementalHearts.Content.Items.Vanilla.Common;
using ElementalHearts.Content.Items.Vanilla.Uncommon;
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
		WorldSettings worldCfg = ElementalHeartsServerConfig.Instance.WorldGen;
		VitalTileSettings vitalCfg = ElementalHeartsServerConfig.Instance.VitalTiles;

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

		int multiplier = ElementalHeartsServerConfig.Instance.WorldGen.SurfaceBiomeCountMultiplier;
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

		// The Vital Soil shouldn't generate in Sand / Desert biomes. Sample a 15-block
		// radius, stepping by 3 — sand/snow biomes are large contiguous patches so a
		// sparse scan reliably catches them at ~9× less cost than a per-tile check.
		for (int i = x - 15; i <= x + 15; i += 3)
		{
			for (int j = y - 15; j <= y + 15; j += 3)
			{
				if (i >= 0 && i < Main.maxTilesX && j >= 0 && j < Main.maxTilesY)
				{
					Tile check = Main.tile[i, j];
					if (check.HasTile)
					{
						ushort type = check.TileType;
						if (type == TileID.Sand || type == TileID.Ebonsand || type == TileID.Crimsand || type == TileID.Pearlsand ||
							type == TileID.Sandstone || type == TileID.CorruptSandstone || type == TileID.CrimsonSandstone || type == TileID.HallowSandstone ||
							type == TileID.HardenedSand || type == TileID.CorruptHardenedSand || type == TileID.CrimsonHardenedSand || type == TileID.HallowHardenedSand ||
							type == TileID.SnowBlock || type == TileID.IceBlock || type == TileID.CorruptIce || type == TileID.FleshIce || type == TileID.HallowedIce)
						{
							return true;
						}
					}
				}
			}
		}

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
						if (t.HasTile)
						{
							ushort type = t.TileType;
							// Only replace base terrain to preserve surface grass blocks and prevent bald spots!
							if (type == TileID.Dirt || type == TileID.Stone || type == TileID.ClayBlock || type == TileID.Mud || type == TileID.Silt || type == TileID.Slush)
							{
								t.TileType = (ushort)vitalSoilType;
							}
						}
					}
				}
			}
		}

	}

	// ── Jungle pass ──────────────────────────────────────────────────────────

	private static System.Collections.Generic.List<int> _availableImpossibleHearts = new System.Collections.Generic.List<int>();

	private static void InitializeImpossibleHearts()
	{
		_availableImpossibleHearts.Clear();

		if (WorldGen.SavedOreTiers.Copper == TileID.Copper) _availableImpossibleHearts.Add(ModContent.ItemType<TinHeart>());
		else _availableImpossibleHearts.Add(ModContent.ItemType<CopperHeart>());

		if (WorldGen.SavedOreTiers.Iron == TileID.Iron) _availableImpossibleHearts.Add(ModContent.ItemType<LeadHeart>());
		else _availableImpossibleHearts.Add(ModContent.ItemType<IronHeart>());

		if (WorldGen.SavedOreTiers.Silver == TileID.Silver) _availableImpossibleHearts.Add(ModContent.ItemType<TungstenHeart>());
		else _availableImpossibleHearts.Add(ModContent.ItemType<SilverHeart>());

		if (WorldGen.SavedOreTiers.Gold == TileID.Gold) _availableImpossibleHearts.Add(ModContent.ItemType<PlatinumHeart>());
		else _availableImpossibleHearts.Add(ModContent.ItemType<GoldHeart>());

		if (WorldGen.crimson)
		{
			_availableImpossibleHearts.Add(ModContent.ItemType<EbonstoneHeart>());
			_availableImpossibleHearts.Add(ModContent.ItemType<EbonwoodHeart>());
			_availableImpossibleHearts.Add(ModContent.ItemType<EbonsandHeart>());
			_availableImpossibleHearts.Add(ModContent.ItemType<VileMushroomHeart>());
		}
		else
		{
			_availableImpossibleHearts.Add(ModContent.ItemType<CrimstoneHeart>());
			_availableImpossibleHearts.Add(ModContent.ItemType<ShadewoodHeart>());
			_availableImpossibleHearts.Add(ModContent.ItemType<CrimsandHeart>());
			_availableImpossibleHearts.Add(ModContent.ItemType<ViciousMushroomHeart>());
		}
	}

	private void JungleBiomePass(GenerationProgress progress, GameConfiguration config)
	{
		progress.Message = Language.GetTextValue("Mods.ElementalHearts.WorldGen.VitalQuartzBiomes");
		InitializeImpossibleHearts();

		int multiplier = ElementalHeartsServerConfig.Instance.WorldGen.JungleBiomeCountMultiplier;
		if (multiplier <= 0)
			return;

		int sets = 1;
		if (Main.maxTilesX > 6000) sets = 2; // Medium World
		if (Main.maxTilesX > 8000) sets = 3; // Large World
		int target = sets * 3 * multiplier;

		int placed = 0;
		int attempts = 0;
		// Drastically increase max attempts because of strict eligibility requirements
		int maxAttempts = target * 10000;

		List<Microsoft.Xna.Framework.Vector2> placedPositions = new();

		while (placed < target && attempts < maxAttempts)
		{
			attempts++;
			progress.Set((float)placed / target);

			int sizeCategory = placed % 3;
			
			int minY = (int)Main.worldSurface + 40;
			int maxY = System.Math.Max(minY + 3, Main.maxTilesY - 250);
			int depthTier = (maxY - minY) / 3;

			int x = WorldGen.genRand.Next(300, Main.maxTilesX - 300);
			int y;
			
			if (sizeCategory == 0) // Small, surface
			{
				y = WorldGen.genRand.Next(minY, minY + depthTier);
			}
			else if (sizeCategory == 1) // Medium, center
			{
				y = WorldGen.genRand.Next(minY + depthTier, minY + depthTier * 2);
			}
			else // Large, bottom
			{
				y = WorldGen.genRand.Next(minY + depthTier * 2, maxY);
			}

			// Simulated Annealing spacing: starts incredibly strict to force maximum spread,
			// but gracefully relaxes if the jungle is too crowded to prevent infinite loops.
			int currentMinSpacing = 600 - (attempts / 5);
			if (currentMinSpacing < 150) currentMinSpacing = 150;

			bool tooClose = false;
			float minSpacingSq = (float)currentMinSpacing * currentMinSpacing;
			foreach (var pos in placedPositions)
			{
				float ddx = pos.X - x;
				float ddy = pos.Y - y;
				if (ddx * ddx + ddy * ddy < minSpacingSq)
				{
					tooClose = true;
					break;
				}
			}

			if (tooClose)
				continue;

			if (!IsInJungle(x, y))
				continue;

			if (PlaceJungleHeart(x, y, sizeCategory))
			{
				placedPositions.Add(new Microsoft.Xna.Framework.Vector2(x, y));
				placed++;
				attempts = 0; // Reset attempts so the next heart gets the maximum spacing requirement!
			}
		}
	}

	private static bool BoundingBoxContainsChest(int originX, int originY, int width, int height)
	{
		// Chest top-left tile coords live in Main.chest[i].x/.y; chests are 2x2, so a chest
		// at (cx, cy) covers (cx..cx+1, cy..cy+1). Treat any overlap with the heart bbox
		// as a collision.
		int x0 = originX;
		int y0 = originY;
		int x1 = originX + width - 1;
		int y1 = originY + height - 1;

		for (int i = 0; i < Main.maxChests; i++)
		{
			Chest chest = Main.chest[i];
			if (chest == null)
				continue;

			int cx0 = chest.x;
			int cy0 = chest.y;
			int cx1 = cx0 + 1;
			int cy1 = cy0 + 1;

			if (cx1 >= x0 && cx0 <= x1 && cy1 >= y0 && cy0 <= y1)
				return true;
		}

		return false;
	}

	private static bool IsInJungle(int x, int y)
	{
		// Pre-scan a wider radius for biome contaminants we never want overlapping a
		// heart placement: glowing mushroom biomes and the Lihzahrd Temple. The radius
		// here is intentionally larger than the heart's worst-case footprint (size 2
		// hearts can reach ~46 wide, so radius ~23) plus a buffer so the heart can't
		// straddle the edge of a mushroom biome and end up with most of its candidate
		// tiles being MushroomGrass — those aren't in the conversion whitelist, which
		// previously caused canopies to spawn with too few Vital Quartz tiles to read
		// as a biome. This pass deliberately runs to completion (no early-out) so a
		// single contaminant anywhere in the scan window rejects the placement.
		for (int dx = -45; dx <= 45; dx += 3)
		{
			for (int dy = -45; dy <= 45; dy += 3)
			{
				int nx = x + dx;
				int ny = y + dy;
				if (nx < 0 || nx >= Main.maxTilesX || ny < 0 || ny >= Main.maxTilesY)
					continue;

				Tile t = Main.tile[nx, ny];
				if (!t.HasTile)
					continue;

				if (t.TileType == TileID.MushroomGrass || t.TileType == TileID.LihzahrdBrick)
					return false;
			}
		}

		// Then verify we're actually DEEP inside the jungle and not just hitting a
		// random mud patch in the cavern layer. Step by 3 for optimization; early-out
		// once we have enough hits.
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
			t.WallType = WallID.Cave6Unsafe;
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

	private static bool PlaceJungleHeart(int centerX, int centerY, int sizeCategory)
	{
		int minWidth, maxWidth;
		if (sizeCategory == 0)
		{
			minWidth = 18;
			maxWidth = 24;
		}
		else if (sizeCategory == 1)
		{
			minWidth = 25;
			maxWidth = 32;
		}
		else
		{
			minWidth = 33;
			maxWidth = 46;
		}

		int Width = WorldGen.genRand.Next(minWidth, maxWidth);
		int Height = Width;
		bool[,] mask = HeartShape.Get(Width, Height);
		int vitalQuartzType = ModContent.TileType<VitalQuartzTile>();
		int vitalSoilType = ModContent.TileType<VitalSoilTile>();

		// Tiles to call WorldGen.TileFrame on at the very end. Populated whenever a
		// vital tile is placed; the 3x3 around each placement is added so neighbors
		// reframe to merge cleanly. Replaces the old (W+10)x(H+10) scan that did the
		// same set membership check on every cell of the bounding box.
		HashSet<(int, int)> framePositions = new();
		void EnqueueFrame(int tx, int ty)
		{
			for (int fdx = -1; fdx <= 1; fdx++)
			{
				for (int fdy = -1; fdy <= 1; fdy++)
				{
					int ni = tx + fdx;
					int nj = ty + fdy;
					if (ni > 5 && ni < Main.maxTilesX - 5 && nj > 5 && nj < Main.maxTilesY - 5)
						framePositions.Add((ni, nj));
				}
			}
		}

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

		// Reject if a vanilla (or any other mod's) chest sits inside the heart's footprint.
		// The interior carve + forced 2x2 clear for the Impossible Heart chest would otherwise
		// stomp a pre-placed chest, destroying its loot. Bailing out here lets the placement
		// loop pick a different spot instead.
		if (BoundingBoxContainsChest(originX, originY, Width, Height))
			return false;

		// 2. Single-pass candidate gather: collect Stone/Mud/Dirt tiles inside the blob
		// outline. Atan2/Sqrt only run for tiles that could actually convert (skipping
		// air and unrelated terrain), and the conversion loop reuses the cached
		// dist/angleRaw instead of recomputing them. RNG is rolled only on candidates,
		// so seed-for-seed output differs slightly from the two-pass version — still
		// a procedurally valid heart, just laid out a hair differently.
		int bounds = (int)(radius * 1.5f);
		int boundsArea = (2 * bounds + 1) * (2 * bounds + 1);
		var candidates = new List<(int x, int y, float dist, double angleRaw, float maxDist)>(boundsArea / 4);

		for (int x = centerX - bounds; x <= centerX + bounds; x++)
		{
			for (int y = centerY - bounds; y <= centerY + bounds; y++)
			{
				if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
					continue;

				Tile t = Main.tile[x, y];
				if (!t.HasTile)
					continue;
				ushort tt = t.TileType;
				if (tt != TileID.Stone && tt != TileID.Mud && tt != TileID.Dirt)
					continue;

				float dx = x - centerX;
				float dy = y - centerY;
				float dist = (float)System.Math.Sqrt(dx * dx + dy * dy);

				double angleRaw = System.Math.Atan2(dy, dx) * 180.0 / System.Math.PI;
				if (angleRaw < 0) angleRaw += 360.0;
				int angle = (int)angleRaw % 360;

				float maxDist = angles[angle];
				if (dist > maxDist)
					continue;

				candidates.Add((x, y, dist, angleRaw, maxDist));
			}
		}

		if (candidates.Count < 30)
			return false; // Not eligible, bail before modifying any tiles

		// 3. Convert the gathered tiles using a curving vine/tendril algorithm
		float vineTwist = WorldGen.genRand.NextFloat(0.3f, 0.8f) * (WorldGen.genRand.NextBool() ? 1 : -1);
		int numVines = WorldGen.genRand.Next(10, 20);

		foreach (var c in candidates)
		{
			float normalizedDist = c.dist / c.maxDist;

			// Calculate vine intensity based on angle and twisted distance
			float vineVal = (float)System.Math.Sin((c.angleRaw + c.dist * vineTwist) * numVines * System.Math.PI / 180f);

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

			if (!convert)
				continue;

			Tile t = Main.tile[c.x, c.y];
			// Tile-type check is implicit — candidates were Stone/Mud/Dirt at gather
			// time. Could theoretically have changed if something else mutated tiles
			// between then and now, but nothing in this method does.

			// Smooth noise to mix Stone (50%), Mud (25%), and Vital Quartz (25%)
			float noise = (float)System.Math.Sin(c.x * 0.12f) * (float)System.Math.Cos(c.y * 0.12f) +
						  (float)System.Math.Sin(c.x * 0.08f + c.y * 0.11f);

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
				EnqueueFrame(c.x, c.y);
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
						EnqueueFrame(tx, ty);
					}
					PlaceJungleHeartWall(tx, ty);
				}
				else if (dy >= floorY)
				{
					// Fill the entire bottom of the heart mask
					t.HasTile = true;
					t.Slope = SlopeType.Solid;
					t.IsHalfBlock = false;
					
					if (dy == floorY)
					{
						t.TileType = TileID.JungleGrass;
					}
					else
					{
						// Smooth wavy pattern mixture of Mud, Quartz, and Soil
						float noise = (float)System.Math.Sin(tx * 0.25f) * (float)System.Math.Cos(ty * 0.25f) +
									  (float)System.Math.Sin(tx * 0.15f + ty * 0.1f);

						if (noise > 0.8f) { t.TileType = (ushort)vitalQuartzType; EnqueueFrame(tx, ty); }
						else if (noise > -0.2f) { t.TileType = (ushort)vitalSoilType; EnqueueFrame(tx, ty); }
						else t.TileType = TileID.Mud;
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

		// 5.5. Spawn Common Life Shards very rarely along the interior edges
		int shardType = ModContent.TileType<CommonLifeShardTile>();

		for (int dx = 0; dx < Width; dx++)
		{
			for (int dy = 0; dy < Height; dy++) // Check the whole interior
			{
				if (!mask[dx, dy]) continue;
				int tx = originX + dx;
				int ty = originY + dy;

				if (tx < 1 || tx >= Main.maxTilesX - 2 || ty < 1 || ty >= Main.maxTilesY - 2)
					continue;

				Tile t1 = Main.tile[tx, ty];
				Tile t2 = Main.tile[tx + 1, ty];
				Tile t3 = Main.tile[tx, ty + 1];
				Tile t4 = Main.tile[tx + 1, ty + 1];

				if (!t1.HasTile && !t2.HasTile && !t3.HasTile && !t4.HasTile)
				{
					if (WorldGen.genRand.NextBool(2)) // Drastically increased chance to compensate for strict flat surface requirements!
					{
						int baseFrameY = -1;

						// Check Floor
						if (Main.tile[tx, ty + 2].HasTile && Main.tile[tx, ty + 2].TileType == vitalQuartzType &&
							Main.tile[tx + 1, ty + 2].HasTile && Main.tile[tx + 1, ty + 2].TileType == vitalQuartzType)
						{
							baseFrameY = 0;
						}
						// Check Ceiling
						else if (Main.tile[tx, ty - 1].HasTile && Main.tile[tx, ty - 1].TileType == vitalQuartzType &&
								 Main.tile[tx + 1, ty - 1].HasTile && Main.tile[tx + 1, ty - 1].TileType == vitalQuartzType)
						{
							baseFrameY = 36;
						}
						// Check Left Wall
						else if (Main.tile[tx - 1, ty].HasTile && Main.tile[tx - 1, ty].TileType == vitalQuartzType &&
								 Main.tile[tx - 1, ty + 1].HasTile && Main.tile[tx - 1, ty + 1].TileType == vitalQuartzType)
						{
							baseFrameY = 108; // Points right
						}
						// Check Right Wall
						else if (Main.tile[tx + 2, ty].HasTile && Main.tile[tx + 2, ty].TileType == vitalQuartzType &&
								 Main.tile[tx + 2, ty + 1].HasTile && Main.tile[tx + 2, ty + 1].TileType == vitalQuartzType)
						{
							baseFrameY = 72; // Points left
						}

						if (baseFrameY != -1)
						{
							int style = WorldGen.genRand.Next(3);
							short frameX = (short)(style * 36);

							t1.HasTile = true; t1.TileType = (ushort)shardType; t1.TileFrameX = frameX; t1.TileFrameY = (short)baseFrameY;
							t2.HasTile = true; t2.TileType = (ushort)shardType; t2.TileFrameX = (short)(frameX + 18); t2.TileFrameY = (short)baseFrameY;
							t3.HasTile = true; t3.TileType = (ushort)shardType; t3.TileFrameX = frameX; t3.TileFrameY = (short)(baseFrameY + 18);
							t4.HasTile = true; t4.TileType = (ushort)shardType; t4.TileFrameX = (short)(frameX + 18); t4.TileFrameY = (short)(baseFrameY + 18);
						}
					}
				}
			}
		}

		// 6. Spawn jungle spores (3x likely) and life fruit (2x likely) in a 30-block radius
		int plantRadius = Width / 2 + 30;
		int plantRadiusSq = plantRadius * plantRadius;
		for (int x = centerX - plantRadius; x <= centerX + plantRadius; x++)
		{
			for (int y = centerY - plantRadius; y <= centerY + plantRadius; y++)
			{
				if (x < 5 || x >= Main.maxTilesX - 5 || y < 5 || y >= Main.maxTilesY - 5)
					continue;

				int dxP = x - centerX;
				int dyP = y - centerY;
				if (dxP * dxP + dyP * dyP <= plantRadiusSq)
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

		// Frame only tiles in the 3x3 neighborhood of each placed vital tile. Set membership
		// guarantees no tile is framed twice even when vital tiles are densely packed.
		// Tiles converted later by PlaceRadiatingVitalSoil aren't enqueued because VitalSoil
		// has tileBlendAll = true, so its visual seam with surrounding terrain is identical
		// regardless of frame state.
		foreach (var (i, j) in framePositions)
		{
			WorldGen.TileFrame(i, j, resetFrame: true, noBreak: true);
		}

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

			if (cx < 0 || cx >= Main.maxTilesX - 1 || cy < 2 || cy >= Main.maxTilesY)
				continue;

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
		if (_availableImpossibleHearts.Count == 0)
		{
			// Repopulate if we run out of unique impossible hearts (rare, but prevents crashes)
			InitializeImpossibleHearts();
		}

		int heartIndex = WorldGen.genRand.Next(_availableImpossibleHearts.Count);
		int selectedHeart = _availableImpossibleHearts[heartIndex];
		_availableImpossibleHearts.RemoveAt(heartIndex); // Ensure uniqueness!

		// 2. Find a spot on the mud floor to place the chest
		// Place near the center of the mud floor
		int chestX = originX + width / 2;
		int chestY = originY + floorY - 1; // Chest sits ON the floor

		if (chestX < 0 || chestX >= Main.maxTilesX - 1 || chestY < 1 || chestY >= Main.maxTilesY)
			return;

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

		ushort chestTileType = (ushort)ModContent.TileType<VitalChestTile>();
		int chestStyle = 0;

		int chestIndex = WorldGen.PlaceChest(chestX, chestY, chestTileType, false, chestStyle);
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
		int radiusSq = radius * radius;

		for (int x = -radius; x <= radius; x++)
		{
			for (int y = -radius; y <= radius; y++)
			{
				int distSq = x * x + y * y;
				if (distSq <= radiusSq)
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
							float distance = (float)System.Math.Sqrt(distSq);
							float fade = 1f - (distance / radius);

							// Smooth noise to generate organic flowing veins
							float noise = (float)System.Math.Sin(cx * 0.12f) * (float)System.Math.Cos(cy * 0.12f) + 
										  (float)System.Math.Sin(cx * 0.07f + cy * 0.09f);

							// Threshold determines the density of the veins.
							// Make patches much more rare (~10% coverage) so they look like thin growing vines/roots
							// At the center (fade=1), threshold=1.0 -> ~10% coverage.
							// As distance increases (fade->0), threshold increases up to 2.5 -> 0% coverage.
							float threshold = 1.0f + (1f - fade) * 1.5f;

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
