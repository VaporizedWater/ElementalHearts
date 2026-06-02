using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ElementalHearts.Common.Players;
using ElementalHearts.Content.Buffs;

namespace ElementalHearts.Content.Projectiles.Minions;

public class DestroyerProbeMinion : ModProjectile
{
	public override string Texture => "Terraria/Images/NPC_" + NPCID.Probe;

	public override void SetStaticDefaults()
	{
		Main.projFrames[Projectile.type] = 1;
		Main.projPet[Projectile.type] = true;
		ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
		ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
		ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
	}

	public override void SetDefaults()
	{
		Projectile.width = 30;
		Projectile.height = 30;
		Projectile.tileCollide = false;
		Projectile.friendly = true;
		Projectile.minion = true;
		Projectile.minionSlots = 0f;
		Projectile.penetrate = -1;
		Projectile.timeLeft = 18000;
		Projectile.scale = 0.6f; // 20% larger than 0.5f
	}

	public override bool? CanCutTiles() => false;
	public override bool MinionContactDamage() => false;

	public override void AI()
	{
		Player player = Main.player[Projectile.owner];
		if (!player.active || player.dead)
		{
			player.GetModPlayer<DestroyerAbilityPlayer>().Enabled = false;
		}
		if (player.GetModPlayer<DestroyerAbilityPlayer>().Enabled)
		{
			Projectile.timeLeft = 2;
		}

		// Calculate fire rate based on progression (Max once every 7 seconds)
		int fireRate = 600; // 10 seconds
		if (Main.hardMode) fireRate = 540; // 9 seconds
		if (NPC.downedMechBossAny) fireRate = 480; // 8 seconds
		if (NPC.downedPlantBoss) fireRate = 450; // 7.5 seconds
		if (NPC.downedMoonlord) fireRate = 420; // 7 seconds maximum frequency

		float maxDetectRadius = 1200f; // Sniper range
		NPC target = null;

		// Find target without the debuff, prioritize MOST HP
		for (int i = 0; i < Main.maxNPCs; i++)
		{
			NPC npc = Main.npc[i];
			if (npc.CanBeChasedBy(this) && !npc.HasBuff(ModContent.BuffType<DestroyerTargetDebuff>()))
			{
				float sqrDistanceToTarget = Vector2.DistanceSquared(npc.Center, Projectile.Center);
				if (sqrDistanceToTarget < maxDetectRadius * maxDetectRadius)
				{
					// Require line of sight for sniper
					if (Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height))
					{
						if (target == null || npc.lifeMax > target.lifeMax || (npc.lifeMax == target.lifeMax && npc.life > target.life))
						{
							target = npc;
						}
					}
				}
			}
		}

		// Find base desired position (opposite of Servant)
		Vector2 basePositionOffset = new Vector2(-player.direction * 20, -25); // Very close fallback
		for (int i = 0; i < Main.maxProjectiles; i++)
		{
			Projectile proj = Main.projectile[i];
			if (proj.active && proj.owner == Projectile.owner && proj.type == ModContent.ProjectileType<ServantOfCthulhuMinion>())
			{
				Vector2 servantOffset = proj.Center - player.Center;
				if (servantOffset.LengthSquared() > 10f)
				{
					servantOffset.Normalize();
					basePositionOffset = -servantOffset * 30f; // 30 pixels away (tucked right next to the player)
				}
				break;
			}
		}

		// Movement
		if (target != null)
		{
			// Add some tighter organic combat bobbing
			Vector2 desiredPosition = player.Center + basePositionOffset + new Vector2((float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f) * 12f, (float)Math.Cos(Main.GlobalTimeWrappedHourly * 1.5f) * 8f);
			Vector2 vectorToDesired = desiredPosition - Projectile.Center;
			float distToDesired = vectorToDesired.Length();
			if (distToDesired > 10f)
			{
				vectorToDesired.Normalize();
				Projectile.velocity = (Projectile.velocity * 20f + vectorToDesired * 6f) / 21f;
			}
		}
		else
		{
			// Idle very closely near player
			Vector2 desiredPosition = player.Center + basePositionOffset + new Vector2(0, (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f) * 5f);
			Vector2 vectorToDesired = desiredPosition - Projectile.Center;
			float distToDesired = vectorToDesired.Length();
			
			if (distToDesired > 2000f)
			{
				Projectile.Center = desiredPosition;
			}
			else if (distToDesired > 15f)
			{
				vectorToDesired.Normalize();
				// Slower, smoother acceleration when idle
				Projectile.velocity = (Projectile.velocity * 40f + vectorToDesired * 4f) / 41f; 
			}
			else
			{
				// Smoothly calm down and stop
				Projectile.velocity *= 0.92f; 
			}
		}

		// Rotation: smooth organic adapting to face target or player
		float targetRotation;
		if (target != null) 
		{
			if (Projectile.ai[0] > fireRate - 30)
			{
				targetRotation = (target.Center - Projectile.Center).ToRotation() + MathHelper.Pi;
			}
			else if (Projectile.velocity.LengthSquared() > 0.1f)
			{
				targetRotation = Projectile.velocity.ToRotation() + MathHelper.Pi;
			}
			else
			{
				targetRotation = (target.Center - Projectile.Center).ToRotation() + MathHelper.Pi;
			}
		}
		else 
		{
			targetRotation = (player.Center - Projectile.Center).ToRotation() + MathHelper.Pi;
		}
		
		// Unwind angles for smooth lerp
		float diff = MathHelper.WrapAngle(targetRotation - Projectile.rotation);
		Projectile.rotation += diff * 0.15f;

		// Dust trail for organic feel
		if (Main.rand.NextBool(5))
		{
			Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, 0f, 0f, 100, default, 0.5f);
			dust.noGravity = true;
			dust.velocity *= 0.3f;
		}

		// Attack
		if (target != null)
		{
			Projectile.ai[0]++;
			// Sniper laser sight 0.5s before firing
			if (Projectile.ai[0] > fireRate - 30) 
			{
				Vector2 sightDir = target.Center - Projectile.Center;
				sightDir.Normalize();
				for(int i = 0; i < 3; i++) 
				{
					Dust sightDust = Dust.NewDustPerfect(Projectile.Center + sightDir * Main.rand.NextFloat(0, Vector2.Distance(Projectile.Center, target.Center)), DustID.RedTorch, Vector2.Zero, 100, default, 0.8f);
					sightDust.noGravity = true;
				}
			}

			if (Projectile.ai[0] >= fireRate)
			{
				Projectile.ai[0] = 0;

				// Lead the target (Iterative prediction for high accuracy)
				float bulletSpeed = 20f * 21f; // 20f base velocity * 21 updates per tick (20 extraUpdates)
				
				float timeToHit = Vector2.Distance(Projectile.Center, target.Center) / bulletSpeed;
				Vector2 predictedPosition = target.Center + (target.velocity * timeToHit);
				
				// Iteration 2
				timeToHit = Vector2.Distance(Projectile.Center, predictedPosition) / bulletSpeed;
				predictedPosition = target.Center + (target.velocity * timeToHit);
				
				// Iteration 3
				timeToHit = Vector2.Distance(Projectile.Center, predictedPosition) / bulletSpeed;
				predictedPosition = target.Center + (target.velocity * timeToHit);
				
				Vector2 shootVel = predictedPosition - Projectile.Center;
				shootVel.Normalize();
				shootVel *= 20f; // 25% faster than 16f

				// Visual and audio juice
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item12.WithVolumeScale(0.7f).WithPitchOffset(0.3f), Projectile.Center);
				for (int i = 0; i < 10; i++)
				{
					Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.RedTorch, shootVel.X * 0.5f, shootVel.Y * 0.5f, 100, default, 1.2f);
					d.noGravity = true;
				}

				// Fire projectile with 1 damage so it registers hits reliably
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, shootVel, ModContent.ProjectileType<DestroyerLaserProjectile>(), 1, 0f, Projectile.owner);
			}
		}
		else
		{
			// If line of sight breaks or no target, hold the wind-up
			if (Projectile.ai[0] > fireRate - 30)
			{
				Projectile.ai[0] = fireRate - 30; // Force full 0.5s laser sight wind-up when a clean line is found
			}
			else
			{
				Projectile.ai[0]++;
			}
		}
	}
}
