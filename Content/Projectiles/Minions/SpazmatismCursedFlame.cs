using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Projectiles.Minions;

public class SpazmatismCursedFlame : ModProjectile
{
	// Point to the vanilla Cursed Flame Hostile texture (Spazmatism's projectile)
	public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.CursedFlameHostile;

	public override void SetStaticDefaults()
	{
		Main.projFrames[Projectile.type] = Main.projFrames[ProjectileID.CursedFlameHostile];
	}

	public override void SetDefaults()
	{
		Projectile.CloneDefaults(ProjectileID.CursedFlameHostile);
		Projectile.friendly = true;
		Projectile.hostile = false;
		Projectile.DamageType = DamageClass.Summon;
		Projectile.penetrate = 1; // dies on first hit
		Projectile.scale = 0.5f; // miniature
		
		// Use custom AI for subtle dust
		AIType = ProjectileID.None; 
	}

	public override void AI()
	{
		// Cursed flame visual rotation
		Projectile.rotation += 0.3f * Projectile.direction;
		
		// Animate the frames if the texture has multiple
		if (Main.projFrames[Projectile.type] > 1)
		{
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
		}

		// Very lenient and subtle dust (only 33% chance per frame)
		if (Main.rand.NextBool(3))
		{
			// Smaller dust scale and minimal velocity inheritance
			Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.CursedTorch, Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 150, default, Projectile.scale * 1.3f);
			dust.noGravity = true;
		}

		// Emit softer, more subtle light
		Lighting.AddLight(Projectile.Center, 0.2f, 0.5f, 0.1f);
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		// Apply Cursed Inferno for 4 seconds
		target.AddBuff(BuffID.CursedInferno, 240);
	}
}
