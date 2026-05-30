using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using ElementalHearts.Content.NPCs.Bosses.Animate;

namespace ElementalHearts.Content.Projectiles;

/// <summary>
/// The Phase 3 "Jump Rope" deathray. The trio merges at the master and fires one long
/// beam pointing directly away from the player, then the master rotates it a full 360°
/// over five seconds — forcing the player to sprint a wide circle to outpace the sweep
/// while threading the Heart Traps left over from the Box-In.
/// <para/>
/// The master is the sole authority: it writes this projectile's origin (its own Center),
/// direction (<see cref="Projectile.localAI"/>[1]), length (<see cref="Projectile.ai"/>[1]),
/// and firing state (<see cref="Projectile.localAI"/>[0]) every tick, and refreshes
/// <see cref="Projectile.timeLeft"/> so the beam dies the instant the master stops driving it.
/// </summary>
public sealed class RareDeathrayProjectile : ModProjectile
{
	public override string Texture => "ElementalHearts/Content/Items/BossSpawns/RareMenacingHeart";

	// ai[0] = master whoAmI
	// ai[1] = beam length (px) and firing state (positive = firing, negative = telegraph)
	// velocity = beam direction (syncs natively)

	public const float DefaultLength = 2600f;
	private const float CollisionWidth = 30f;

	private bool Firing => Projectile.ai[1] > 0f;
	private float Length => Math.Abs(Projectile.ai[1]) > 0f ? Math.Abs(Projectile.ai[1]) : DefaultLength;
	private Vector2 BeamDir => Projectile.velocity == Vector2.Zero ? Vector2.UnitX : Projectile.velocity;



	public override void SetDefaults()
	{
		Projectile.width = 18;
		Projectile.height = 18;
		Projectile.hostile = true;
		Projectile.friendly = false;
		Projectile.penetrate = -1;
		Projectile.tileCollide = false;
		Projectile.ignoreWater = true;
		Projectile.timeLeft = 2;          // master keeps it topped up; dies fast once it stops
		Projectile.scale = 1f;
		Projectile.netImportant = true;
	}

	public override void AI()
	{
		Projectile.timeLeft = 10; // Prevent client despawn

		// Detach + die if the master is gone — the beam should never outlive its caster.
		int who = (int)Projectile.ai[0];
		if (who < 0 || who >= Main.maxNPCs || !Main.npc[who].active)
		{
			Projectile.Kill();
			return;
		}

		NPC master = Main.npc[who];
		Projectile.Center = master.Center;            // stay welded to the trio
		Projectile.rotation = BeamDir.ToRotation();  // render rotation = beam direction

		Lighting.AddLight(Projectile.Center, 0.9f, 0.3f, 0.5f);

		// Muzzle sparks at the origin, and a sheet of dust running down the beam while firing.
		if (Firing)
		{
			Vector2 dir = BeamDir;
			for (int i = 0; i < 3; i++)
			{
				Dust muzzle = Dust.NewDustPerfect(Projectile.Center, DustID.Electric, dir.RotatedByRandom(0.5) * Main.rand.NextFloat(2f, 10f), 0, default, 1.2f);
				muzzle.noGravity = true;
			}
			for (int i = 0; i < 4; i++) // Spawn intense energy dust running along the beam
			{
				float along = Main.rand.NextFloat(60f, Length);
				Vector2 pos = Projectile.Center + dir * along + dir.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-15f, 15f);
				Dust beam = Dust.NewDustPerfect(pos, DustID.MagicMirror, dir * Main.rand.NextFloat(4f, 12f), 0, default, 1.5f);
				beam.noGravity = true;
			}
		}
	}

	// Line collision: only deal damage in the firing state, along the beam from the origin.
	public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
	{
		if (!Firing)
			return false;

		float collisionPoint = 0f;
		Vector2 start = Projectile.Center;
		Vector2 end = start + BeamDir * Length;
		return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, CollisionWidth * Projectile.scale, ref collisionPoint);
	}

	public override bool ShouldUpdatePosition() => false; // master writes Center directly

	public override bool PreDraw(ref Color lightColor)
	{
		Vector2 dir = BeamDir;
		float length = Length;
		float intensity = Firing ? 1f : 0.45f; // telegraph is a faint preview; firing is blinding
		float angle = dir.ToRotation();

		Texture2D glowTex = ModContent.Request<Texture2D>("Terraria/Images/Extra_98").Value;
		Vector2 glowOrigin = glowTex.Size() / 2f;
		
		// The holy grail of laser textures: We take a 1-pixel wide vertical slice from the center of a soft glow orb.
		// When stretched horizontally, this creates a perfectly smooth, anti-aliased solid beam with soft vertical edges!
		Rectangle softBeamSrc = new Rectangle(glowTex.Width / 2, 0, 1, glowTex.Height);
		Vector2 softBeamOrigin = new Vector2(0f, glowTex.Height / 2f);

		// Dynamic pulsing driven by time to make the beam rapidly vibrate with energy
		float time = Main.GlobalTimeWrappedHourly;
		float bluePulse = Firing ? 1f + (float)Math.Sin(time * 30f) * 0.12f : 1f;
		float redPulse = Firing ? 1f + (float)Math.Sin(time * 35f + 1f) * 0.12f : 1f;
		float greenPulse = Firing ? 1f + (float)Math.Sin(time * 38f + 2f) * 0.12f : 1f;
		float corePulse = Firing ? 1f + (float)Math.Sin(time * 45f + 3f) * 0.12f : 1f;

		float baseThickness = Firing ? 12f : 6f;

		// Colors (scale by intensity)
		Color blueColor = new Color(30, 110, 255) * intensity;
		Color redColor = new Color(255, 30, 30) * intensity;
		Color greenColor = new Color(30, 255, 30) * intensity;
		Color coreColor = Color.White * intensity;

		Vector2 start = Projectile.Center - Main.screenPosition;

		// Switch to Additive blending
		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

		// --- DRAW SOLID CONTINUOUS PLASMA BEAM ---
		// We stack them exactly on top of each other (no offset) with decreasing widths and increasing opacity.
		// A massive, faint aura blends smoothly into a tight, searing core.
		
		float solidBlueThick = baseThickness * 9.0f * bluePulse;
		Main.EntitySpriteDraw(glowTex, start, softBeamSrc, blueColor * 0.3f, angle, softBeamOrigin, new Vector2(length, solidBlueThick / glowTex.Height), SpriteEffects.None, 0);

		float solidRedThick = baseThickness * 5.0f * redPulse;
		Main.EntitySpriteDraw(glowTex, start, softBeamSrc, redColor * 0.5f, angle, softBeamOrigin, new Vector2(length, solidRedThick / glowTex.Height), SpriteEffects.None, 0);

		float solidGreenThick = baseThickness * 2.5f * greenPulse;
		Main.EntitySpriteDraw(glowTex, start, softBeamSrc, greenColor * 0.7f, angle, softBeamOrigin, new Vector2(length, solidGreenThick / glowTex.Height), SpriteEffects.None, 0);

		float solidCoreThick = baseThickness * 1.0f * corePulse;
		Main.EntitySpriteDraw(glowTex, start, softBeamSrc, coreColor * 1.0f, angle, softBeamOrigin, new Vector2(length, solidCoreThick / glowTex.Height), SpriteEffects.None, 0);

		// Draw Muzzle & Tip Glows to cap the beam softly
		if (Firing)
		{
			Vector2 tipPos = start + dir * length;
			
			// Muzzle (start)
			Main.EntitySpriteDraw(glowTex, start, null, blueColor * 0.9f, 0f, glowOrigin, baseThickness * 5f / glowTex.Width, SpriteEffects.None, 0);
			Main.EntitySpriteDraw(glowTex, start, null, coreColor, 0f, glowOrigin, baseThickness * 2.5f / glowTex.Width, SpriteEffects.None, 0);

			// Tip (end)
			Main.EntitySpriteDraw(glowTex, tipPos, null, blueColor * 0.9f, 0f, glowOrigin, baseThickness * 5f / glowTex.Width, SpriteEffects.None, 0);
			Main.EntitySpriteDraw(glowTex, tipPos, null, coreColor, 0f, glowOrigin, baseThickness * 2.5f / glowTex.Width, SpriteEffects.None, 0);
		}

		// Restore default alpha blending
		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

		return false;
	}
}
