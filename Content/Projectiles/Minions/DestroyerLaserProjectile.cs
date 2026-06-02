using ElementalHearts.Content.Buffs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Projectiles.Minions;

public class DestroyerLaserProjectile : ModProjectile
{
	public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DeathLaser;

	public override void SetDefaults()
	{
		Projectile.width = 4;
		Projectile.height = 4;
		Projectile.friendly = true;
		Projectile.hostile = false;
		Projectile.penetrate = 1;
		Projectile.timeLeft = 600;
		Projectile.extraUpdates = 20; // Hitscan sniper speeds
		Projectile.ignoreWater = true;
		Projectile.ArmorPenetration = 100;
	}

	public override void AI()
	{
		Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
		Lighting.AddLight(Projectile.Center, 0.8f, 0f, 0f);

		if (Main.rand.NextBool(2))
		{
			Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.RedTorch, 0f, 0f, 100, default, 1.2f);
			d.noGravity = true;
			d.velocity *= 0.1f;
		}
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		target.AddBuff(ModContent.BuffType<DestroyerTargetDebuff>(), 900); // 15 seconds
		
		// Visual explosion on hit
		for (int i = 0; i < 15; i++)
		{
			Dust d = Dust.NewDustDirect(target.position, target.width, target.height, DustID.RedTorch, Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 100, default, 1.5f);
			d.noGravity = true;
		}
	}
}
