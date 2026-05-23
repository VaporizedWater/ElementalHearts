using ElementalHearts.Common.LifeShards;

using Terraria;
using Terraria.ModLoader;

namespace ElementalHearts.Content.NPCs.Bosses.Animate;

[AutoloadBossHead]
public sealed class RareAnimate : AnimateBoss
{
	public override int ProgressionTier => 2;
	public override LifeShardTier Tier => LifeShardTier.Rare;

	public override string Texture => "ElementalHearts/Content/Items/BossSpawns/RareMenacingHeart";
	public override string BossHeadTexture => "ElementalHearts/Content/Items/BossSpawns/RareMenacingHeart";

	public override void SetDefaults()
	{
		base.SetDefaults();
		if (!Main.dedServ)
		{
			Music = MusicLoader.GetMusicSlot(Mod, "Music/RareAnimateTheme");
		}
	}
}

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
			Music = MusicLoader.GetMusicSlot(Mod, "Music/EpicAnimateTheme");
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
			Music = MusicLoader.GetMusicSlot(Mod, "Music/LegendaryAnimateTheme");
		}
	}
}
