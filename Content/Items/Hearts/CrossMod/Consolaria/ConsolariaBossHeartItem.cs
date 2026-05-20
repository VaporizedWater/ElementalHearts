namespace ElementalHearts.Content.Items.Hearts.CrossMod.Consolaria;

/// <summary>
/// Base for Consolaria boss-themed hearts. A <see cref="BossHeartItem"/> that also reports
/// its <see cref="ElementalHeartItem.SourceMod"/> so it obeys the per-mod load gate.
/// </summary>
public abstract class ConsolariaBossHeartItem : BossHeartItem
{
	public override string SourceMod => "Consolaria";
}
