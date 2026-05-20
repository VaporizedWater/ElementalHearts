using ElementalHearts.Common.Hearts;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts;

/// <summary>
/// Boss-themed heart that cannot be crafted; dropped by its boss via
/// <see cref="Common.NPCs.BossHeartDropGlobalNPC"/>. Layers a boss-themed signature
/// sound on top of the base consume sound.
/// </summary>
public abstract class BossHeartItem : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Exotic;

	public sealed override void AddRecipes() { }

	/// <summary>Boss-themed signature sound. Override to give each boss its own cue.</summary>
	protected virtual SoundStyle BossConsumeSound => SoundID.Roar;

	protected override void PlayConsumeSound(Vector2 center)
	{
		base.PlayConsumeSound(center);
		SoundEngine.PlaySound(BossConsumeSound, center);
	}
}
