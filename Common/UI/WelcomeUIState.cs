using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;
using ElementalHearts.Common.Players;

namespace ElementalHearts.Common.UI;

public class WelcomeUIState : UIState
{
	private UIPanel _mainPanel;
	private UIText _discordText;
	private UIText _closeButton;
	private UIText _checkboxText;
	private UIPanel _checkboxPanel;
	private UIText _timerText;
	private int _closeTimer = 1200; // 20 seconds

	public void ResetTimer()
	{
		_closeTimer = 1200;
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
		if (_closeTimer > 0)
		{
			_closeTimer--;
			int secondsLeft = (_closeTimer / 60) + 1;
			if (_closeTimer == 0) secondsLeft = 0;
			if (_timerText != null)
			{
				_timerText.SetText($"{secondsLeft}");
			}

			if (_closeTimer <= 0)
			{
				WelcomeUISystem.Hide();
			}
		}
	}

	public override void OnInitialize()
	{
		_mainPanel = new UIPanel();
		_mainPanel.Width.Set(350, 0f);
		_mainPanel.Height.Set(200, 0f);
		// Lower left quadrant, aligned with chat
		_mainPanel.Left.Set(88, 0f);
		_mainPanel.Top.Set(-330, 1f); 
		_mainPanel.BackgroundColor = new Color(33, 43, 79) * 0.8f;
		_mainPanel.SetPadding(12f); 
		Append(_mainPanel);

		// Welcome Title
		var titleText = new UIText("We need help!");
		titleText.HAlign = 0.5f;
		titleText.Top.Set(0, 0f);
		_mainPanel.Append(titleText);

		// Subtitle
		var subtitleText = new UIText("Testers, Spriters, Ideas", 0.8f);
		subtitleText.HAlign = 0.5f;
		subtitleText.Top.Set(25, 0f);
		_mainPanel.Append(subtitleText);

		// Timer Display (Text Only)
		_timerText = new UIText("20", 0.9f);
		_timerText.HAlign = 1f;
		_timerText.VAlign = 0f;
		_timerText.Top.Set(5, 0f);
		_timerText.Left.Set(-10, 0f);
		_mainPanel.Append(_timerText);

		// Discord Link Button
		var discordPanel = new UIPanel();
		discordPanel.Width.Set(250, 0f);
		discordPanel.Height.Set(45, 0f);
		discordPanel.HAlign = 0.5f;
		discordPanel.Top.Set(55, 0f);
		discordPanel.BackgroundColor = new Color(73, 94, 171);
		discordPanel.SetPadding(0);
		discordPanel.OnLeftClick += (evt, element) => {
			SoundEngine.PlaySound(SoundID.MenuOpen);
			Terraria.Utils.OpenToURL("https://discord.gg/7WmrGXdQWD");
		};
		discordPanel.OnMouseOver += (evt, element) => {
			discordPanel.BackgroundColor = new Color(93, 114, 191);
			SoundEngine.PlaySound(SoundID.MenuTick);
		};
		discordPanel.OnMouseOut += (evt, element) => {
			discordPanel.BackgroundColor = new Color(73, 94, 171);
		};
		_mainPanel.Append(discordPanel);

		_discordText = new UIText("Join the Discord", 0.9f);
		_discordText.HAlign = 0.5f;
		_discordText.VAlign = 0.5f;
		discordPanel.Append(_discordText);

		// Don't show again toggle
		var toggleContainer = new UIElement();
		toggleContainer.Width.Set(250, 0f);
		toggleContainer.Height.Set(20, 0f);
		toggleContainer.HAlign = 0.5f;
		toggleContainer.Top.Set(110, 0f);
		_mainPanel.Append(toggleContainer);

		_checkboxPanel = new UIPanel();
		_checkboxPanel.Width.Set(20, 0f);
		_checkboxPanel.Height.Set(20, 0f);
		_checkboxPanel.Left.Set(0, 0f);
		_checkboxPanel.VAlign = 0.5f;
		_checkboxPanel.SetPadding(0);
		_checkboxPanel.OnLeftClick += (evt, element) => {
			SoundEngine.PlaySound(SoundID.MenuTick);
			var player = Main.LocalPlayer.GetModPlayer<WelcomeMessagePlayer>();
			player.HideWelcomeMessage = !player.HideWelcomeMessage;
		};
		toggleContainer.Append(_checkboxPanel);

		_checkboxText = new UIText("Don't show this message again", 0.8f);
		_checkboxText.Left.Set(30, 0f);
		_checkboxText.VAlign = 0.5f;
		toggleContainer.Append(_checkboxText);

		// Close Button
		var closePanel = new UIPanel();
		closePanel.Width.Set(80, 0f);
		closePanel.Height.Set(30, 0f);
		closePanel.HAlign = 0.5f;
		closePanel.Top.Set(140, 0f);
		closePanel.SetPadding(0);
		closePanel.OnLeftClick += (evt, element) => {
			SoundEngine.PlaySound(SoundID.MenuClose);
			WelcomeUISystem.Hide();
		};
		closePanel.OnMouseOver += (evt, element) => {
			closePanel.BackgroundColor = new Color(100, 100, 100);
			SoundEngine.PlaySound(SoundID.MenuTick);
		};
		closePanel.OnMouseOut += (evt, element) => {
			closePanel.BackgroundColor = new Color(63, 82, 151) * 0.7f;
		};
		_mainPanel.Append(closePanel);

		_closeButton = new UIText("Close", 0.9f);
		_closeButton.HAlign = 0.5f;
		_closeButton.VAlign = 0.5f;
		closePanel.Append(_closeButton);
	}

	public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);

		if (Main.LocalPlayer != null && Main.LocalPlayer.active)
		{
			var player = Main.LocalPlayer.GetModPlayer<WelcomeMessagePlayer>();
			if (player.HideWelcomeMessage)
			{
				var dimensions = _checkboxPanel.GetDimensions();
				var rect = dimensions.ToRectangle();
				
				Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(
					spriteBatch,
					Terraria.GameContent.FontAssets.MouseText.Value,
					"X",
					new Vector2(rect.X + 4, rect.Y),
					Color.White,
					0f,
					Vector2.Zero,
					new Vector2(0.8f),
					-1f,
					1f
				);
			}
		}
	}
}
