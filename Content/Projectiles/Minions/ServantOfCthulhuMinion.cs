using System;
using ElementalHearts.Content.Buffs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Projectiles.Minions;

public class ServantOfCthulhuMinion : ModProjectile
{
	public override string Texture => "Terraria/Images/NPC_5";

	public override void SetStaticDefaults()
	{
		Main.projFrames[Projectile.type] = 2;
		Main.projPet[Projectile.type] = true;
		ProjectileID.Sets.MinionSacrificable[Projectile.type] = false;
		ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
	}

	public override void SetDefaults()
	{
		Projectile.width = 30;
		Projectile.height = 30;
		Projectile.tileCollide = false;
		Projectile.friendly = true;
		Projectile.minion = false; // Intentionally false so it's not a summoner class weapon
		Projectile.DamageType = DamageClass.Default;
		Projectile.minionSlots = 0f;
		Projectile.penetrate = -1;
	}

	public override bool? CanCutTiles()
	{
		return false;
	}

	public override bool MinionContactDamage()
	{
		return true;
	}

	public override void AI()
	{
		Projectile.spriteDirection = 1; // The Servant of Cthulhu sprite is symmetrical and natively points DOWN, so no flipping is needed!
		Player player = Main.player[Projectile.owner];

		if (player.dead || !player.active)
		{
			player.ClearBuff(ModContent.BuffType<ServantOfCthulhuBuff>());
		}
		if (player.HasBuff(ModContent.BuffType<ServantOfCthulhuBuff>()))
		{
			Projectile.timeLeft = 2;
		}

		// Custom animation
		Projectile.frameCounter++;
		if (Projectile.frameCounter >= 6)
		{
			Projectile.frameCounter = 0;
			Projectile.frame++;
			if (Projectile.frame >= Main.projFrames[Projectile.type])
			{
				Projectile.frame = 0;
			}
		}

		// AI State Machine
		ref float state = ref Projectile.ai[0];
		ref float timer = ref Projectile.ai[1];
		
		float targetRotation = Projectile.rotation;

		if (state == 0) // Idle / Orbit
		{
			// Find closest target
			NPC target = null;
			float closestDist = 350f; // Targeting range
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (npc.CanBeChasedBy() && Collision.CanHit(Projectile.Center, 1, 1, npc.Center, 1, 1))
				{
					float dist = Vector2.Distance(player.Center, npc.Center);
					if (dist < closestDist)
					{
						closestDist = dist;
						target = npc;
					}
				}
			}

			if (target != null)
			{
				// Orbit around player, but clamped to a leash!
				timer++;
				float orbitSpeed = 0.025f; // Slower orbit
				float orbitAngle = timer * orbitSpeed;
				
				float orbitRadius = Math.Min(closestDist, 120f); // Leash ensures he never strays too far
				Vector2 targetPos = player.Center + new Vector2((float)Math.Cos(orbitAngle), (float)Math.Sin(orbitAngle)) * orbitRadius;
				
				// Move smoothly towards orbit position
				Vector2 direction = targetPos - Projectile.Center;
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 0.08f, 0.03f); // Slower, smoother

				// Stare at the target with intent
				if (target.Center != Projectile.Center)
				{
					targetRotation = (target.Center - Projectile.Center).ToRotation() - MathHelper.PiOver2;
				}

				// Periodically wind up for a dash
				if (timer > 120) // Every 2 seconds
				{
					state = 1; // Transition to Windup
					timer = 0;
					Projectile.ai[2] = target.whoAmI; // Store target
				}
			}
			else
			{
				// No target, idle close to player
				timer = 0;
				float bobOffset = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f) * 6f; // Subtle vertical bobbing
				Vector2 idlePos = player.Center + new Vector2(-player.direction * 20f, -30f + bobOffset);
				Vector2 direction = idlePos - Projectile.Center;
				float dist = direction.Length();

				if (dist > 2000f)
				{
					Projectile.Center = player.Center; // Teleport if too far
				}
				else if (dist > 10f)
				{
					direction.Normalize();
					direction *= Math.Min(dist * 0.03f, 4f); // Very slow, floaty max speed
					Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction, 0.02f); // Extremely smooth accel/decel
				}
				else
				{
					Projectile.velocity *= 0.8f; // Slow down when close
				}

				// Gaze warmly at the player
				if (player.Center != Projectile.Center)
				{
					targetRotation = (player.Center - Projectile.Center).ToRotation() - MathHelper.PiOver2;
				}
			}
		}
		else if (state == 1) // Dash Windup
		{
			timer++;
			Projectile.velocity *= 0.85f; // Brake hard to slow down for a second

			int targetIndex = (int)Projectile.ai[2];
			if (targetIndex >= 0 && targetIndex < Main.maxNPCs && Main.npc[targetIndex].active)
			{
				// Intently stare at the target during windup
				NPC target = Main.npc[targetIndex];
				if (target.Center != Projectile.Center)
				{
					targetRotation = (target.Center - Projectile.Center).ToRotation() - MathHelper.PiOver2;
				}
			}
			else
			{
				state = 0; // Target lost
				timer = 0;
			}

			if (timer > 45) // ~0.75 seconds of windup
			{
				state = 2; // Execute Dash
				timer = 0;
			}
		}
		else if (state == 2) // Dash
		{
			timer++;
			
			if (timer == 1)
			{
				// High initial burst of speed
				int targetIndex = (int)Projectile.ai[2];
				if (targetIndex >= 0 && targetIndex < Main.maxNPCs && Main.npc[targetIndex].active)
				{
					NPC target = Main.npc[targetIndex];
					Vector2 dashDir = target.Center - Projectile.Center;
					dashDir.Normalize();
					Projectile.velocity = dashDir * 35f; // Extremely high initial velocity
				}
				else
				{
					state = 0;
					timer = 0;
				}
			}
			else
			{
				Projectile.velocity *= 0.88f; // High air drag, slowing down significantly each frame
			}

			// Add visual juice (blood dust trailing behind)
			int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, -Projectile.velocity.X * 0.5f, -Projectile.velocity.Y * 0.5f, 100, default, 1.5f);
			Main.dust[dust].noGravity = true;

			// Face dash direction instantly for impact
			if (Projectile.velocity != Vector2.Zero)
			{
				targetRotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
				Projectile.rotation = targetRotation; // Snap to direction for punchiness
			}

			if (timer > 40) // Dash duration
			{
				state = 0;
				timer = 0;
			}
		}
		
		// Smoothly interpolate rotation to give it a heavy, organic, purposeful feel (except during dash where it snaps)
		if (state != 2)
		{
			Projectile.rotation = Utils.AngleLerp(Projectile.rotation, targetRotation, 0.15f);
		}
	}
}
