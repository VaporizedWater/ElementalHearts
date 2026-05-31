using ElementalHearts.Common.Hearts;
using Terraria.Audio;
using Terraria.ModLoader;

using ElementalHearts.Content.Items.Hearts;

// Category folder (like Potions/): a themed family that spans tiers Common→Legendary, so it
// lives under a theme name rather than any single tier folder. Each class still declares its own
// Tier; the consumed-heart identity is the class name, unaffected by this namespace.
namespace ElementalHearts.Content.Items.Vanilla.Pacified;

public sealed class CommonPacifiedHeart : BossHeartItem
{
	public override HeartTier Tier => HeartTier.Common;
	protected override SoundStyle BossConsumeSound => new SoundStyle($"ElementalHearts/Assets/Sounds/{Tier}BossItemUsed");
}

public sealed class UncommonPacifiedHeart : BossHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	protected override SoundStyle BossConsumeSound => new SoundStyle($"ElementalHearts/Assets/Sounds/{Tier}BossItemUsed");
}

public sealed class RarePacifiedHeart : BossHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;
	protected override SoundStyle BossConsumeSound => new SoundStyle($"ElementalHearts/Assets/Sounds/{Tier}BossItemUsed");
}

public sealed class EpicPacifiedHeart : BossHeartItem
{
	public override HeartTier Tier => HeartTier.Epic;
	protected override SoundStyle BossConsumeSound => new SoundStyle($"ElementalHearts/Assets/Sounds/{Tier}BossItemUsed");
}

public sealed class LegendaryPacifiedHeart : BossHeartItem
{
	public override HeartTier Tier => HeartTier.Legendary;
	protected override SoundStyle BossConsumeSound => new SoundStyle($"ElementalHearts/Assets/Sounds/{Tier}BossItemUsed");
}
