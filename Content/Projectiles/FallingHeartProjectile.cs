using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.Hearts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Projectiles;

public class FallingHeartProjectile : ModProjectile
{
	public override string Texture => "Terraria/Images/Item_" + ItemID.FallenStar;

	public override void SetDefaults()
	{
		Projectile.width = 16;
		Projectile.height = 16;
		Projectile.friendly = true;
		Projectile.penetrate = -1;
		Projectile.light = 0.5f;
		Projectile.tileCollide = true;
		Projectile.ignoreWater = false;
	}

	public override void AI()
	{
		// ai[0] is the Item.type of the heart.
		Projectile.rotation += Projectile.velocity.X * 0.05f;

		// Acceleration downwards (gravity)
		Projectile.velocity.Y += 0.2f;
		if (Projectile.velocity.Y > 16f)
			Projectile.velocity.Y = 16f;

		// Light and dust based on the heart tier
		int itemType = (int)Projectile.ai[0];
		if (itemType > 0 && ModContent.GetModItem(itemType) is ElementalHeartItem heart)
		{
			Color tierColor = heart.Tier.GetEffectColor();
			Lighting.AddLight(Projectile.Center, tierColor.ToVector3() * 0.8f);

			if (Main.rand.NextBool(3))
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.FireworksRGB, 0f, 0f, 100, tierColor, 1.2f);
				dust.noGravity = true;
				dust.velocity *= 0.5f;
			}
			if (Main.rand.NextBool(5))
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GemDiamond, 0f, 0f, 100, tierColor, 0.8f);
				dust.noGravity = true;
				dust.velocity *= 0.5f;
			}
		}

		// Vanilla falling stars die at daytime.
		if (Main.dayTime)
			Projectile.Kill();
	}

	public override bool PreDraw(ref Color lightColor)
	{
		int itemType = (int)Projectile.ai[0];
		if (itemType <= 0)
			return false;

		Main.instance.LoadItem(itemType);
		Texture2D texture = TextureAssets.Item[itemType].Value;
		
		// Use animation frame if the heart has an animation sheet.
		Rectangle? sourceRect = null;
		if (Main.itemAnimations[itemType] != null)
			sourceRect = Main.itemAnimations[itemType].GetFrame(texture);

		Vector2 drawPos = Projectile.Center - Main.screenPosition;
		Vector2 origin = sourceRect.HasValue ? new Vector2(sourceRect.Value.Width, sourceRect.Value.Height) / 2f : texture.Size() / 2f;

		// Draw a simple glowing trail/bloom
		if (ModContent.GetModItem(itemType) is ElementalHeartItem heart)
		{
			Color tierColor = heart.Tier.GetEffectColor() * 0.5f;
			for (int i = 0; i < 4; i++)
			{
				Vector2 offset = Projectile.velocity * -1 * (i / 4f) * 3f;
				Main.EntitySpriteDraw(texture, drawPos + offset, sourceRect, tierColor, Projectile.rotation, origin, Projectile.scale * 1.1f, SpriteEffects.None, 0);
			}
		}

		Main.EntitySpriteDraw(texture, drawPos, sourceRect, Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

		return false;
	}

	public override void Kill(int timeLeft)
	{
		int itemType = (int)Projectile.ai[0];

		// Play effect and drop item
		if (itemType > 0 && ModContent.GetModItem(itemType) is ElementalHeartItem heart)
		{
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position); // Star hit sound

			// Burst dust
			Color tierColor = heart.Tier.GetEffectColor();
			for (int i = 0; i < 15; i++)
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.FireworksRGB, 0f, 0f, 100, tierColor, 1.5f);
				dust.velocity *= 2f;
				dust.noGravity = true;
			}

			// Drop the actual item
			if (Main.netMode != NetmodeID.MultiplayerClient && !Main.dayTime)
			{
				Item.NewItem(Projectile.GetSource_DropAsItem(), Projectile.Center, itemType);
			}
		}
	}
}
