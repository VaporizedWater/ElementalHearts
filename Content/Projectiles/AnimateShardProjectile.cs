using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using ElementalHearts.Content.Items.BossSpawns;

namespace ElementalHearts.Content.Projectiles;

public class AnimateShardProjectile : ModProjectile
{
	public override string Texture => "ElementalHearts/Content/Items/BossSpawns/CommonMenacingHeart";

	public override void SetStaticDefaults()
	{
		// DisplayName.SetDefault("Animate Shard");
	}

	public override void SetDefaults()
	{
		Projectile.width = 16;
		Projectile.height = 16;
		Projectile.hostile = true;
		Projectile.friendly = false;
		Projectile.penetrate = -1;
		Projectile.timeLeft = 300;
		Projectile.ignoreWater = true;
		Projectile.tileCollide = false; // We want it to be predictable bullet hell, so ignore tiles
		
		// Scale it down since we are using the Menacing Heart sprite
		Projectile.scale = 0.5f;
	}

	public override void AI()
	{
		Projectile.rotation += 0.1f * (Projectile.velocity.X > 0 ? 1f : -1f);
		
		Lighting.AddLight(Projectile.Center, 0.8f, 0.2f, 0.5f);
		
		// Add some dust trail
		if (Main.rand.NextBool(3))
		{
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.PinkCrystalShard);
		}
	}

	public override Color? GetAlpha(Color lightColor)
	{
		return Color.White; // Draw fullbright
	}
}
