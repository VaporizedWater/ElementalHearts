using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Content.Projectiles;

public class TwinsCloneProjectile : ModProjectile
{
	public override string Texture => "Terraria/Images/NPC_" + NPCID.Retinazer; // A recognizable twins sprite as placeholder

	public override void SetDefaults()
	{
		Projectile.width = 30;
		Projectile.height = 48;
		Projectile.friendly = true;
		Projectile.hostile = false;
		Projectile.penetrate = -1;
		Projectile.timeLeft = 600;
		Projectile.tileCollide = true;
		Projectile.ignoreWater = false;
	}

	public override void AI()
	{
		Projectile.velocity.Y += 0.3f;
		if (Projectile.velocity.Y > 10f) Projectile.velocity.Y = 10f;
		
		if (Projectile.velocity.X == 0)
		{
			Projectile.direction = Main.player[Projectile.owner].direction * -1;
			Projectile.velocity.X = Projectile.direction * 5f;
		}
		else
		{
			Projectile.velocity.X = Projectile.direction * 5f;
		}
		
		if (Main.rand.NextBool(3))
		{
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame);
		}
	}
	
	public override bool OnTileCollide(Vector2 oldVelocity)
	{
		if (Projectile.velocity.X != oldVelocity.X)
		{
			Projectile.velocity.Y = -6f;
			Projectile.velocity.X = oldVelocity.X;
		}
		return false;
	}

	public override bool PreDraw(ref Color lightColor)
	{
		return false; // Render invisible, just use dust
	}
}
