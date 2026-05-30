using ElementalHearts.Content.Items.Hearts;
using ElementalHearts.Content.Items.CrossModHearts;
namespace ElementalHearts.Content.Items.CrossModHearts.Consolaria;

/// <summary>
/// Base for Consolaria boss-themed hearts. A <see cref="BossHeartItem"/> that also reports
/// its <see cref="ElementalHeartItem.SourceMod"/> so it obeys the per-mod load gate.
/// </summary>
public abstract class ConsolariaBossHeartItem : BossHeartItem
{
	public override string SourceMod => "Consolaria";
}
