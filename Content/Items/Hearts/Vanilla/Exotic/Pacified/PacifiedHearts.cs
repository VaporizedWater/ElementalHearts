using ElementalHearts.Common.Hearts;
using Terraria.Audio;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Items.Hearts.Vanilla.Exotic.Pacified;

public sealed class CommonPacifiedHeart : BossHeartItem
{
	public override HeartTier Tier => HeartTier.Common;
	protected override SoundStyle BossConsumeSound => new SoundStyle($"ElementalHearts/Sounds/{Tier}BossItemUsed");
}

public sealed class UncommonPacifiedHeart : BossHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	protected override SoundStyle BossConsumeSound => new SoundStyle($"ElementalHearts/Sounds/{Tier}BossItemUsed");
}

public sealed class RarePacifiedHeart : BossHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;
	protected override SoundStyle BossConsumeSound => new SoundStyle($"ElementalHearts/Sounds/{Tier}BossItemUsed");
}

public sealed class EpicPacifiedHeart : BossHeartItem
{
	public override HeartTier Tier => HeartTier.Epic;
	protected override SoundStyle BossConsumeSound => new SoundStyle($"ElementalHearts/Sounds/{Tier}BossItemUsed");
}

public sealed class LegendaryPacifiedHeart : BossHeartItem
{
	public override HeartTier Tier => HeartTier.Legendary;
	protected override SoundStyle BossConsumeSound => new SoundStyle($"ElementalHearts/Sounds/{Tier}BossItemUsed");
}
