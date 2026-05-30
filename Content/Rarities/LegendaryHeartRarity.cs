using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Rarities;

public sealed class LegendaryHeartRarity : ModRarity
{
	// Fiery flicker between deep and bright orange.
	public override Color RarityColor => RarityShimmer.Pulse(new(255, 100, 25), new(255, 175, 65), 3f);
}
