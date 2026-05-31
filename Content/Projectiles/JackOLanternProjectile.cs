using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Projectiles;

/// <summary>
/// The mini jack-o'-lantern lobbed out the back of every dash by the Jack-O'-Lantern Heart's
/// upgrade (see <see cref="Common.Players.JackOLanternDashPlayer"/>). It tumbles, glows like a lit
/// gourd, trails embers and bounces off terrain a couple of times before popping — pure juice. Its
/// damage is set at spawn from world progression, so it stays useful all game without scaling out of
/// control. Reuses the vanilla Jack 'O Lantern sprite, so no bespoke texture is needed.
/// </summary>
public sealed class JackOLanternProjectile : ModProjectile
{
	public override string Texture => "Terraria/Images/Item_" + ItemID.JackOLantern;

	private const float Gravity = 0.28f;
	private const float MaxFallSpeed = 13f;
	private const int MaxBounces = 3;            // poofs out of existence after this many
	private const float HomingRange = 420f;      // no homing at all when no monster is this close
	private const float CloseHomingRange = 170f; // inside this it homes hard and fights gravity
	private const float HomingStrength = 0.5f;   // ~50% gentle homing in the band between the two ranges
	private const int BlastTiles = 8;            // explosion size in tiles (8x8)

	public override void SetDefaults()
	{
		Projectile.width = 20;
		Projectile.height = 20;
		Projectile.friendly = true;
		Projectile.hostile = false;
		Projectile.DamageType = DamageClass.Generic;
		Projectile.penetrate = 3;
		Projectile.timeLeft = 150;
		Projectile.scale = 0.6f;                 // "mini" lantern
		Projectile.tileCollide = true;
		Projectile.ignoreWater = false;
		// Lets it tag several enemies on a bounce-through without machine-gunning a single one.
		Projectile.usesLocalNPCImmunity = true;
		Projectile.localNPCHitCooldown = 12;
	}

	public override void AI()
	{
		// While the blast hitbox is live (see Explode) the projectile is frozen for its last few
		// frames — skip movement/homing entirely so it detonates exactly where it struck.
		if (Projectile.ai[1] != 0f)
			return;

		// Custom gravity arc + a touch of air drag on the horizontal throw.
		Projectile.velocity.Y += Gravity;
		if (Projectile.velocity.Y > MaxFallSpeed)
			Projectile.velocity.Y = MaxFallSpeed;
		Projectile.velocity.X *= 0.99f;

		// Homing in two bands. With no monster within HomingRange it just arcs (won't chase the whole
		// screen). In the outer band it curves gently under gravity (~50%). Once a monster is
		// CloseHomingRange-close it turns aggressive — cancels gravity and steers hard — so it can
		// climb to flying enemies instead of sagging away beneath them.
		NPC target = FindTarget(HomingRange);
		if (target != null)
		{
			Vector2 toTarget = Projectile.DirectionTo(target.Center);
			if (Vector2.Distance(Projectile.Center, target.Center) <= CloseHomingRange)
			{
				Projectile.velocity.Y -= Gravity; // undo this frame's pull so homing beats gravity
				float speed = MathHelper.Max(Projectile.velocity.Length(), 9f);
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * speed, 0.2f);
			}
			else
			{
				Projectile.velocity = Vector2.Lerp(Projectile.velocity,
					toTarget * Projectile.velocity.Length(), 0.09f * HomingStrength);
			}
		}

		// Spin rapidly, rolling in the current direction of travel.
		Projectile.rotation += 0.45f * (Projectile.velocity.X >= 0f ? 1f : -1f);

		// Carved-pumpkin firelight, gently flickering.
		float flicker = 0.85f + 0.15f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 30f + Projectile.identity);
		Lighting.AddLight(Projectile.Center, new Vector3(0.9f, 0.42f, 0.12f) * flicker);

		// Ember trail.
		if (Main.rand.NextBool(2))
		{
			Dust ember = Dust.NewDustPerfect(Projectile.Center, DustID.Torch,
				-Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f), 100, default, 1.0f);
			ember.noGravity = true;
			ember.fadeIn = 0.4f;
		}
	}

	public override bool OnTileCollide(Vector2 oldVelocity)
	{
		// Hard cap on bounces: the collision past the limit poofs it instead of bouncing.
		if (++Projectile.ai[0] > MaxBounces)
		{
			Projectile.Kill();
			return false;
		}

		// Bounce with energy loss — 50% bouncier than a dead-stop skitter (0.75 / 0.6 retained).
		if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
			Projectile.velocity.X = -oldVelocity.X * 0.75f;
		if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
			Projectile.velocity.Y = -oldVelocity.Y * 0.6f;

		SoundEngine.PlaySound(SoundID.Dig.WithVolumeScale(0.35f).WithPitchOffset(-0.3f), Projectile.position);
		return false;
	}

	/// <summary>Nearest chaseable enemy within <paramref name="maxRange"/> pixels, or null.</summary>
	private NPC FindTarget(float maxRange)
	{
		NPC closest = null;
		float closestSq = maxRange * maxRange;
		for (int i = 0; i < Main.maxNPCs; i++)
		{
			NPC npc = Main.npc[i];
			if (!npc.CanBeChasedBy(Projectile))
				continue;

			float distSq = Vector2.DistanceSquared(npc.Center, Projectile.Center);
			if (distSq < closestSq)
			{
				closestSq = distSq;
				closest = npc;
			}
		}
		return closest;
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		// Detonate on the first enemy it touches.
		Explode();
	}

	/// <summary>
	/// Turns the lantern into an 8x8-tile blast: it balloons its (invisible) hitbox to that size,
	/// stops, and lives a few more frames with no per-target cooldown so the engine's own damage pass
	/// hits everything caught in the radius (and stays MP-correct). The fiery visual plays in
	/// <see cref="OnKill"/>. Idempotent — extra enemies the blast clips just re-enter here harmlessly.
	/// </summary>
	private void Explode()
	{
		if (Projectile.ai[1] != 0f)
			return;
		Projectile.ai[1] = 1f;

		const int blast = BlastTiles * 16;
		Projectile.position = Projectile.Center - new Vector2(blast / 2f);
		Projectile.width = Projectile.height = blast;
		Projectile.velocity = Vector2.Zero;
		Projectile.penetrate = -1;
		Projectile.maxPenetrate = -1;
		Projectile.usesLocalNPCImmunity = true;
		Projectile.localNPCHitCooldown = -1; // each enemy in the blast is struck once
		Projectile.tileCollide = false;
		Projectile.knockBack = 8f;
		if (Projectile.timeLeft > 3)
			Projectile.timeLeft = 3;
	}

	public override void OnKill(int timeLeft)
	{
		if (Projectile.ai[1] != 0f)
		{
			// Full 8x8 blast: boom, fireball, smoke, gourd shrapnel and a bright flash.
			SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

			for (int i = 0; i < 40; i++)
			{
				Dust fire = Dust.NewDustPerfect(Projectile.Center, DustID.Torch,
					Main.rand.NextVector2Circular(8f, 8f), 60, default, 1.9f);
				fire.noGravity = true;
			}
			for (int i = 0; i < 16; i++)
			{
				Dust.NewDustPerfect(Projectile.Center, DustID.Smoke,
					Main.rand.NextVector2Circular(4.5f, 4.5f), 120, default, 1.7f);
			}
			for (int i = 0; i < 10; i++)
			{
				Dust bit = Dust.NewDustPerfect(Projectile.Center, DustID.Pumpkin,
					Main.rand.NextVector2Circular(6f, 6f), 0, default, 1.3f);
				bit.velocity.Y -= 1.5f;
			}
			Lighting.AddLight(Projectile.Center, new Vector3(1.1f, 0.55f, 0.18f) * 1.6f);
			return;
		}

		// Bounced out / timed out without striking anything: the small ember poof.
		SoundEngine.PlaySound(SoundID.Item10.WithVolumeScale(0.45f).WithPitchOffset(-0.2f), Projectile.position);

		// Pop of embers + a few heavier gourd bits.
		for (int i = 0; i < 14; i++)
		{
			Dust ember = Dust.NewDustPerfect(Projectile.Center, DustID.Torch,
				Main.rand.NextVector2Circular(3.5f, 3.5f), 80, default, 1.3f);
			ember.noGravity = true;
		}
		for (int i = 0; i < 5; i++)
		{
			Dust bit = Dust.NewDustPerfect(Projectile.Center, DustID.Pumpkin,
				Main.rand.NextVector2Circular(2.5f, 2.5f), 0, default, 1.1f);
			bit.velocity.Y -= 1f;
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		// The blast hitbox is huge and invisible; don't draw the little lantern over it.
		if (Projectile.ai[1] != 0f)
			return false;

		Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
		Vector2 origin = texture.Size() / 2f;
		Vector2 drawPos = Projectile.Center - Main.screenPosition;

		// Soft additive bloom so the lit gourd glows against dark caves.
		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState,
			DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
		Color glow = new Color(255, 150, 40, 0) * 0.55f;
		for (int i = 0; i < 4; i++)
		{
			Vector2 offset = (MathHelper.TwoPi * i / 4f).ToRotationVector2() * 2f;
			Main.EntitySpriteDraw(texture, drawPos + offset, null, glow, Projectile.rotation, origin,
				Projectile.scale * 1.05f, SpriteEffects.None, 0);
		}
		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
			DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

		// The lantern itself, full-bright so the carved face always reads.
		Main.EntitySpriteDraw(texture, drawPos, null, Color.White, Projectile.rotation, origin,
			Projectile.scale, SpriteEffects.None, 0);
		return false;
	}
}
