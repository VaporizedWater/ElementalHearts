using System;
using ElementalHearts.Content.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Content.NPCs.Bosses.Animate;

/// <summary>
/// One of the two Enforcers that flank the Rare Animate master ("Blue"): the Red and Green
/// hearts. Both are invulnerable puppets — only Blue holds the shared life pool — driven by a
/// small command protocol the master writes into <see cref="NPC.ai"/>. The two are mechanically
/// identical; the <see cref="Variant"/> byte (synced via ExtraAI) only changes colour and the
/// pitch of their telegraph audio so the player can tell which is winding up by sound alone.
/// <para/>
/// Their dashes and the Finale frenzy leave a true primitive light-ribbon drawn with a
/// <see cref="VertexStrip"/> and the <c>MagicMissile</c> shader.
/// </summary>
public sealed class RareAnimateEnforcer : ModNPC
{
	public override string Texture => "ElementalHearts/Content/Items/BossSpawns/RareMenacingHeart";

	public const int VariantRed = 0;
	public const int VariantGreen = 1;

	// --- Command protocol (written by the master into NPC.ai) ---
	public const int CmdSlaved = 0;          // master writes NPC.Center directly; do nothing
	public const int CmdIdleAir = 1;         // smoothly track toward (TargetX, TargetY)
	public const int CmdTelegraphDash = 2;   // self-aim at player, lock 30 ticks out, then dash inward
	public const int CmdDashImmediate = 3;   // (TargetX,TargetY) is peak velocity; decay → 0 over Duration
	public const int CmdFrenzy = 4;          // autonomous escalating chain-dash (Finale)
	public const int CmdDespawn = 5;         // poof and remove

	private ref float Cmd => ref NPC.ai[0];
	private ref float TargetX => ref NPC.ai[1];
	private ref float TargetY => ref NPC.ai[2];
	// ai[3] holds the variant — it's auto-synced with the rest of NPC.ai[], and Rare's enforcers
	// never need a per-command duration (all telegraph/dash timings are constants), so the slot is free.

	private float Sub0 { get => NPC.localAI[0]; set => NPC.localAI[0] = value; } // sub-timer
	private float Sub1 { get => NPC.localAI[1]; set => NPC.localAI[1] = value; } // sub-phase
	private float Sub2 { get => NPC.localAI[2]; set => NPC.localAI[2] = value; } // stashed dir/aim X
	private float Sub3 { get => NPC.localAI[3]; set => NPC.localAI[3] = value; } // stashed dir/aim Y

	public int Variant => (int)NPC.ai[3];
	private int _lastCmd = -1;

	// Dash tuning — peak velocity, then linear decay to a coasting stop.
	private const float DashPeak = 28f;
	private const float DashDuration = 42f;
	private const float TelegraphTicks = 50f; // 20 ticks tracking + 30 ticks locked, per the spec
	private const float LockTicks = 30f;
	private const float BaseScale = 1.7f;

	// Primitive trail
	private const int TrailLength = 18;
	private readonly Vector2[] _trailPos = new Vector2[TrailLength];
	private bool _trailInit;
	private float _trailStrength;
	private readonly VertexStrip _strip = new();

	// Scale pulse on launch
	private float _scalePulse = 1f;
	private void PulseScale(float amt) { if (amt > _scalePulse) _scalePulse = amt; }

	public Color Tint => Variant == VariantRed ? new Color(255, 60, 40) : new Color(40, 230, 100);
	private int TorchDust => Variant == VariantRed ? DustID.RedTorch : DustID.GreenTorch;
	private float TelegraphPitch => Variant == VariantRed ? -0.45f : 0.45f;

	/// <summary>True while running a self-directed action — the master must not slave its position.</summary>
	public bool ActionInProgress =>
		Cmd == CmdTelegraphDash || Cmd == CmdDashImmediate || Cmd == CmdFrenzy;

	// === Command setters (master-facing) ===
	public void SetVariant(int variant) { NPC.ai[3] = variant; NPC.netUpdate = true; }

	public void Cmd_Slaved() { if (Cmd != CmdSlaved) Cmd = CmdSlaved; }

	public void Cmd_IdleAir(Vector2 target)
	{
		Cmd = CmdIdleAir;
		TargetX = target.X;
		TargetY = target.Y;
	}

	public void Cmd_TelegraphDash() => Cmd = CmdTelegraphDash;

	public void Cmd_DashImmediate(Vector2 peakVelocity)
	{
		Cmd = CmdDashImmediate;
		TargetX = peakVelocity.X;
		TargetY = peakVelocity.Y;
	}

	public void Cmd_Frenzy() { if (Cmd != CmdFrenzy) Cmd = CmdFrenzy; }
	public void Cmd_Despawn() => Cmd = CmdDespawn;

	private ReLogic.Utilities.SlotId _ambientSoundSlot;

	public override bool PreAI()
	{
		if (!SoundEngine.TryGetActiveSound(_ambientSoundSlot, out var activeSound))
		{
			_ambientSoundSlot = SoundEngine.PlaySound(AnimateBossSounds.RareEmission, NPC.Center, sound =>
			{
				if (!NPC.active) return false;
				sound.Position = NPC.Center;
				return true;
			});
		}
		return true;
	}

	public override void SetStaticDefaults()
	{
		Main.npcFrameCount[NPC.type] = 1;
		NPCID.Sets.CantTakeLunchMoney[NPC.type] = true;
		NPCID.Sets.NPCBestiaryDrawOffset[NPC.type] = new NPCID.Sets.NPCBestiaryDrawModifiers { Hide = true };
		NPCID.Sets.TrailCacheLength[NPC.type] = TrailLength;
	}

	public override void SetDefaults()
	{
		NPC.width = 16;
		NPC.height = 16;
		NPC.damage = 50;
		NPC.defense = 0;
		NPC.lifeMax = 1;
		NPC.HitSound = SoundID.NPCHit1;
		NPC.knockBackResist = 0f;
		NPC.aiStyle = -1;
		NPC.npcSlots = 0f;
		NPC.value = 0;
		NPC.dontTakeDamage = true;
		NPC.dontCountMe = true;
		NPC.immortal = true;
		NPC.noGravity = true;
		NPC.noTileCollide = true;
		NPC.scale = BaseScale;
		NPC.alpha = 0;
	}

	// Invulnerable adds — only the master is damageable.
	public override bool CheckActive() => false;
	public override bool? CanBeHitByItem(Player player, Item item) => false;
	public override bool? CanBeHitByProjectile(Projectile projectile) => false;

	// Red and Green sit on different immunity slots so both can connect on the same tick if the
	// player threads between them, and neither hides behind the other's i-frames.
	public override bool CanHitPlayer(Player target, ref int cooldownSlot)
	{
		Rectangle customHitbox = NPC.Hitbox;
		customHitbox.Inflate(-customHitbox.Width / 4, -customHitbox.Height / 4);
		if (!customHitbox.Intersects(target.Hitbox)) return false;

		// Only have collision when actively dashing (Phase 1 / Finale dashes)
		if (Cmd == CmdSlaved || Cmd == CmdIdleAir) return false;
		if (Cmd == CmdTelegraphDash && Sub1 == 0f) return false;
		if (Cmd == CmdFrenzy && Sub1 != 1f) return false;

		cooldownSlot = Variant == VariantRed ? ImmunityCooldownID.TileContactDamage : ImmunityCooldownID.Bosses;
		return true;
	}

	private void ResetTrail()
	{
		for (int i = 0; i < TrailLength; i++) _trailPos[i] = NPC.Center;
		_trailInit = true;
	}

	public void PoofVisuals()
	{
		SoundEngine.PlaySound(SoundID.Item8 with { Pitch = TelegraphPitch }, NPC.Center);
		for (int i = 0; i < 28; i++)
			Dust.NewDustPerfect(NPC.Center, TorchDust, Main.rand.NextVector2CircularEdge(5f, 5f), 0, default, 1.5f).noGravity = true;
	}

	public override void AI()
	{
		if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
			NPC.TargetClosest();
		Player player = Main.player[NPC.target];
		if (player.dead)
		{
			NPC.EncourageDespawn(10);
			return;
		}

		Lighting.AddLight(NPC.Center, Tint.R / 320f, Tint.G / 320f, Tint.B / 320f);

		int cmd = (int)Cmd;
		if (cmd != _lastCmd)
		{
			Sub0 = Sub1 = Sub2 = Sub3 = 0f;
			_lastCmd = cmd;
		}

		switch (cmd)
		{
			case CmdSlaved: DoSlaved(); break;
			case CmdIdleAir: DoIdleAir(); break;
			case CmdTelegraphDash: DoTelegraphDash(player); break;
			case CmdDashImmediate: DoDashImmediate(); break;
			case CmdFrenzy: DoFrenzy(player); break;
			case CmdDespawn: DoDespawn(); break;
		}

		if (NPC.velocity.Length() > 34f)
			NPC.velocity = Vector2.Normalize(NPC.velocity) * 34f;

		// Polish + trail bookkeeping
		if (_scalePulse > 1f) _scalePulse = MathHelper.Lerp(_scalePulse, 1f, 0.12f);
		float variantScale = Variant == VariantGreen ? 1.1f : 1f;
		NPC.scale = BaseScale * variantScale * _scalePulse;
		_trailStrength = MathHelper.Clamp(_trailStrength - 0.05f, 0f, 1f);

		if (!_trailInit)
		{
			ResetTrail();
		}
		else
		{
			for (int i = TrailLength - 1; i > 0; i--) _trailPos[i] = _trailPos[i - 1];
			_trailPos[0] = NPC.Center;
		}
	}

	// === SLAVED: master owns the position; we only emit ambient dust. ===
	private void DoSlaved()
	{
		NPC.velocity = Vector2.Zero;
		NPC.alpha = 0;
		NPC.rotation += 0.05f;
		if (Main.rand.NextBool(20))
			Dust.NewDust(NPC.position, NPC.width, NPC.height, TorchDust);
	}

	// === IDLE AIR: smoothly track a point the master keeps updating (orbit follow). ===
	private void DoIdleAir()
	{
		Vector2 target = new(TargetX, TargetY);
		Vector2 delta = target - NPC.Center;
		if (delta.Length() > 1400f)
		{
			NPC.Center = target;
			NPC.velocity = Vector2.Zero;
			ResetTrail();
		}
		else
		{
			Vector2 desired = delta * 0.16f;
			NPC.velocity = Vector2.Lerp(NPC.velocity, desired, 0.3f);
		}
		NPC.rotation += 0.05f;
		if (Main.rand.NextBool(22))
			Dust.NewDust(NPC.position, NPC.width, NPC.height, TorchDust);
	}

	// === TELEGRAPH DASH: track the player live, then lock the line 30 ticks before the dash. ===
	// Sub1: 0 = telegraph, 1 = dashing. Sub2/Sub3: locked aim point (telegraph) → unit dir (dash).
	private void DoTelegraphDash(Player player)
	{
		if (Sub1 == 0f)
		{
			NPC.velocity *= 0.85f;
			Sub0++;
			float dur = TelegraphTicks;

			if (Sub0 == 1f)
				SoundEngine.PlaySound(SoundID.Item28 with { Pitch = TelegraphPitch, PitchVariance = 0.1f }, NPC.Center);

			// Track until the lock window, then freeze the aim — the locked line is the contract:
			// step out of it and the dash misses.
			if (Sub0 <= dur - LockTicks)
			{
				Sub2 = player.Center.X;
				Sub3 = player.Center.Y;
			}

			if (Main.rand.NextBool(2))
			{
				Vector2 spawn = NPC.Center + Main.rand.NextVector2CircularEdge(36f, 36f);
				Dust d = Dust.NewDustPerfect(spawn, TorchDust, (NPC.Center - spawn) * 0.08f, 0, default, 1.1f);
				d.noGravity = true;
			}

			if (Sub0 >= dur)
			{
				Vector2 dir = Vector2.Normalize(new Vector2(Sub2, Sub3) - NPC.Center);
				if (dir == Vector2.Zero) dir = Vector2.UnitX;
				Sub2 = dir.X;
				Sub3 = dir.Y;
				NPC.velocity = dir * DashPeak;
				Sub1 = 1f;
				Sub0 = 0f;
				_trailStrength = 1f;
				PulseScale(1.3f);
				PoofVisuals();
				SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
				SoundEngine.PlaySound(SoundID.Roar with { Pitch = TelegraphPitch, PitchVariance = 0.1f }, NPC.Center);
				AnimateFx.ShakeCamera(NPC.Center, 3f, 900f, 8, "RareEnforcerDash");
			}
		}
		else
		{
			Sub0++;
			float ratio = MathHelper.Clamp(1f - Sub0 / DashDuration, 0f, 1f);
			NPC.velocity = new Vector2(Sub2, Sub3) * DashPeak * ratio;
			NPC.rotation += Math.Sign(Sub2 != 0f ? Sub2 : 1f) * 0.35f * ratio;
			_trailStrength = 1f;
			if (Main.rand.NextBool())
			{
				Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, TorchDust);
				d.velocity = NPC.velocity * -0.5f;
			}
			if (Sub0 > DashDuration)
			{
				Cmd = CmdSlaved;
				_lastCmd = (int)Cmd;
			}
		}
	}

	// === DASH IMMEDIATE: master supplies the peak velocity vector; decay to 0 over Duration. ===
	private void DoDashImmediate()
	{
		float dur = DashDuration;
		if (Sub0 == 0f)
		{
			NPC.velocity = new Vector2(TargetX, TargetY);
			_trailStrength = 1f;
			PulseScale(1.3f);
			PoofVisuals();
			SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
		}
		Sub0++;
		float ratio = MathHelper.Clamp(1f - Sub0 / dur, 0f, 1f);
		NPC.velocity = new Vector2(TargetX, TargetY) * ratio;
		NPC.rotation += Math.Sign(TargetX != 0f ? TargetX : 1f) * 0.35f * ratio;
		_trailStrength = 1f;
		if (Main.rand.NextBool())
		{
			Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, TorchDust);
			d.velocity = NPC.velocity * -0.5f;
		}
		if (Sub0 > dur)
		{
			Cmd = CmdSlaved;
			_lastCmd = (int)Cmd;
		}
	}

	// === FRENZY: the Finale. Independent, escalating chain-dashes with a very short read. ===
	// Sub1: 0 = brief telegraph, 1 = dash, 2 = recover. Sub2/Sub3: dash dir.
	private void DoFrenzy(Player player)
	{
		float telegraph;
		float dash;
		float recover;
		float speed;

		if (Variant == VariantGreen)
		{
			// Green: Lower frequency (long telegraph), higher distance, lower speed
			telegraph = 50f;
			dash = 42f;
			recover = 35f;
			speed = 22f;
		}
		else // VariantRed
		{
			// Red: Higher frequency (short telegraph), longer distance, higher speed
			telegraph = 30f;
			dash = 35f;
			recover = 25f;
			speed = 31f;
		}

		if (Sub1 == 0f) // telegraph (short, but still pitched + lined so it's barely fair)
		{
			NPC.velocity = Vector2.Zero;
			Sub0++;
			if (Sub0 == 1f)
				SoundEngine.PlaySound(SoundID.Item28 with { Pitch = TelegraphPitch, PitchVariance = 0.2f }, NPC.Center);
			Sub2 = player.Center.X; // frenzy tracks to the very end — chaos, not fairness
			Sub3 = player.Center.Y;
			if (Main.rand.NextBool(2))
				Dust.NewDustPerfect(NPC.Center, TorchDust, Main.rand.NextVector2Circular(2f, 2f), 0, default, 1.1f).noGravity = true;
			if (Sub0 >= telegraph)
			{
				Vector2 dir = Vector2.Normalize(new Vector2(Sub2, Sub3) - NPC.Center);
				if (dir == Vector2.Zero) dir = Vector2.UnitX;
				Sub2 = dir.X; Sub3 = dir.Y;
				NPC.velocity = dir * speed;
				Sub1 = 1f; Sub0 = 0f;
				_trailStrength = 1f;
				PulseScale(1.35f);
				PoofVisuals();
				SoundEngine.PlaySound(SoundID.Roar with { Pitch = TelegraphPitch, PitchVariance = 0.2f }, NPC.Center);
			}
		}
		else if (Sub1 == 1f) // dash
		{
			Sub0++;
			float ratio = MathHelper.Clamp(1f - Sub0 / dash, 0.15f, 1f);
			NPC.velocity = new Vector2(Sub2, Sub3) * speed * ratio;
			NPC.rotation += Math.Sign(Sub2 != 0f ? Sub2 : 1f) * 0.4f;
			_trailStrength = 1f;
			if (Main.rand.NextBool())
			{
				Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, TorchDust);
				d.velocity = NPC.velocity * -0.5f;
			}
			if (Sub0 > dash) { Sub1 = 2f; Sub0 = 0f; }
		}
		else // recover, then immediately wind up again
		{
			Sub0++;
			NPC.velocity = Vector2.Zero;
			if (Sub0 > recover) { Sub1 = 0f; Sub0 = 0f; }
		}
	}

	private void DoDespawn()
	{
		PoofVisuals();
		NPC.active = false;
	}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		// 1) Primitive light-ribbon (only meaningful at dash/frenzy speeds — _trailStrength gates it).
		DrawPrimitiveTrail();

		// 2) Telegraph line while winding up a dash. Locks 30 ticks before the dash (Sub0 > dur - 30).
		if (Cmd == CmdTelegraphDash && Sub1 == 0f)
		{
			float dur = TelegraphTicks;
			float progress = MathHelper.Clamp(Sub0 / (dur - LockTicks), 0f, 1f);
			AnimateFx.DrawLaserBeam(spriteBatch, screenPos, NPC.Center, new Vector2(Sub2, Sub3), Tint, progress, 3f);
		}

		// 3) Body — the actual boss sprite for this variant, drawn in its TRUE colours and left
		//    completely unmodified: Red shows Common Animate's heart, Green shows Uncommon Animate.
		//    Only the dash trail / telegraph above are layered on; the sprite itself is untouched.
		Texture2D texture = BodyTexture;
		Vector2 origin = texture.Size() / 2f;
		Color body = Color.White * (1f - NPC.alpha / 255f);
		spriteBatch.Draw(texture, NPC.Center - screenPos, null, body, NPC.rotation, origin, NPC.scale, SpriteEffects.None, 0f);

		return false;
	}

	// Red = Common Animate's sprite, Green = Uncommon Animate's sprite (their natural art).
	private Texture2D BodyTexture => ModContent.Request<Texture2D>(Variant == VariantRed
		? "ElementalHearts/Content/Items/BossSpawns/CommonMenacingHeart"
		: "ElementalHearts/Content/NPCs/Bosses/Animate/UncommonAnimate").Value;

	// === True VertexStrip primitive trail via the MagicMissile shader (Last Prism-style). ===
	private void DrawPrimitiveTrail()
	{
		if (_trailStrength <= 0.02f || !_trailInit)
			return;

		float[] rotations = new float[TrailLength];
		for (int i = 0; i < TrailLength; i++)
		{
			Vector2 ahead = i == 0 ? NPC.Center : _trailPos[i - 1];
			Vector2 delta = ahead - _trailPos[i];
			rotations[i] = delta == Vector2.Zero ? NPC.rotation : delta.ToRotation();
		}

		MiscShaderData shader = GameShaders.Misc["MagicMissile"];
		shader.UseSaturation(-2.5f);
		shader.UseOpacity(_trailStrength * 0.7f);
		shader.Apply(null);

		_strip.PrepareStrip(_trailPos, rotations, StripColor, StripWidth, -Main.screenPosition, TrailLength, true);
		_strip.DrawTrail();

		Main.pixelShader.CurrentTechnique.Passes[0].Apply();
	}

	private Color StripColor(float progress)
	{
		// Real alpha (not 0) so the ribbon actually renders; fades toward the tail and as the
		// dash ends (_trailStrength decays).
		Color c = Tint;
		c.A = (byte)(220 * (1f - progress));
		return c * _trailStrength;
	}

	private float StripWidth(float progress)
	{
		return MathHelper.Lerp(18f, 2f, progress) * NPC.scale * 0.5f;
	}
}
