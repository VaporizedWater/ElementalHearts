using ElementalHearts.Common.LifeShards;

namespace ElementalHearts.Content.NPCs.Bosses.Animate;

public sealed class CommonAnimate : AnimateBoss
{
	public override int ProgressionTier => 0;
	public override LifeShardTier Tier => LifeShardTier.Common;
}

public sealed class UncommonAnimate : AnimateBoss
{
	public override int ProgressionTier => 1;
	public override LifeShardTier Tier => LifeShardTier.Uncommon;
}

public sealed class RareAnimate : AnimateBoss
{
	public override int ProgressionTier => 2;
	public override LifeShardTier Tier => LifeShardTier.Rare;
}

public sealed class EpicAnimate : AnimateBoss
{
	public override int ProgressionTier => 3;
	public override LifeShardTier Tier => LifeShardTier.Epic;
}

public sealed class LegendaryAnimate : AnimateBoss
{
	public override int ProgressionTier => 4;
	public override LifeShardTier Tier => LifeShardTier.Legendary;
}
