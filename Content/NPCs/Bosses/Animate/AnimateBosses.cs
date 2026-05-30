using ElementalHearts.Common.LifeShards;

using Terraria;
using Terraria.ModLoader;

namespace ElementalHearts.Content.NPCs.Bosses.Animate;

// RareAnimate (tier 2) is the full multi-body "Blue + Red/Green Enforcers" encounter,
// implemented in its own file (RareAnimate.cs). Epic and Legendary remain placeholders below.

[AutoloadBossHead]
public sealed class EpicAnimate : AnimateBoss
{
	public override int ProgressionTier => 3;
	public override LifeShardTier Tier => LifeShardTier.Epic;

	public override string Texture => "ElementalHearts/Content/Items/BossSpawns/EpicMenacingHeart";
	public override string BossHeadTexture => "ElementalHearts/Content/Items/BossSpawns/EpicMenacingHeart";

	public override void SetDefaults()
	{
		base.SetDefaults();
		if (!Main.dedServ)
		{
			string musicPath = "Music/EpicAnimateTheme";
			if (MusicLoader.MusicExists(Mod, musicPath))
			{
				Music = MusicLoader.GetMusicSlot(Mod, musicPath);
			}
		}
	}
}

[AutoloadBossHead]
public sealed class LegendaryAnimate : AnimateBoss
{
	public override int ProgressionTier => 4;
	public override LifeShardTier Tier => LifeShardTier.Legendary;

	public override string Texture => "ElementalHearts/Content/Items/BossSpawns/LegendaryMenacingHeart";
	public override string BossHeadTexture => "ElementalHearts/Content/Items/BossSpawns/LegendaryMenacingHeart";

	public override void SetDefaults()
	{
		base.SetDefaults();
		if (!Main.dedServ)
		{
			string musicPath = "Music/LegendaryAnimateTheme";
			if (MusicLoader.MusicExists(Mod, musicPath))
			{
				Music = MusicLoader.GetMusicSlot(Mod, musicPath);
			}
		}
	}
}
