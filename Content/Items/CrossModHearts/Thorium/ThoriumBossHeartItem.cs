using ElementalHearts.Content.Items.Hearts;
using ElementalHearts.Content.Items.CrossModHearts;
namespace ElementalHearts.Content.Items.CrossModHearts.Thorium;

/// <summary>
/// Base for Thorium boss-themed hearts. A <see cref="BossHeartItem"/> that also reports
/// its <see cref="ElementalHeartItem.SourceMod"/> so it obeys the per-mod load gate.
/// </summary>
public abstract class ThoriumBossHeartItem : BossHeartItem
{
	public override string SourceMod => "ThoriumMod";
}
