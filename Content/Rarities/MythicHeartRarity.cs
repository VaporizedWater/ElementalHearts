using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Rarities;

public sealed class MythicHeartRarity : ModRarity
{
	// The crown tier: a fast, radiant gold-to-white shimmer.
	public override Color RarityColor => RarityShimmer.Pulse(new(255, 220, 0), new(255, 255, 215), 4.5f);
}
