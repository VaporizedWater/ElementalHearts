using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using ReLogic.Graphics;
using ElementalHearts.Common.Players;

namespace ElementalHearts.Content.Projectiles;

public class KingSlimeComboTextProjectile : ModProjectile
{
	// Override texture to a vanilla invisible/placeholder texture to avoid MissingResourceException
	public override string Texture => "Terraria/Images/Item_0";

	public override void SetDefaults()
	{
		Projectile.width = 1;
		Projectile.height = 1;
		Projectile.friendly = false;
		Projectile.hostile = false;
		Projectile.tileCollide = false;
		Projectile.ignoreWater = true;
		Projectile.timeLeft = 90; // 1.5 seconds
		Projectile.penetrate = -1;
	}

	public override void OnSpawn(IEntitySource source)
	{
		int combo = (int)Projectile.ai[0];
		int flags = (int)Projectile.ai[1];
		bool killed = (flags & 1) != 0;
		bool wasGroundPounding = (flags & 2) != 0;

		SoundStyle baseSound = killed 
			? new SoundStyle("ElementalHearts/Assets/Sounds/PlayerBounceKill")
			: new SoundStyle("ElementalHearts/Assets/Sounds/PlayerBounce");
		
		SoundEngine.PlaySound(baseSound, Projectile.Center);

		// Random attack overlay sound (1 through 4)
		int randomAttack = Main.rand.Next(1, 5);
		SoundStyle attackOverlay = new SoundStyle($"ElementalHearts/Assets/Sounds/PlayerBounceAttack_{randomAttack}");
		SoundEngine.PlaySound(attackOverlay, Projectile.Center);

		string comboSoundPath = combo switch
		{
			1 => "ElementalHearts/Assets/Sounds/PlayerBounceNice1",
			2 => "ElementalHearts/Assets/Sounds/PlayerBounceNice2",
			3 => "ElementalHearts/Assets/Sounds/PlayerBounceNice3",
			4 => "ElementalHearts/Assets/Sounds/PlayerBounceGood",
			5 => "ElementalHearts/Assets/Sounds/PlayerBounceGreat",
			6 => "ElementalHearts/Assets/Sounds/PlayerBounceWonderful",
			_ => "ElementalHearts/Assets/Sounds/PlayerBounceExcellent"
		};

		Player player = Main.player[Projectile.owner];
		bool hasEncumbering = player.active && player.GetModPlayer<EncumberingAbilityPlayer>().Enabled;

		if (wasGroundPounding)
		{
			SoundEngine.PlaySound(new SoundStyle("ElementalHearts/Assets/Sounds/PlayerGroundPoundLandClean"), Projectile.Center);
			if (hasEncumbering)
			{
				SoundStyle comboSound = new SoundStyle(comboSoundPath);
				SoundEngine.PlaySound(comboSound, Projectile.Center);
			}
		}
		else
		{
			SoundStyle comboSound = new SoundStyle(comboSoundPath);
			SoundEngine.PlaySound(comboSound, Projectile.Center);
		}
	}

	public override void AI()
	{
		// Float upwards slowly
		Projectile.velocity.Y *= 0.95f;
	}

	public override bool PreDraw(ref Color lightColor)
	{
		int combo = (int)Projectile.ai[0];
		string text = GetComboText(combo);
		Color color = GetComboColor(combo);
		
		// Scale gets bigger up to GOOD (combo 4)
		float targetScale = 1f;
		if (combo == 1) targetScale = 1.0f;
		else if (combo == 2) targetScale = 1.3f;
		else if (combo == 3) targetScale = 1.6f;
		else targetScale = 2.0f; // 4+

		float scale = targetScale;
		int timeLived = 90 - Projectile.timeLeft;

		// Pop-in effect: scales up slightly past the target, then settles down
		if (timeLived < 6)
		{
			float t = timeLived / 6f; // 0 to 1
			scale = targetScale * (t * 1.3f); // overshoots up to 1.3x
		}
		else if (timeLived < 12)
		{
			float t = (timeLived - 6f) / 6f; // 0 to 1
			scale = targetScale * MathHelper.Lerp(1.3f, 1f, t); // settles back to targetScale
		}

		// Fade out
		float alpha = 1f;
		if (Projectile.timeLeft < 20)
		{
			alpha = Projectile.timeLeft / 20f;
		}

		color *= alpha;

		Vector2 drawPos = Projectile.Center - Main.screenPosition;
		
		DynamicSpriteFont font = FontAssets.MouseText.Value;
		
		Vector2 textSize = font.MeasureString(text) * scale;
		Vector2 origin = textSize / (2f * scale); // Origin is measured in unscaled coordinates

		// We can offset the text slightly based on a sine wave if we want it to wiggle, but fixed is fine
		ChatManager.DrawColorCodedStringWithShadow(
			Main.spriteBatch, 
			font, 
			text, 
			drawPos, 
			color, 
			0f, 
			origin, 
			new Vector2(scale)
		);

		return false; // Don't draw standard projectile texture
	}

	private string GetComboText(int combo)
	{
		return combo switch
		{
			1 => "Nice",
			2 => "Nice",
			3 => "NICE",
			4 => "GOOD",
			5 => "GREAT",
			6 => "WONDERFUL",
			_ => "EXCELLENT"
		};
	}

	private Color GetComboColor(int combo)
	{
		return combo switch
		{
			1 => new Color(75, 0, 130), // Indigo
			2 => new Color(75, 0, 130),
			3 => new Color(75, 0, 130),
			4 => Color.LimeGreen, // Green
			5 => Color.Orange,
			6 => Color.Red,
			_ => new Color(192, 192, 192) // Metallic Silver
		};
	}
}
