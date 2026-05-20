namespace ElementalHearts.Content.Items.Hearts.CrossMod.Calamity;

/// <summary>
/// Base for Calamity boss-themed hearts. A <see cref="BossHeartItem"/> that also reports
/// its <see cref="ElementalHeartItem.SourceMod"/> so it obeys the per-mod load gate.
/// </summary>
public abstract class CalamityBossHeartItem : BossHeartItem
{
	public override string SourceMod => "CalamityMod";
}
