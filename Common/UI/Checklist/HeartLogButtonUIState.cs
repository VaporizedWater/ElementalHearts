using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using ElementalHearts.Content.Items.Vanilla.Legendary;

namespace ElementalHearts.Common.UI.Checklist;

public class HeartLogButtonUIState : UIState
{
	private UIElement _openButton;
	private float _scaleJuice = 1f;
	public static bool HasUnseenContent = true;

	public override void OnInitialize()
	{
		Main.instance.LoadItem(ModContent.ItemType<AstraHeart>());
		
		_openButton = new UIElement();
		_openButton.Width.Set(34, 0f);
		_openButton.Height.Set(34, 0f);
		_openButton.OnLeftClick += (evt, element) => {
			Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuOpen);
			_scaleJuice = 1.4f; // Give it a bouncy click juice!
			ChecklistUISystem.ToggleUI();
		};
		_openButton.OnMouseOver += (evt, element) => {
			Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
		};
		Append(_openButton);
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
		
		// Boss Checklist's Boss Log button is at roughly Main.screenWidth - 264, Main.screenHeight - 42.
		// Moving this button left and vertically centered with it.
		_openButton.Left.Set(Main.screenWidth - 315, 0f);
		_openButton.Top.Set(Main.screenHeight - 48, 0f);

		if (_openButton.IsMouseHovering && HasUnseenContent)
		{
			HasUnseenContent = false;
		}

		// Juice interpolation
		float targetScale = _openButton.IsMouseHovering ? 1.15f : 1f;
		_scaleJuice = Microsoft.Xna.Framework.MathHelper.Lerp(_scaleJuice, targetScale, 0.2f);
	}

	protected override void DrawSelf(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);

		var texture = Terraria.GameContent.TextureAssets.Item[ModContent.ItemType<AstraHeart>()].Value;
		var dimensions = _openButton.GetDimensions();

		// Scale the heart down so it isn't massive and matches the aesthetic of the boss log book
		float baseScale = 1f;
		if (texture.Width > 28 || texture.Height > 28)
		{
			baseScale = 28f / System.Math.Max(texture.Width, texture.Height);
		}

		float finalScale = baseScale * _scaleJuice;
		Color color = _openButton.IsMouseHovering ? Color.White : Color.White * 0.75f;
		Vector2 position = dimensions.Center();

		spriteBatch.Draw(
			texture,
			position,
			null,
			color,
			0f,
			texture.Size() / 2f,
			finalScale,
			Microsoft.Xna.Framework.Graphics.SpriteEffects.None,
			0f
		);

		if (_openButton.IsMouseHovering)
		{
			// Determine the correct outline sprite based on the player's highest tier
			var player = Main.LocalPlayer.GetModPlayer<Common.Players.HeartConsumptionPlayer>();
			Common.Hearts.HeartTier? currentTier = player.HighestTier;
			
			string outlineTexturePath = "ElementalHearts/Assets/UI/HeartOutline";
			if (currentTier.HasValue)
			{
				switch (currentTier.Value)
				{
					case Common.Hearts.HeartTier.Uncommon:
						outlineTexturePath = "ElementalHearts/Assets/UI/UncommonHeartOutline";
						break;
					case Common.Hearts.HeartTier.Rare:
						outlineTexturePath = "ElementalHearts/Assets/UI/RareHeartOutline";
						break;
					case Common.Hearts.HeartTier.Epic:
						outlineTexturePath = "ElementalHearts/Assets/UI/EpicHeartOutline";
						break;
					case Common.Hearts.HeartTier.Legendary:
					case Common.Hearts.HeartTier.Mythic:
						outlineTexturePath = "ElementalHearts/Assets/UI/LegendaryHeartOutline";
						break;
				}
			}

			Microsoft.Xna.Framework.Graphics.Texture2D outlineTexture = ModContent.Request<Microsoft.Xna.Framework.Graphics.Texture2D>(outlineTexturePath).Value;

			// Draw the custom outline sprite OVER the heart
			spriteBatch.Draw(
				outlineTexture,
				position,
				null,
				Color.White,
				0f,
				outlineTexture.Size() / 2f,
				finalScale,
				Microsoft.Xna.Framework.Graphics.SpriteEffects.None,
				0f
			);
		}

		if (_openButton.IsMouseHovering)
		{
			// Draw the tooltip centered above the button
			string text = "Hearts";
			Vector2 stringSize = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(text);
			Vector2 textPosition = new Vector2(position.X - stringSize.X / 2f, position.Y - (texture.Height / 2f * finalScale) - 26f);

			Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(
				spriteBatch,
				Terraria.GameContent.FontAssets.MouseText.Value,
				text,
				textPosition,
				Color.White,
				0f,
				Vector2.Zero,
				Vector2.One
			);
		}
		else if (HasUnseenContent)
		{
			// Draw the custom TipLabel sprite with a clean and subtle bob/pulse
			float floatOffset = (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 2.5f) * 2f;
			float exScale = 1f + (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 5f) * 0.05f;
			
			Microsoft.Xna.Framework.Graphics.Texture2D tipTexture = ModContent.Request<Microsoft.Xna.Framework.Graphics.Texture2D>("ElementalHearts/Assets/UI/TipLabel").Value;
			Vector2 tipPosition = new Vector2(position.X, position.Y - (texture.Height / 2f * finalScale) - 14f + floatOffset);

			spriteBatch.Draw(
				tipTexture,
				tipPosition,
				null,
				Color.White,
				0f,
				tipTexture.Size() / 2f,
				exScale,
				Microsoft.Xna.Framework.Graphics.SpriteEffects.None,
				0f
			);
		}
	}
}
