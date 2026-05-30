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
/// Stationary area-denial heart dropped by the Rare Animate master ("Blue").
/// It rhythmically pulses like a beating heart (a sine wave on
/// <see cref="Main.GlobalTimeWrappedHourly"/>) and, when the player blunders into it,
/// leeches life back to the master and bursts a ring of dust outward. It lingers
/// long enough (8s) to remain a hazard during the Phase 3 deathray sweep, which is
/// what turns the arena into a genuine spatial puzzle.
/// </summary>
public sealed class HeartTrapProjectile : ModProjectile
{
	public override string Texture => "ElementalHearts/Content/Items/BossSpawns/RareMenacingHeart";

	// ai[0] = whoAmI of the master NPC to heal on a successful leech (-1 = none).
	private const int Lifetime = 480;       // 8 seconds
	private const int FadeIn = 20;          // ticks to fade in after spawn
	private const int FadeOut = 40;         // ticks to fade out before death
	private const int HealPerLeech = 22;    // life returned to the master each leech

	public override void SetStaticDefaults()
	{
		ProjectileID.Sets.TrailCacheLength[Projectile.type] = 0;
	}

	public override void SetDefaults()
	{
		Projectile.width = 36;   // ~2x2 tiles
		Projectile.height = 36;
		Projectile.hostile = true;
		Projectile.friendly = false;
		Projectile.penetrate = -1;
		Projectile.timeLeft = Lifetime;
		Projectile.ignoreWater = true;
		Projectile.tileCollide = false;   // predictable, deliberate placement — no terrain snapping
		Projectile.netImportant = true;
		Projectile.scale = 1f;
	}

	public override void AI()
	{
		Projectile.velocity = Vector2.Zero;
		Lighting.AddLight(Projectile.Center, 0.2f, 0.45f, 0.95f);

		// Bubbles drifting up through the "juice" — sells the liquid look even when idle.
		if (Main.rand.NextBool(6))
		{
			Vector2 spawn = Projectile.Center + Main.rand.NextVector2Circular(15f, 17f);
			int dustType = Main.rand.NextBool(3) ? DustID.BlueTorch : DustID.IceTorch;
			Dust d = Dust.NewDustPerfect(spawn, dustType, new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -Main.rand.NextFloat(0.4f, 1.1f)), 120, default, Main.rand.NextFloat(0.7f, 1.1f));
			d.noGravity = true;
			d.fadeIn = 1.1f;
		}
	}

	public override void OnHitPlayer(Player target, Player.HurtInfo info)
	{
		// Visuals run everywhere; the actual heal is server/SP-authoritative so MP clients
		// can't double-apply it. (Boss life is server-owned and synced down.)
		LeechVisuals();

		if (Main.netMode != NetmodeID.MultiplayerClient)
		{
			int who = (int)Projectile.ai[0];
			if (who >= 0 && who < Main.maxNPCs)
			{
				NPC master = Main.npc[who];
				if (master.active && master.life > 0 && master.life < master.lifeMax)
				{
					int heal = Math.Min(HealPerLeech, master.lifeMax - master.life);
					master.life += heal;
					master.HealEffect(heal, true);
				}
			}
		}
	}

	private void LeechVisuals()
	{
		SoundEngine.PlaySound(SoundID.Item20 with { Pitch = -0.3f, PitchVariance = 0.2f }, Projectile.Center);
		// A splash of "juice" bursting outward in a ring as the trap drains the player.
		const int points = 26;
		for (int i = 0; i < points; i++)
		{
			Vector2 dir = (MathHelper.TwoPi * i / points).ToRotationVector2();
			Dust d = Dust.NewDustPerfect(Projectile.Center + dir * 8f, DustID.IceTorch, dir * 6f, 0, default, 1.6f);
			d.noGravity = true;
		}
		for (int i = 0; i < 6; i++)
		{
			Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.BlueTorch, Main.rand.NextVector2Circular(4f, 4f), 0, default, 1.4f);
			d.noGravity = true;
		}
	}

	// Smooth fade-in on spawn and fade-out before despawning so traps don't pop in/out.
	private float LifeAlpha()
	{
		int age = Lifetime - Projectile.timeLeft;
		float a = 1f;
		if (age < FadeIn) a = age / (float)FadeIn;
		if (Projectile.timeLeft < FadeOut) a = Math.Min(a, Projectile.timeLeft / (float)FadeOut);
		return MathHelper.Clamp(a, 0f, 1f);
	}

	// Palette for the "glass of juice" look.
	private static readonly Color DeepBlue = new(40, 110, 230);
	private static readonly Color MidBlue = new(80, 165, 255);
	private static readonly Color BrightCyan = new(150, 220, 255);

	// ---- Procedural heart geometry (no sprite at all) ----
	// The shape is the implicit heart f(x,y) = (x²+y²−1)³ − x²·y³ ≤ 0 (y up), scanline-filled
	// with the 1×1 MagicPixel. Computed once into static row buffers (the shape never changes —
	// only the per-frame pixel scale / colour / offset do), then reused by every trap.
	private const int Rows = 40;
	private const float YTop = 1.30f;
	private const float YBottom = -1.15f;
	private const float YMid = (YTop + YBottom) / 2f; // centres the heart on the hitbox
	private const float UnitBase = 15f;               // normalized→pixels (heart ≈ 36×40 px ≈ 2×2 tiles)

	private static bool _heartBuilt;
	private static readonly float[] _ynRow = new float[Rows];
	private static readonly int[] _segN = new int[Rows];
	private static readonly float[] _segL0 = new float[Rows];
	private static readonly float[] _segR0 = new float[Rows];
	private static readonly float[] _segL1 = new float[Rows];
	private static readonly float[] _segR1 = new float[Rows];

	private static float HeartField(float x, float y)
	{
		float a = x * x + y * y - 1f;
		return a * a * a - x * x * y * y * y;
	}

	private static void BuildHeart()
	{
		if (_heartBuilt) return;
		for (int r = 0; r < Rows; r++)
		{
			float yn = MathHelper.Lerp(YTop, YBottom, r / (float)(Rows - 1));
			_ynRow[r] = yn;

			int count = 0;
			bool inside = false;
			float segStart = 0f;
			for (float x = -1.3f; x <= 1.3f; x += 0.01f)
			{
				bool nowInside = HeartField(x, yn) <= 0f;
				if (nowInside && !inside) { segStart = x; inside = true; }
				else if (!nowInside && inside) { inside = false; StoreSeg(r, ref count, segStart, x); }
			}
			if (inside) StoreSeg(r, ref count, segStart, 1.3f);
			_segN[r] = count;
		}
		_heartBuilt = true;
	}

	private static void StoreSeg(int r, ref int count, float l, float rr)
	{
		// The heart has at most two spans per row (the top cleft splits into two lobes).
		if (count == 0) { _segL0[r] = l; _segR0[r] = rr; count = 1; }
		else if (count == 1) { _segL1[r] = l; _segR1[r] = rr; count = 2; }
	}

	// Scanline-fills the heart with a top→bottom colour gradient. `top`/`bottom` carry their own
	// alpha (translucent for the alpha-blend body; intensity for the additive passes).
	private static void DrawHeartFill(Vector2 center, float unitX, float unitY, Color top, Color bottom)
	{
		Texture2D px = TextureAssets.MagicPixel.Value;
		int thickness = Math.Max(1, (int)Math.Ceiling((YTop - YBottom) * unitY / Rows) + 1);
		for (int r = 0; r < Rows; r++)
		{
			if (_segN[r] == 0) continue;
			float t = (_ynRow[r] - YBottom) / (YTop - YBottom);
			Color c = Color.Lerp(bottom, top, t);
			int sy = (int)(center.Y - (_ynRow[r] - YMid) * unitY);
			for (int s = 0; s < _segN[r]; s++)
			{
				float l = s == 0 ? _segL0[r] : _segL1[r];
				float rr = s == 0 ? _segR0[r] : _segR1[r];
				int x0 = (int)(center.X + l * unitX);
				int x1 = (int)Math.Ceiling(center.X + rr * unitX);
				Main.spriteBatch.Draw(px, new Rectangle(x0, sy, Math.Max(1, x1 - x0), thickness), c);
			}
		}
	}

	// A small soft bright blob (the "wet glass" sheen) — also purely math (a filled circle).
	private static void DrawSheen(Vector2 center, float radius, Color color)
	{
		Texture2D px = TextureAssets.MagicPixel.Value;
		int rad = Math.Max(2, (int)radius);
		for (int dy = -rad; dy <= rad; dy++)
		{
			int half = (int)Math.Sqrt(Math.Max(0, rad * rad - dy * dy));
			if (half <= 0) continue;
			Color c = color * (1f - Math.Abs(dy) / (float)(rad + 1)); // vertical falloff for softness
			Main.spriteBatch.Draw(px, new Rectangle((int)(center.X - half), (int)(center.Y + dy), half * 2, 1), c);
		}
	}

	public override void OnKill(int timeLeft)
	{
		if (timeLeft <= 0)
		{
			SoundEngine.PlaySound(SoundID.Item54 with { Pitch = 0.4f, Volume = 0.4f }, Projectile.Center);
			for (int i = 0; i < 12; i++)
			{
				Dust.NewDustPerfect(Projectile.Center, DustID.IceTorch, Main.rand.NextVector2Circular(2.5f, 2.5f), 0, default, 1.1f).noGravity = true;
			}
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		BuildHeart();
		Vector2 pos = Projectile.Center - Main.screenPosition;
		float fade = LifeAlpha();
		float time = Main.GlobalTimeWrappedHourly;

		// Beating-heart pulse: a sharp double-thump built from two offset sine waves.
		float t = time * MathHelper.TwoPi * 1.4f;
		float beat = (float)(Math.Sin(t) * 0.6 + Math.Sin(t * 2.0) * 0.4);
		
		float sizeScale = 1f;
		if (Projectile.timeLeft < FadeOut)
			sizeScale = MathHelper.Lerp(0.75f, 1f, Projectile.timeLeft / (float)FadeOut);
			
		float bs = UnitBase * (1f + beat * 0.08f) * sizeScale;

		// Liquid jiggle: squash one axis while stretching the other (out of phase).
		float wobX = (float)Math.Sin(time * 5f);
		float wobY = (float)Math.Sin(time * 5f + 1.3f);
		float ux = bs * (1f + wobX * 0.06f);
		float uy = bs * (1f - wobY * 0.06f);

		// Slow lissajous so the inner light/juice appears to slosh around.
		Vector2 slosh = new Vector2((float)Math.Cos(time * 2.1f), (float)Math.Sin(time * 1.6f)) * 2.5f;

		// ---- Additive halo: soft blue glow so the trap pops on ANY background ----
		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

		Color haloTop = MidBlue; haloTop.A = (byte)(55 * fade);
		Color haloBot = DeepBlue; haloBot.A = (byte)(45 * fade);
		DrawHeartFill(pos, ux * 1.30f, uy * 1.30f, haloTop, haloBot);
		Color halo2 = MidBlue; halo2.A = (byte)(70 * fade);
		DrawHeartFill(pos, ux * 1.13f, uy * 1.13f, halo2, halo2);

		// ---- Translucent body (alpha-blended): the see-through "juice" heart ----
		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

		// Outer fluid — deeper blue, more transparent, slightly oversized (gives "thickness").
		Color outTop = MidBlue; outTop.A = (byte)(125 * fade);
		Color outBot = DeepBlue; outBot.A = (byte)(120 * fade);
		DrawHeartFill(pos, ux * 1.04f, uy * 1.04f, outTop, outBot);

		// Inner fluid — brighter, denser, sloshing offset so it reads as liquid in motion.
		Color inTop = BrightCyan; inTop.A = (byte)(165 * fade);
		Color inBot = MidBlue; inBot.A = (byte)(165 * fade);
		DrawHeartFill(pos + slosh, ux * 0.82f, uy * 0.82f, inTop, inBot);

		// ---- Additive top pass: inner glow + moving specular sheen ----
		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

		float glowI = (0.5f + beat * 0.25f) * fade;
		Color glowTop = BrightCyan; glowTop.A = (byte)(150 * glowI);
		Color glowBot = MidBlue; glowBot.A = (byte)(110 * glowI);
		DrawHeartFill(pos + slosh, ux * 0.66f, uy * 0.66f, glowTop, glowBot);

		// Wet-glass gloss near the top-left, drifting with the slosh.
		Color sheen = Color.White; sheen.A = (byte)(190 * fade);
		DrawSheen(pos + new Vector2(-ux * 0.32f, -uy * 0.55f) + slosh * 0.3f, bs * 0.22f, sheen);

		// Restore the default blend for the rest of the projectile layer.
		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

		return false;
	}
}
