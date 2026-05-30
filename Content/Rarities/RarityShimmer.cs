using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace ElementalHearts.Content.Rarities;

/// <summary>
/// Shared time-based colour animation for the top-tier rarities, so a Legendary/Exotic/Mythic
/// heart's tooltip name visibly shimmers and reads as "earned" at a glance. Lower tiers stay
/// static on purpose — the motion is what keeps the top of the ladder feeling special.
/// </summary>
internal static class RarityShimmer
{
	/// <summary>Smooth ping-pong between two colours, driven by the shared game clock.</summary>
	public static Color Pulse(Color a, Color b, float speed, float offset = 0f)
	{
		float t = 0.5f + (0.5f * (float)Math.Sin((Main.GlobalTimeWrappedHourly * speed) + offset));
		return Color.Lerp(a, b, t);
	}
}
