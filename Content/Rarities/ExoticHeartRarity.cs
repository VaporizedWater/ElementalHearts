using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Rarities;

public sealed class ExoticHeartRarity : ModRarity
{
	// Electric cyan that pulses toward pale aqua-white.
	public override Color RarityColor => RarityShimmer.Pulse(new(0, 240, 230), new(180, 255, 255), 4f);
}
