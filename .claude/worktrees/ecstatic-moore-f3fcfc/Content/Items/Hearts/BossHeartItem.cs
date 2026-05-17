using ElementalHearts.Common.Hearts;

namespace ElementalHearts.Content.Items.Hearts;

/// <summary>
/// Boss-themed heart that cannot be crafted; dropped by its boss via
/// <see cref="Common.NPCs.BossHeartDropGlobalNPC"/>.
/// </summary>
public abstract class BossHeartItem : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Exotic;

	public sealed override void AddRecipes() { }
}
