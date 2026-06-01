using System.Collections.Generic;
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
/// Post-chest-placement worldgen pass: rolls 10% on every plain Wooden Chest (TileID 21, style 0)
/// and converts winners into a <see cref="VitalChestTile"/>. The chest keeps all its original loot;
/// only slot 0 is overwritten with an ElementalHearts heart matching the chest's biome of origin
/// (cavern → cavern heart, surface → surface heart, jungle → jungle heart, etc.).
/// </summary>
public sealed class VitalChestConversionSystem : ModSystem
{
	public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
	{
		// Worldgen tiles only exist if the tile system is enabled.
		if (!VitalTilesConfig.Instance.SystemEnabled)
			return;

		// "Buried Chests" is vanilla's main chest placement pass. We slot in just after the
		// later "Buried Chests Tin" pass so every wooden chest in the world (surface, cavern,
		// biome chests) is already present when we sweep through.
		int idx = tasks.FindIndex(p => p.Name == "Final Cleanup");
		if (idx == -1)
			idx = tasks.Count - 1;

		tasks.Insert(idx, new PassLegacy("Vital Chests", ConversionPass, 0.5));
	}

	private static void ConversionPass(GenerationProgress progress, GameConfiguration config)
	{
		progress.Message = Language.GetTextValue("Mods.ElementalHearts.WorldGen.VitalChests");

		int vitalChestType = ModContent.TileType<VitalChestTile>();
		int vitalQuartzType = ModContent.TileType<VitalQuartzTile>();
		int vitalSoilType = ModContent.TileType<VitalSoilTile>();
		int commonShardType = ModContent.TileType<CommonLifeShardTile>();

		for (int chestIndex = 0; chestIndex < Main.maxChests; chestIndex++)
		{
			Chest chest = Main.chest[chestIndex];
			if (chest == null)
				continue;

			int x = chest.x;
			int y = chest.y;

			Tile anchor = Main.tile[x, y];
			if (!anchor.HasTile)
				continue;

			// Defensive: never touch a Vital Chest. A correct filter on TileType below would
			// already skip these, but this makes the intent obvious to future readers.
			if (anchor.TileType == vitalChestType)
				continue;

			// Only convert vanilla chest tiles. Vanilla chest styles span 36 px each in
			// the sprite sheet, so style = frameX / 36.
			if (anchor.TileType != TileID.Containers)
				continue;
			int style = anchor.TileFrameX / 36;

			// Style 0 = Wooden Chest (10% upgrade); styles 1/7/11 = Gold / Rich Mahogany /
			// Frozen Chest (rarer 5% upgrade — they're already biome-themed loot).
			int rollDenominator;
			switch (style)
			{
				case 0: rollDenominator = 10; break;   // Wooden
				case 1:                                // Gold
				case 7:                                // Rich Mahogany
				case 11: rollDenominator = 20; break;  // Frozen
				default: continue;
			}

			// Skip any chest sitting inside a Vital Canopy / Vital Soil patch. Vanilla
			// `Buried Chests` runs after our biome passes and will happily drop a chest into
			// the cavity left by an Impossible Heart heart — converting that chest would
			// effectively "stomp" the canopy's intended chest. Detect by scanning a small
			// radius for any of our vital tiles.
			if (IsInVitalBiome(x, y, vitalQuartzType, vitalSoilType, commonShardType, vitalChestType))
				continue;

			if (!WorldGen.genRand.NextBool(rollDenominator))
				continue;

			ConvertChest(x, y, vitalChestType);

			int heartType = PickBiomeHeart(x, y);
			if (heartType > 0)
			{
				chest.item[0].SetDefaults(heartType);
				chest.item[0].stack = 1;
			}
		}
	}

	private static bool IsInVitalBiome(int x, int y, int vitalQuartzType, int vitalSoilType, int commonShardType, int vitalChestType)
	{
		const int radius = 25;
		for (int i = x - radius; i <= x + radius; i += 2)
		{
			for (int j = y - radius; j <= y + radius; j += 2)
			{
				if (i < 0 || i >= Main.maxTilesX || j < 0 || j >= Main.maxTilesY)
					continue;
				Tile t = Main.tile[i, j];
				if (!t.HasTile)
					continue;
				ushort type = t.TileType;
				if (type == vitalQuartzType || type == vitalSoilType || type == commonShardType || type == vitalChestType)
					return true;
			}
		}
		return false;
	}

	private static void ConvertChest(int chestX, int chestY, int vitalChestType)
	{
		// Rewrite the underlying 2×2 tile footprint to point at Vital Chest style 0 while
		// keeping the chest entry in Main.chest intact (so its inventory is preserved).
		for (int dx = 0; dx < 2; dx++)
		{
			for (int dy = 0; dy < 2; dy++)
			{
				Tile t = Main.tile[chestX + dx, chestY + dy];
				t.TileType = (ushort)vitalChestType;
				t.TileFrameX = (short)(dx * 18);
				t.TileFrameY = (short)(dy * 18);
			}
		}
	}

	// ── Biome detection ─────────────────────────────────────────────────────

	private enum ChestBiome
	{
		Sky,
		Surface,
		Underground,
		Cavern,
		Hell,
		Jungle,
		Snow,
		Desert,
		Corruption,
		Crimson,
		Hallow,
		Mushroom,
		Ocean,
		Dungeon,
	}

	private static ChestBiome ClassifyBiome(int x, int y)
	{
		// Vertical bands first — they're absolute and override surface biome scans.
		if (y > Main.maxTilesY - 200)
			return ChestBiome.Hell;
		if (y < Main.worldSurface - 50)
			return ChestBiome.Sky;

		// Scan a small radius around the chest for biome-defining tiles. Step by 3 to
		// keep the sweep cheap — worldgen biome regions are large contiguous patches.
		int dungeon = 0, jungle = 0, snow = 0, desert = 0;
		int corrupt = 0, crimson = 0, hallow = 0, mushroom = 0, ocean = 0;

		const int radius = 30;
		for (int i = x - radius; i <= x + radius; i += 3)
		{
			for (int j = y - radius; j <= y + radius; j += 3)
			{
				if (i < 0 || i >= Main.maxTilesX || j < 0 || j >= Main.maxTilesY)
					continue;

				Tile t = Main.tile[i, j];
				if (!t.HasTile)
					continue;

				ushort type = t.TileType;

				switch (type)
				{
					case TileID.BlueDungeonBrick:
					case TileID.GreenDungeonBrick:
					case TileID.PinkDungeonBrick:
					case TileID.LihzahrdBrick:
						dungeon++;
						break;
					case TileID.JungleGrass:
					case TileID.Mud:
						jungle++;
						break;
					case TileID.SnowBlock:
					case TileID.IceBlock:
					case TileID.CorruptIce:
					case TileID.FleshIce:
					case TileID.HallowedIce:
						snow++;
						break;
					case TileID.Sand:
					case TileID.HardenedSand:
					case TileID.Sandstone:
						desert++;
						break;
					case TileID.Ebonstone:
					case TileID.Ebonsand:
					case TileID.CorruptHardenedSand:
					case TileID.CorruptSandstone:
					case TileID.CorruptGrass:
						corrupt++;
						break;
					case TileID.Crimstone:
					case TileID.Crimsand:
					case TileID.CrimsonHardenedSand:
					case TileID.CrimsonSandstone:
					case TileID.CrimsonGrass:
						crimson++;
						break;
					case TileID.Pearlstone:
					case TileID.Pearlsand:
					case TileID.HallowHardenedSand:
					case TileID.HallowSandstone:
					case TileID.HallowedGrass:
						hallow++;
						break;
					case TileID.MushroomGrass:
						mushroom++;
						break;
				}
			}
		}

		// Ocean detection: close to world edge with water nearby.
		if ((x < 380 || x > Main.maxTilesX - 380) && y < Main.worldSurface + 50)
		{
			for (int i = x - 20; i <= x + 20; i += 3)
			{
				for (int j = y - 10; j <= y + 20; j += 3)
				{
					if (i < 0 || i >= Main.maxTilesX || j < 0 || j >= Main.maxTilesY)
						continue;
					if (Main.tile[i, j].LiquidAmount > 0 && Main.tile[i, j].LiquidType == LiquidID.Water)
						ocean++;
				}
			}
		}

		// Pick the strongest biome signal. Dungeon and structural biomes win outright if
		// their characteristic blocks show up at all near the chest.
		if (dungeon >= 3)
			return ChestBiome.Dungeon;
		if (mushroom >= 4)
			return ChestBiome.Mushroom;

		int max = 0;
		ChestBiome winner = ChestBiome.Surface;
		void Consider(int count, ChestBiome b) { if (count > max) { max = count; winner = b; } }

		Consider(jungle, ChestBiome.Jungle);
		Consider(snow, ChestBiome.Snow);
		Consider(desert, ChestBiome.Desert);
		Consider(corrupt, ChestBiome.Corruption);
		Consider(crimson, ChestBiome.Crimson);
		Consider(hallow, ChestBiome.Hallow);
		Consider(ocean, ChestBiome.Ocean);

		if (max >= 5)
			return winner;

		// Fall back to depth bands when no specific biome dominates.
		if (y < Main.worldSurface)
			return ChestBiome.Surface;
		if (y < Main.rockLayer)
			return ChestBiome.Underground;
		return ChestBiome.Cavern;
	}

	// ── Heart pools ─────────────────────────────────────────────────────────

	private static int PickBiomeHeart(int x, int y)
	{
		ChestBiome biome = ClassifyBiome(x, y);
		int[] pool = PoolFor(biome);
		if (pool == null || pool.Length == 0)
			return 0;
		return pool[WorldGen.genRand.Next(pool.Length)];
	}

	private static int[] PoolFor(ChestBiome biome) => biome switch
	{
		ChestBiome.Sky => new[] {
			ModContent.ItemType<CloudHeart>(),
			ModContent.ItemType<RainCloudHeart>(),
			ModContent.ItemType<SnowCloudHeart>(),
			ModContent.ItemType<SunplateHeart>(),
		},
		ChestBiome.Hell => new[] {
			ModContent.ItemType<ObsidianHeart>(),
			ModContent.ItemType<FleshHeart>(),
			ModContent.ItemType<FireblossomHeart>(),
		},
		ChestBiome.Jungle => new[] {
			ModContent.ItemType<RichMahoganyHeart>(),
			ModContent.ItemType<MudHeart>(),
			ModContent.ItemType<MoonglowHeart>(),
			ModContent.ItemType<GlowingMushroomHeart>(),
		},
		ChestBiome.Snow => new[] {
			ModContent.ItemType<IceHeart>(),
			ModContent.ItemType<PinkIceHeart>(),
			ModContent.ItemType<PurpleIceHeart>(),
			ModContent.ItemType<RedIceHeart>(),
			ModContent.ItemType<SnowHeart>(),
			ModContent.ItemType<BorealWoodHeart>(),
			ModContent.ItemType<ShiverthornHeart>(),
		},
		ChestBiome.Desert => new[] {
			ModContent.ItemType<SandHeart>(),
			ModContent.ItemType<PalmWoodHeart>(),
			ModContent.ItemType<CactusHeart>(),
			ModContent.ItemType<FossilHeart>(),
			ModContent.ItemType<WaterleafHeart>(),
		},
		ChestBiome.Corruption => new[] {
			ModContent.ItemType<EbonstoneHeart>(),
			ModContent.ItemType<EbonsandHeart>(),
			ModContent.ItemType<EbonwoodHeart>(),
			ModContent.ItemType<DeathweedHeart>(),
			ModContent.ItemType<VileMushroomHeart>(),
		},
		ChestBiome.Crimson => new[] {
			ModContent.ItemType<CrimstoneHeart>(),
			ModContent.ItemType<CrimsandHeart>(),
			ModContent.ItemType<ShadewoodHeart>(),
			ModContent.ItemType<FleshHeart>(),
			ModContent.ItemType<DeathweedHeart>(),
			ModContent.ItemType<ViciousMushroomHeart>(),
		},
		ChestBiome.Hallow => new[] {
			ModContent.ItemType<PearlstoneHeart>(),
			ModContent.ItemType<PearlsandHeart>(),
			ModContent.ItemType<PearlwoodHeart>(),
			ModContent.ItemType<PinkIceHeart>(),
		},
		ChestBiome.Mushroom => new[] {
			ModContent.ItemType<GlowingMushroomHeart>(),
			ModContent.ItemType<MushroomHeart>(),
		},
		ChestBiome.Ocean => new[] {
			ModContent.ItemType<CoralstoneHeart>(),
			ModContent.ItemType<BubbleHeart>(),
			ModContent.ItemType<PalmWoodHeart>(),
			ModContent.ItemType<SandHeart>(),
		},
		ChestBiome.Dungeon => new[] {
			ModContent.ItemType<StoneHeart>(),
			ModContent.ItemType<ObsidianHeart>(),
			ModContent.ItemType<EnchantedHeart>(),
		},
		ChestBiome.Surface => new[] {
			ModContent.ItemType<WoodHeart>(),
			ModContent.ItemType<DirtHeart>(),
			ModContent.ItemType<HayHeart>(),
			ModContent.ItemType<PumpkinHeart>(),
			ModContent.ItemType<DaybloomHeart>(),
			ModContent.ItemType<BlinkrootHeart>(),
		},
		ChestBiome.Underground => new[] {
			ModContent.ItemType<StoneHeart>(),
			ModContent.ItemType<DirtHeart>(),
			ModContent.ItemType<CopperHeart>(),
			ModContent.ItemType<TinHeart>(),
			ModContent.ItemType<IronHeart>(),
			ModContent.ItemType<LeadHeart>(),
			ModContent.ItemType<BlinkrootHeart>(),
		},
		ChestBiome.Cavern => new[] {
			ModContent.ItemType<StoneHeart>(),
			ModContent.ItemType<GraniteHeart>(),
			ModContent.ItemType<MarbleHeart>(),
			ModContent.ItemType<AmethystHeart>(),
			ModContent.ItemType<TopazHeart>(),
			ModContent.ItemType<SapphireHeart>(),
			ModContent.ItemType<EmeraldHeart>(),
			ModContent.ItemType<RubyHeart>(),
			ModContent.ItemType<SilverHeart>(),
			ModContent.ItemType<TungstenHeart>(),
			ModContent.ItemType<GoldHeart>(),
			ModContent.ItemType<PlatinumHeart>(),
			ModContent.ItemType<MushroomHeart>(),
			ModContent.ItemType<FireblossomHeart>(),
		},
		_ => null,
	};
}
