using ElementalHearts.Common.Biomes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Common.Systems;

/// <summary>
/// Sprinkles drifting leaf / life-essence particles around the local player while inside
/// the Vital Canopy. Pure client-side cosmetic — dust is gameplay-inert and replicates
/// itself nowhere, so this skips dedicated servers entirely.
/// </summary>
public sealed class VitalCanopyAmbience : ModSystem
{
	// Tuned for polished ambience: a handful of soft, slow particles spread over a
	// region wider than the visible screen so leaves and petals drift in from the
	// edges rather than popping in mid-view. Variety (leaves / grass / petals /
	// sparkles) sells the biome's "alive" feel without any one type dominating.
	private const int ParticlesPerFrame = 4;
	private const int HorizontalRange = 1400; // pixels each side of player
	private const int VerticalRange = 800;    // pixels above/below player

	public override void PostUpdateEverything()
	{
		if (Main.dedServ || Main.gamePaused)
			return;

		Player player = Main.LocalPlayer;
		if (player == null || !player.active)
			return;

		if (!player.InModBiome<VitalCanopyBiome>())
			return;

		for (int i = 0; i < ParticlesPerFrame; i++)
			SpawnAmbientParticle(player);
	}

	private static void SpawnAmbientParticle(Player player)
	{
		Vector2 pos = new(
			player.Center.X + Main.rand.Next(-HorizontalRange, HorizontalRange),
			player.Center.Y + Main.rand.Next(-VerticalRange, VerticalRange / 2));

		// Skip spawns inside solid tiles — leaves coming out of stone look broken.
		int tileX = (int)(pos.X / 16f);
		int tileY = (int)(pos.Y / 16f);
		if (tileX < 0 || tileX >= Main.maxTilesX || tileY < 0 || tileY >= Main.maxTilesY)
			return;
		Tile t = Main.tile[tileX, tileY];
		if (t.HasTile && Main.tileSolid[t.TileType])
			return;

		// Four flavours, weighted: drifting jungle leaves, fine grass blades, soft
		// flower petals (warm pink), and rare shiny life-essence sparkles. Roll out
		// of 20 keeps weights expressive without floating-point fiddling.
		int roll = Main.rand.Next(20);
		int dustType;
		float startScale; // scale at spawn
		float peakScale;  // scale to grow into via fadeIn (also extends visible life)
		Color colorOverride = default;
		int alpha;        // higher = more transparent; tunes how forward-or-back the mote sits
		bool sparkle = false;

		if (roll < 9) // 45% — leaves
		{
			dustType = DustID.JunglePlants;
			startScale = Main.rand.NextFloat(0.4f, 0.6f);
			peakScale = Main.rand.NextFloat(1.0f, 1.3f);
			alpha = 110;
		}
		else if (roll < 14) // 25% — grass blades
		{
			dustType = DustID.GrassBlades;
			startScale = Main.rand.NextFloat(0.35f, 0.55f);
			peakScale = Main.rand.NextFloat(0.9f, 1.1f);
			alpha = 120;
		}
		else if (roll < 18) // 20% — flower petals
		{
			dustType = DustID.PinkCrystalShard;
			startScale = Main.rand.NextFloat(0.45f, 0.65f);
			peakScale = Main.rand.NextFloat(1.1f, 1.4f);
			// Warm petal pink — distinct from the cooler shimmer below so the two
			// reads as "petals" and "sparkles" instead of the same particle twice.
			colorOverride = new Color(255, 180, 215);
			alpha = 90;
		}
		else // 10% — shiny life sparkle
		{
			dustType = DustID.AncientLight;
			startScale = Main.rand.NextFloat(0.5f, 0.7f);
			peakScale = Main.rand.NextFloat(1.0f, 1.3f);
			colorOverride = new Color(170, 255, 200);
			alpha = 60;
			sparkle = true;
		}

		Dust d = Dust.NewDustPerfect(pos, dustType, Vector2.Zero, 0, colorOverride, startScale);
		d.noGravity = true;
		// Sparkles emit a faint pinpoint glow; leaves/petals/grass stay light-inert so
		// they don't accidentally illuminate caves with hundreds of tiny lamps.
		d.noLight = !sparkle;
		// fadeIn > scale grows the dust each frame until it reaches fadeIn, then it
		// shrinks normally — this is what stretches the visible lifetime so particles
		// linger as polished atmosphere instead of flashing past.
		d.fadeIn = peakScale;
		d.alpha = alpha;

		// Slow, gentle drift — closer to floating motes than falling debris. Sparkles
		// barely move at all; petals waft a touch more than leaves.
		float swayRange = sparkle ? 0.05f : (dustType == DustID.PinkCrystalShard ? 0.25f : 0.15f);
		float fallMin = sparkle ? -0.05f : 0.05f;
		float fallMax = sparkle ? 0.05f : 0.25f;
		float sway = Main.rand.NextFloat(-swayRange, swayRange);
		float fall = Main.rand.NextFloat(fallMin, fallMax);
		d.velocity = new Vector2(sway, fall);
		d.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
	}
}
