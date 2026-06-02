using System;
using ElementalHearts.Content.Buffs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ElementalHearts.Common.Players;

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

		if (player.dead || !player.active || !player.GetModPlayer<EyeOfCthulhuAbilityPlayer>().Enabled)
		{
			player.ClearBuff(ModContent.BuffType<ServantOfCthulhuBuff>());
		}
		if (player.HasBuff(ModContent.BuffType<ServantOfCthulhuBuff>()))
		{
			Projectile.timeLeft = 2;
		}

		bool isUpgraded = player.GetModPlayer<TwinsAbilityPlayer>().Enabled && player.GetModPlayer<EyeOfCthulhuAbilityPlayer>().Enabled;

		// Custom animation
		Projectile.frameCounter++;
		if (Projectile.frameCounter >= 5)
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

		// UPGRADED ATTACK: Shoot cursed flames independently of dash state
		if (isUpgraded)
		{
			NPC shootTarget = null;
			float closestShootDist = 600f; // Shooting range
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (npc.CanBeChasedBy() && Collision.CanHit(Projectile.Center, 1, 1, npc.Center, 1, 1))
				{
					float dist = Vector2.Distance(Projectile.Center, npc.Center);
					if (dist < closestShootDist)
					{
						closestShootDist = dist;
						shootTarget = npc;
					}
				}
			}

			ref float cycleTimer = ref Projectile.localAI[0];

			if (shootTarget != null)
			{
				// 1,0,1,0,0,0,0,0 pattern over 8 seconds.
				// 1st second (0) and 3rd second (120)
				if (cycleTimer == 0 || cycleTimer == 120)
				{
					// Calculate direction based on visual rotation, adding PiOver2 to undo the sprite downward offset
					Vector2 velocity = (Projectile.rotation + MathHelper.PiOver2).ToRotationVector2() * 10f;
					if (player.whoAmI == Main.myPlayer)
					{
						Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<SpazmatismCursedFlame>(), Projectile.damage, 0f, player.whoAmI);
					}
				}
				
				cycleTimer++;
				if (cycleTimer >= 480) // 8 seconds total cycle
				{
					cycleTimer = 0;
				}
			}
			else
			{
				// Always advance the cycle timer so he can return to dashing if no target is found
				cycleTimer++;
				if (cycleTimer >= 480)
				{
					cycleTimer = 0;
				}
			}
		}

		if (state == 0) // Idle / Orbit
		{
			// Find closest target, prioritizing debuffed enemies
			NPC target = null;
			float closestDist = 350f; // Targeting range
			bool foundDebuffedTarget = false;

			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (npc.CanBeChasedBy() && Collision.CanHit(Projectile.Center, 1, 1, npc.Center, 1, 1))
				{
					bool hasDebuff = npc.HasBuff(ModContent.BuffType<DestroyerTargetDebuff>());
					float dist = Vector2.Distance(player.Center, npc.Center);
					
					if (hasDebuff && !foundDebuffedTarget)
					{
						// Prioritize the first debuffed enemy in an extended range
						if (dist < 1200f)
						{
							target = npc;
							closestDist = dist;
							foundDebuffedTarget = true;
						}
					}
					else if (hasDebuff && foundDebuffedTarget)
					{
						// If multiple debuffed targets, find the closest one of them
						if (dist < closestDist)
						{
							closestDist = dist;
							target = npc;
						}
					}
					else if (!foundDebuffedTarget)
					{
						// Normal targeting if no debuffed target found yet
						if (dist < closestDist)
						{
							closestDist = dist;
							target = npc;
						}
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
				
				// Move smoothly towards orbit position with a strict maximum speed
				Vector2 direction = targetPos - Projectile.Center;
				float distToTargetPos = direction.Length();
				
				if (distToTargetPos > 2000f)
				{
					Projectile.Center = player.Center; // Teleport if left far behind
				}
				else if (distToTargetPos > 5f)
				{
					direction.Normalize();
					direction *= Math.Min(distToTargetPos * 0.05f, 10f); // Strict maximum speed limit of 10f
					Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction, 0.05f); // Smooth, intentional acceleration
				}
				else
				{
					Projectile.velocity *= 0.8f; // Decelerate smoothly when arrived
				}

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

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		// Apply natural, physics-based bounce when colliding with an enemy
		if (target.Center != Projectile.Center)
		{
			// The collision normal (pointing from the enemy center to the minion)
			Vector2 normal = Projectile.Center - target.Center;
			normal.Normalize();

			// Calculate how much the minion's velocity is moving INTO the enemy
			float dotProduct = Vector2.Dot(Projectile.velocity, normal);

			// Only bounce if he is actually moving towards the target
			if (dotProduct < 0)
			{
				// Standard physics reflection formula: V - 2 * (V dot N) * N
				Projectile.velocity = Projectile.velocity - 2f * dotProduct * normal;
				
				// Apply restitution (elasticity) so he loses energy on impact
				Projectile.velocity *= 0.4f; 
			}
			else
			{
				// Fallback nudge just in case
				Projectile.velocity += normal * 5f;
			}

			// Strictly clamp the maximum knockback speed so he doesn't go flying too fast
			if (Projectile.velocity.Length() > 12f)
			{
				Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 12f;
			}
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Player player = Main.player[Projectile.owner];
		bool isUpgraded = player.GetModPlayer<TwinsAbilityPlayer>().Enabled && player.GetModPlayer<EyeOfCthulhuAbilityPlayer>().Enabled;

		Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
		float drawRotation = Projectile.rotation;
		
		if (isUpgraded && ModContent.HasAsset("ElementalHearts/Assets/NPCs/SpazmatismServant"))
		{
			texture = ModContent.Request<Texture2D>("ElementalHearts/Assets/NPCs/SpazmatismServant").Value;
			// The new sprite points right natively, while the vanilla one points down.
			// The AI assumes a down-pointing sprite (subtracting Pi/2), so we add Pi/2 back for this sprite.
			drawRotation += MathHelper.PiOver2;
		}

		int frameHeight = texture.Height / Main.projFrames[Projectile.type];
		int startY = frameHeight * Projectile.frame;
		Rectangle sourceRectangle = new Rectangle(0, startY, texture.Width, frameHeight);
		Vector2 origin = sourceRectangle.Size() / 2f;

		Main.EntitySpriteDraw(texture,
			Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
			sourceRectangle, lightColor, drawRotation, origin, Projectile.scale, SpriteEffects.None, 0);

		return false;
	}
}
