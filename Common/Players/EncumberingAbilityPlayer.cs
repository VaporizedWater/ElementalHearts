using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameInput;
using Terraria.Audio;

namespace ElementalHearts.Common.Players;

public class EncumberingAbilityPlayer : ToggleAbilityPlayer
{
	public bool isGroundPounding;
	public int groundPoundDelay;
	public int airTime;
	public bool fastFalling;
	public int preventPlatformDropTimer;
	public int groundPoundDuration;

	public static readonly SoundStyle GroundPoundInitiateSound = new SoundStyle("ElementalHearts/Assets/Sounds/PlayerGroudPound_") {
		Variants = new int[] { 1, 2, 3, 4, 5 },
		Volume = 0.8f,
		PitchVariance = 0.1f
	};

	public static readonly SoundStyle GroundPoundLandSound = new SoundStyle("ElementalHearts/Assets/Sounds/PlayerGroundPoundLand") {
		Variants = new int[] { 1, 2, 3 },
		Volume = 0.8f,
		PitchVariance = 0.1f
	};

	public override void PreUpdateMovement()
	{
		if (!Enabled)
			return;

		if (preventPlatformDropTimer > 0)
		{
			preventPlatformDropTimer--;
			// Release down so players don't instantly drop through platforms upon landing
			Player.controlDown = false;
		}

		if (isGroundPounding)
		{
			// Do not allow players to fall through platforms during a ground pound
			Player.controlDown = false;
		}

		// Initiate ground pound if in mid-air for 0.5 seconds (30 frames), not mounted, and pressing down
		if (airTime >= 30 && !Player.mount.Active && !isGroundPounding && Player.controlDown)
		{
			isGroundPounding = true;
			fastFalling = false;
			groundPoundDelay = 20; // 0.33 seconds pause
			groundPoundDuration = 0;
			
			// Reset fall damage to only calculate from the height the ground pound is initiated at
			Player.fallStart = (int)(Player.position.Y / 16f);
			Player.fallStart2 = (int)(Player.position.Y / 16f);
			
			Terraria.Audio.SoundEngine.PlaySound(GroundPoundInitiateSound, Player.Center);
		}

		if (isGroundPounding)
		{

			if (groundPoundDelay > 0)
			{
				// Freeze in midair
				Player.velocity = Vector2.Zero;
				groundPoundDelay--;
				
				// Keep fall start pinned so the freeze time doesn't add to the fall distance somehow
				Player.fallStart = (int)(Player.position.Y / 16f);
				Player.fallStart2 = (int)(Player.position.Y / 16f);
				
				// Quick 360 spin
				float progress = 1f - (groundPoundDelay / 20f);
				Player.fullRotation = MathHelper.TwoPi * progress * Player.direction;
				Player.fullRotationOrigin = new Vector2(Player.width / 2f, Player.height / 2f);
			}
			else
			{
				fastFalling = true;
				// Fast fall
				Player.velocity.Y = 16.875f; 
				Player.maxFallSpeed = 22.5f;
				Player.runAcceleration *= 2.5f; // Increase horizontal control
				Player.fullRotation = 0f; // Reset rotation while falling

				// Safety reset in case they bounce or grapple
				if (Player.velocity.Y < 0 || Player.grappling[0] != -1)
				{
					isGroundPounding = false;
					fastFalling = false;
					Player.fullRotation = 0f;
					groundPoundDuration = 0;
				}
			}
		}
	}

	public override void PostUpdate()
	{
		// Update air time after collision logic so velocity.Y is accurate
		if (Player.velocity.Y == 0)
		{
			airTime = 0;
		}
		else
		{
			airTime++;
		}

		if (isGroundPounding)
		{
			// Check for landing after collision has run
			if (Player.velocity.Y == 0 && fastFalling)
			{
				isGroundPounding = false;
				fastFalling = false;
				preventPlatformDropTimer = 30; // Prevent platform dropping for 0.5s
				
				// Particle explosion
				for (int i = 0; i < 15; i++)
				{
					Dust dust = Dust.NewDustDirect(Player.BottomLeft - new Vector2(16, 16), Player.width + 32, 32, DustID.Smoke, 0f, -1f, 100, default, 1.1f);
					dust.velocity.X *= 1.2f;
					dust.velocity.Y = -Main.rand.NextFloat(0.5f, 1.5f);
				}
				
				for (int i = 0; i < 10; i++)
				{
					Dust dust = Dust.NewDustDirect(Player.BottomLeft - new Vector2(16, 16), Player.width + 32, 32, DustID.Stone, 0f, -1f, 100, default, 0.9f);
					dust.velocity.X *= 1f;
					dust.velocity.Y = -Main.rand.NextFloat(0.5f, 1.5f);
				}

				// Impact sound
				if (groundPoundDuration < 45)
				{
					Terraria.Audio.SoundEngine.PlaySound(new Terraria.Audio.SoundStyle("ElementalHearts/Assets/Sounds/PlayerGroundPoundLandClean") { PitchVariance = 0.1f }, Player.Center);
				}
				else
				{
					Terraria.Audio.SoundEngine.PlaySound(GroundPoundLandSound, Player.Center);
				}

				// Screen shake for heavy landing
				float magnitude = Math.Min(12f, 4f + groundPoundDuration * 0.2f);
				Vector2 punchDir = new Vector2(0f, 1f);
				Main.instance.CameraModifiers.Add(new Terraria.Graphics.CameraModifiers.PunchCameraModifier(
					Player.Center,
					punchDir,
					magnitude,
					2.5f, // vibrations per second (lower is smoother)
					20, // duration in frames (slightly longer to let it settle)
					1200f, // distance falloff
					"EncumberingLand"));
				
				// Clear rotation
				Player.fullRotation = 0f;
				groundPoundDuration = 0;
			}
			else
			{
				groundPoundDuration++;
			}
		}
	}

	public override void FrameEffects()
	{
		if (isGroundPounding)
		{
			// Activate Terraria's native procedural sitting animation (which bends the legs forward)
			Player.sitting.isSitting = true;
		}
	}

	public override void ModifyHurt(ref Player.HurtModifiers modifiers)
	{
		// SourceOtherIndex == 0 is Fall Damage
		if (modifiers.DamageSource.SourceOtherIndex == 0)
		{
			// If they just landed a ground pound or are actively pounding
			if (isGroundPounding || preventPlatformDropTimer > 0)
			{
				modifiers.FinalDamage *= 2f;
			}
		}
	}
}
