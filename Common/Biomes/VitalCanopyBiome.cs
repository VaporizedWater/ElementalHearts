using ElementalHearts.Content.Tiles.Vital;
using Terraria;
using Terraria.ModLoader;

namespace ElementalHearts.Common.Biomes;

/// <summary>
/// Jungle sub-biome formed by the Vital Quartz heart formations. Activates when enough
/// Vital Quartz tiles appear in the player's scene-metrics scan while underground in the
/// jungle. Music, ambience, and modified spawns all key off <see cref="ModBiome"/> so
/// detection follows the same rules vanilla biomes use, instead of a hand-rolled tile
/// scan whose range disagreed with the worldgen footprint.
/// </summary>
public sealed class VitalCanopyBiome : ModBiome
{
	// Vital Quartz tiles needed inside the scene-metrics scan window for the biome to
	// register. A modest size-0 heart produces well over this; this threshold lets the
	// biome activate even when a chunk of the formation overlaps unconvertible terrain
	// (e.g. a mushroom-biome edge), which was the root cause of canopies failing to
	// register before the conversion to a real ModBiome.
	public const int RequiredQuartzCount = 7;

	public override SceneEffectPriority Priority => SceneEffectPriority.BiomeMedium;

	public override int Music
	{
		get
		{
			ElementalHearts mod = ModContent.GetInstance<ElementalHearts>();
			if (mod == null || !mod.MusicAutoloadingEnabled)
				return 0;

			string musicPath = "Music/VitalCanopyTheme";
			if (!MusicLoader.MusicExists(Mod, musicPath))
				return 0;

			return MusicLoader.GetMusicSlot(Mod, musicPath);
		}
	}

	public override bool IsBiomeActive(Player player)
	{
		if (!player.ZoneJungle)
			return false;

		ushort quartzType = (ushort)ModContent.TileType<VitalQuartzTile>();
		int tileX = (int)(player.Center.X / 16);
		int tileY = (int)(player.Center.Y / 16);
		int radius = 12;
		int count = 0;

		for (int i = tileX - radius; i <= tileX + radius; i++)
		{
			for (int j = tileY - radius; j <= tileY + radius; j++)
			{
				if (WorldGen.InWorld(i, j) && Main.tile[i, j].HasTile && Main.tile[i, j].TileType == quartzType)
				{
					count++;
					if (count >= RequiredQuartzCount)
						return true;
				}
			}
		}

		return false;
	}
}
