using ElementalHearts.Common.LifeShards;
using ElementalHearts.Content.NPCs.Bosses.Animate;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Items.BossSpawns;

public sealed class CommonMenacingHeart : MenacingHeartItem
{
	public override LifeShardTier Tier => LifeShardTier.Common;
	public override int NPCSpawnType => ModContent.NPCType<CommonAnimate>();
}

public sealed class UncommonMenacingHeart : MenacingHeartItem
{
	public override LifeShardTier Tier => LifeShardTier.Uncommon;
	public override int NPCSpawnType => ModContent.NPCType<UncommonAnimate>();
}

public sealed class RareMenacingHeart : MenacingHeartItem
{
	public override LifeShardTier Tier => LifeShardTier.Rare;
	public override int NPCSpawnType => ModContent.NPCType<RareAnimate>();
}

public sealed class EpicMenacingHeart : MenacingHeartItem
{
	public override LifeShardTier Tier => LifeShardTier.Epic;
	public override int NPCSpawnType => ModContent.NPCType<EpicAnimate>();
}

public sealed class LegendaryMenacingHeart : MenacingHeartItem
{
	public override LifeShardTier Tier => LifeShardTier.Legendary;
	public override int NPCSpawnType => ModContent.NPCType<LegendaryAnimate>();
}
