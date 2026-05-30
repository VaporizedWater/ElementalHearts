using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace ElementalHearts.Content.Projectiles;

public class RareShardProjectile : ModProjectile
{
	public override string Texture => "ElementalHearts/Content/Projectiles/RareSmallBossProjectile";

	public override void SetStaticDefaults()
	{
		ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
		ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
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
		Projectile.tileCollide = false;
		Projectile.scale = 1f;
	}

	public override void AI()
	{
		Projectile.rotation += 0.1f * (Projectile.velocity.X > 0 ? 1f : -1f);

		Lighting.AddLight(Projectile.Center, 0.2f, 0.45f, 0.95f);

		if (Main.rand.NextBool(3))
		{
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch);
		}
	}

	public override Color? GetAlpha(Color lightColor)
	{
		return Color.White;
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
		Vector2 origin = texture.Size() / 2f;

		for (int i = 1; i < Projectile.oldPos.Length; i++)
		{
			Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
			Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length) * 0.5f;
			Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
		}

		Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), null, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
		return false;
	}
}
