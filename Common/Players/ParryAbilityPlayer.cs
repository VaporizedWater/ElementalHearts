using System;
using ElementalHearts.Common.Systems;
using ElementalHearts.Content.Buffs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Common.Players;

/// <summary>
/// The Deerclops Heart's active ability: a winter-beast <b>parry</b>. Tapping the Parry keybind makes
/// the player flare pink for one second; touch an enemy in that window and you body-check it for ten
/// times its contact damage while shrugging the blow off completely. A clean parry grants a short
/// grace of i-frames; whether it lands or whiffs, the move goes on a 30-second cooldown (shown as the
/// <see cref="ParryCooldown"/> debuff, the way Chaos State gates the Rod of Discord). When the cooldown
/// lapses, a burst of pink sparks tells the player they're armed again.
///
/// All the gameplay (window, contact, cooldown, FX) lives here so the heart item stays a pure
/// declaration — same split as <see cref="DiscordAbilityPlayer"/>.
/// </summary>
public class ParryAbilityPlayer : ToggleAbilityPlayer
{
	/// <summary>How long the pink "active" window lasts — a tight quarter-second of catch-frames, so
	/// landing a parry is a real timing test. Public so the net handler can seed the visual glow on
	/// remote clients with the same duration.</summary>
	public const int WindowDuration = 15;

	/// <summary>Forced i-frames handed out the instant a parry connects, so you can reposition.</summary>
	private const int SuccessIFrames = 60;

	/// <summary>The 15-second lockout between parries.</summary>
	private const int Cooldown = 15 * 60;

	/// <summary>Enemies caught in the parry window eat this multiple of their own contact damage.</summary>
	private const int DamageMultiplier = 10;

	/// <summary>Against bosses the counter is capped to <c>lifeMax / this</c> — i.e. 10% of their max
	/// health — so a single parry can never delete a boss. Regular enemies are uncapped.</summary>
	private const int BossDamageCapDivisor = 10;

	/// <summary>How hard a successful parry slings the player away from what they hit.</summary>
	private const float LaunchSpeed = 15f;

	/// <summary>Upward kick blended into the launch so the recoil leaps rather than skids.</summary>
	private const float LaunchLift = 4f;

	private static readonly Color ParryPink = new(255, 105, 180);

	/// <summary>Bright, near-white pink the player's sprite is washed toward while parrying.</summary>
	private static readonly Color GlowTint = new(255, 120, 200);

	private int _windowTimer;
	private int _cooldownTimer;

	/// <summary>
	/// Purely cosmetic countdown for the pink glow, ticked on <b>every</b> client for <b>every</b>
	/// player (unlike <see cref="_windowTimer"/>, which is owner-only gameplay). The owner seeds it on
	/// parry and broadcasts <see cref="Network.MessageType.ParryStarted"/> so remote clients light up
	/// the same flare — the glow is visible to everyone, not just the parrying player.
	/// </summary>
	public int ParryGlowTimer;

	public override void ProcessTriggers(TriggersSet triggersSet)
	{
		if (!Enabled)
			return;

		if (ParryAbilitySystem.ParryKeybind.JustPressed)
			TryStartParry();
	}

	/// <summary>Opens the parry window if we're not dead and the move is off cooldown.</summary>
	private void TryStartParry()
	{
		if (Player.dead || _cooldownTimer > 0 || _windowTimer > 0)
			return;

		_windowTimer = WindowDuration;
		_cooldownTimer = Cooldown;  // attempting the parry spends it, hit or miss

		BeginGlow();  // light up the pink flare + start puff locally…

		// …and tell everyone else to light it up too, so the glow shows on every client.
		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			ModPacket packet = Mod.GetPacket();
			packet.Write((byte)Network.MessageType.ParryStarted);
			packet.Write((byte)Player.whoAmI);
			packet.Send();
		}

		// Every parry attempt slaps — a sharp tell that the window just opened.
		SoundEngine.PlaySound(SlapSound(), Player.Center);
	}

	/// <summary>Seeds the cosmetic glow window and pops the subtle "parry on" puff. Called locally by
	/// the parrying player and, via <see cref="Network.MessageType.ParryStarted"/>, on every other
	/// client — so the start flourish shows for everyone, not just the owner.</summary>
	public void BeginGlow()
	{
		ParryGlowTimer = WindowDuration;
		SpawnStartBurst();
	}

	public override void PostUpdate()
	{
		// The pink flare is cosmetic and ticks for EVERY player on EVERY client (the owner seeds it
		// locally and via ParryStarted; remote clients seed it from that packet), so the glow is
		// visible to all — not just the parrying player.
		if (ParryGlowTimer > 0)
		{
			ParryGlowTimer--;
			EmitGlow();
		}

		// Everything below is gameplay — owner-only. NPC strikes are synced explicitly inside LandParry.
		if (Player.whoAmI != Main.myPlayer)
			return;

		if (_windowTimer > 0)
		{
			_windowTimer--;
			CheckParryContact();  // may close the window early on a successful catch
		}

		if (_cooldownTimer > 0)
		{
			_cooldownTimer--;
			RefreshCooldownBuff();

			// The exact frame the lock lifts, pop pink sparks so the player feels "ready" without
			// having to watch the buff bar.
			if (_cooldownTimer == 0)
				SpawnReadyBurst();
		}
	}

	/// <summary>While the window is open the player is untouchable — dodge everything for free.</summary>
	public override bool FreeDodge(Player.HurtInfo info) => _windowTimer > 0;

	/// <summary>
	/// Washes the whole player sprite toward a bright, pulsing pink while the glow is live — the
	/// "a pink shader got slapped on them" look. Because <see cref="ParryGlowTimer"/> is synced and
	/// this runs in every player's draw pass on every client, the flare shows up for everyone. The
	/// tint nearly overrides ambient lighting (lerping toward a near-white pink) so the player reads
	/// as genuinely glowing even in a pitch-black cave.
	/// </summary>
	public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
	{
		if (ParryGlowTimer <= 0)
			return;

		// Fade the wash in over the first/last few frames and pulse it so the glow feels alive.
		float envelope = Math.Min(1f, Math.Min(ParryGlowTimer, WindowDuration - ParryGlowTimer + 1) / 8f);
		float pulse = 0.7f + (0.18f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 22f));
		float strength = MathHelper.Clamp(pulse * envelope, 0f, 0.92f);

		Color Wash(Color c) => Color.Lerp(c, GlowTint, strength);

		drawInfo.colorArmorHead = Wash(drawInfo.colorArmorHead);
		drawInfo.colorArmorBody = Wash(drawInfo.colorArmorBody);
		drawInfo.colorArmorLegs = Wash(drawInfo.colorArmorLegs);
		drawInfo.colorHead = Wash(drawInfo.colorHead);
		drawInfo.colorBodySkin = Wash(drawInfo.colorBodySkin);
		drawInfo.colorLegs = Wash(drawInfo.colorLegs);
		drawInfo.colorEyeWhites = Wash(drawInfo.colorEyeWhites);
		drawInfo.colorEyes = Wash(drawInfo.colorEyes);
		drawInfo.colorHair = Wash(drawInfo.colorHair);
		drawInfo.colorShirt = Wash(drawInfo.colorShirt);
		drawInfo.colorUnderShirt = Wash(drawInfo.colorUnderShirt);
		drawInfo.colorPants = Wash(drawInfo.colorPants);
		drawInfo.colorShoes = Wash(drawInfo.colorShoes);
		drawInfo.colorMount = Wash(drawInfo.colorMount);
	}

	/// <summary>Pink bloom: a strong point light plus a steady drizzle of pink sparks hugging the
	/// player, so the glowing sprite is haloed by light too. Runs on every client for every glowing
	/// player.</summary>
	private void EmitGlow()
	{
		Lighting.AddLight(Player.Center, ParryPink.ToVector3() * 1.1f);

		// A couple of clingy sparks per frame for a shimmering outline around the lit-up sprite.
		for (int i = 0; i < 2; i++)
		{
			Dust spark = Dust.NewDustPerfect(
				Player.Center + Main.rand.NextVector2Circular(Player.width, Player.height),
				DustID.PinkTorch, Main.rand.NextVector2Circular(1.2f, 1.2f), 80, default, 1.25f);
			spark.noGravity = true;
		}
	}

	/// <summary>Subtle "parry on" feedback the instant the window opens — a small pink puff and a faint
	/// even ring, far lighter than the success spectacle. Just enough to confirm the input landed.</summary>
	private void SpawnStartBurst()
	{
		// A sparse, even ring that reads as a quick pulse.
		const int ringPoints = 10;
		for (int i = 0; i < ringPoints; i++)
		{
			Vector2 dir = (MathHelper.TwoPi * i / ringPoints).ToRotationVector2();
			Dust ring = Dust.NewDustPerfect(Player.Center, DustID.PinkTorch, dir * 3.2f, 120, default, 1f);
			ring.noGravity = true;
		}

		// A few loose sparks for a touch of life.
		for (int i = 0; i < 6; i++)
		{
			Vector2 velocity = Main.rand.NextVector2Circular(2.5f, 2.5f);
			Dust spark = Dust.NewDustPerfect(Player.Center, DustID.PinkTorch, velocity, 130, default, Main.rand.NextFloat(0.8f, 1.2f));
			spark.noGravity = true;
		}
	}

	/// <summary>Scans for the first hostile NPC overlapping the player and, if found, lands the parry.</summary>
	private void CheckParryContact()
	{
		Rectangle hitbox = Player.Hitbox;

		for (int i = 0; i < Main.maxNPCs; i++)
		{
			NPC npc = Main.npc[i];
			if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.damage <= 0 || npc.type == NPCID.TargetDummy)
				continue;

			if (npc.Hitbox.Intersects(hitbox))
			{
				LandParry(npc);
				return;
			}
		}
	}

	/// <summary>Counters <paramref name="npc"/> for 10× its contact damage and rewards the catch.</summary>
	private void LandParry(NPC npc)
	{
		int damage = Math.Max(1, npc.damage * DamageMultiplier);

		// Safety cap: a boss's huge contact damage could otherwise let a single parry erase a big
		// chunk of its health (some endgame bosses hit for hundreds → thousands after the 10×). Clamp
		// the counter to 10% of the boss's max life so the parry stays a strong-but-fair tool, never a
		// one-shot. Regular enemies are uncapped.
		if (npc.boss && npc.lifeMax > 0)
			damage = Math.Min(damage, npc.lifeMax / BossDamageCapDivisor);

		NPC.HitInfo hit = new()
		{
			Damage = damage,
			Knockback = 6f,
			HitDirection = Math.Sign(npc.Center.X - Player.Center.X),
		};
		if (hit.HitDirection == 0)
			hit.HitDirection = Player.direction;

		npc.StrikeNPC(hit);
		if (Main.netMode != NetmodeID.SinglePlayer)
			NetMessage.SendStrikeNPC(npc, hit);

		// Close the window and hand out the post-parry grace frames.
		_windowTimer = 0;
		Player.immune = true;
		Player.immuneNoBlink = false;
		Player.immuneTime = Math.Max(Player.immuneTime, SuccessIFrames);

		// Recoil: the parry slings the player away from whatever they just clobbered, with a touch of
		// lift so it reads as a triumphant leap back rather than a flat shove. The 0.001f nudge keeps
		// Normalize safe when perfectly overlapping.
		Vector2 awayDir = Vector2.Normalize(Player.Center - npc.Center + new Vector2(0.001f));
		Player.velocity = (awayDir * LaunchSpeed) + new Vector2(0f, -LaunchLift);

		// A landed parry adds the meaty hit on top of the slap that already played on press.
		SoundEngine.PlaySound(HitSound(), Player.Center);

		SpawnSuccessBurst(npc.Center, awayDir);

		// Tactile counter-punch toward the enemy.
		Main.instance.CameraModifiers.Add(new Terraria.Graphics.CameraModifiers.PunchCameraModifier(
			Player.Center,
			-awayDir,
			8f, 5f, 16, 1000f, "DeerclopsParry"));
	}

	/// <summary>Keeps the cooldown debuff icon's timer pinned to our real countdown.</summary>
	private void RefreshCooldownBuff()
	{
		int buffType = ModContent.BuffType<ParryCooldown>();
		int index = Player.FindBuffIndex(buffType);
		if (index == -1)
			Player.AddBuff(buffType, _cooldownTimer);
		else
			Player.buffTime[index] = _cooldownTimer;
	}

	/// <summary>
	/// The full-fat success spectacle: a clean parry throws everything at the contact point. An
	/// expanding pink shock-ring, a fat omnidirectional spark spray, a cone of debris flung the way the
	/// player launches, frosty winter-beast glints, and a few drifting smoke puffs for weight.
	/// </summary>
	private void SpawnSuccessBurst(Vector2 center, Vector2 launchDir)
	{
		// Expanding shock-ring — even spokes so it reads as a deliberate ring, not just noise.
		const int ringPoints = 36;
		for (int i = 0; i < ringPoints; i++)
		{
			Vector2 dir = (MathHelper.TwoPi * i / ringPoints).ToRotationVector2();
			Dust ring = Dust.NewDustPerfect(center, DustID.PinkTorch, dir * 9f, 60, default, 1.6f);
			ring.noGravity = true;
		}

		// Dense omnidirectional spark spray.
		for (int i = 0; i < 45; i++)
		{
			Vector2 velocity = Main.rand.NextVector2Circular(9f, 9f);
			Dust spark = Dust.NewDustPerfect(center, DustID.PinkTorch, velocity, 80, default, Main.rand.NextFloat(1.2f, 2f));
			spark.noGravity = true;
		}

		// Bright glow cores for a punchy flash at the impact.
		for (int i = 0; i < 18; i++)
		{
			Vector2 velocity = Main.rand.NextVector2Circular(6f, 6f);
			Dust glow = Dust.NewDustPerfect(center, DustID.RainbowMk2, velocity, 0, ParryPink, Main.rand.NextFloat(1.4f, 2.2f));
			glow.noGravity = true;
		}

		// Debris cone flung the direction the player recoils — sells the "knocked apart" moment.
		for (int i = 0; i < 22; i++)
		{
			Vector2 velocity = launchDir.RotatedByRandom(0.7f) * Main.rand.NextFloat(4f, 12f);
			Dust shrapnel = Dust.NewDustPerfect(center, DustID.PinkTorch, velocity, 70, default, Main.rand.NextFloat(1f, 1.6f));
			shrapnel.noGravity = true;
		}

		// Wintry glints — a nod to the Deerclops' tundra theme.
		for (int i = 0; i < 14; i++)
		{
			Vector2 velocity = Main.rand.NextVector2Circular(5f, 5f);
			Dust frost = Dust.NewDustPerfect(center, DustID.SnowflakeIce, velocity, 100, default, Main.rand.NextFloat(1f, 1.6f));
			frost.noGravity = true;
		}

		// Slow smoke puffs for body and afterglow.
		for (int i = 0; i < 10; i++)
		{
			Vector2 velocity = Main.rand.NextVector2Circular(2.5f, 2.5f);
			Dust smoke = Dust.NewDustPerfect(center, DustID.Smoke, velocity, 140, ParryPink, Main.rand.NextFloat(1.4f, 2f));
			smoke.noGravity = true;
		}

		// A brief pink flash of light at the point of contact.
		Lighting.AddLight(center, ParryPink.ToVector3() * 1.5f);
	}

	/// <summary>Softer pink puff around the player the moment the parry comes back online.</summary>
	private void SpawnReadyBurst()
	{
		for (int i = 0; i < 24; i++)
		{
			Vector2 velocity = Main.rand.NextVector2Circular(5f, 5f);
			Dust spark = Dust.NewDustPerfect(Player.Center, DustID.PinkTorch, velocity, 100, default, Main.rand.NextFloat(1f, 1.5f));
			spark.noGravity = true;
		}

		// A quiet shimmer punctuates "ready" without being intrusive.
		SoundEngine.PlaySound(SoundID.MaxMana.WithVolumeScale(0.5f).WithPitchOffset(0.3f), Player.Center);
	}

	// PlayerParrySlap1/2 and PlayerParryHit1/2 ship as paired variants; pick one at random each time
	// so repeated parries never sound stamped.
	private static SoundStyle SlapSound() =>
		new($"ElementalHearts/Assets/Sounds/PlayerParrySlap{Main.rand.Next(1, 3)}") { PitchVariance = 0.1f, Volume = 0.9f };

	private static SoundStyle HitSound() =>
		new($"ElementalHearts/Assets/Sounds/PlayerParryHit{Main.rand.Next(1, 3)}") { PitchVariance = 0.1f };
}
