using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Projectiles;

/// <summary>
/// Shared base for the per-tier hostile "shard" bolts the Animate bosses spray during their
/// bullet-hell phases: a slowly spinning, trailing projectile that ignores tiles so the patterns
/// stay readable. A concrete tier is pure declaration — it states its tint, trail dust, hitbox
/// and draw scale; the spin, lighting, dust trail and afterimage drawing all live here.
/// </summary>
public abstract class SmallBossShardProjectile : ModProjectile
{
	/// <summary>RGB light the bolt casts — matches its tier colour.</summary>
	protected abstract Vector3 LightColor { get; }

	/// <summary>Dust spat out as the trail.</summary>
	protected abstract int TrailDust { get; }

	/// <summary>Square hitbox edge; the calmer tiers read fine at 16, the showier ones at 20.</summary>
	protected virtual int HitboxSize => 16;

	/// <summary>Draw scale; &gt;1 makes the bolt read bigger for visibility without bloating the hitbox.</summary>
	protected virtual float DrawScale => 1f;

	public override void SetStaticDefaults()
	{
		ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
		ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
	}

	public override void SetDefaults()
	{
		Projectile.width = HitboxSize;
		Projectile.height = HitboxSize;
		Projectile.hostile = true;
		Projectile.friendly = false;
		Projectile.penetrate = -1;
		Projectile.timeLeft = 300;
		Projectile.ignoreWater = true;
		Projectile.tileCollide = false; // predictable bullet-hell — ignore terrain
		Projectile.scale = DrawScale;
	}

	public override void AI()
	{
		Projectile.rotation += 0.1f * (Projectile.velocity.X > 0 ? 1f : -1f);

		Lighting.AddLight(Projectile.Center, LightColor.X, LightColor.Y, LightColor.Z);

		if (Main.rand.NextBool(3))
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, TrailDust);
	}

	public override Color? GetAlpha(Color lightColor) => Color.White; // draw full-bright

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
