using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace ElementalHearts.Common.UI.Elements;

public class UIAnimatedButton : AuroraPanel
{
	public Color BaseColor { get; set; } = new Color(20, 26, 48);
	public Color HoverColor { get; set; } = new Color(50, 65, 120);
	
	private float _hoverTimer = 0f;
	
	public float? FixedWidth { get; set; } = null;
	public float? FixedHeight { get; set; } = null;
	
	public bool IsSelected { get; set; }

	public UIText TextElement { get; private set; }
	
	private string _currentText;
	private float _currentScale;
	private bool _currentLarge;

	public UIAnimatedButton(string text, float textScale = 1f, bool large = false)
	{
		_currentText = text;
		_currentScale = textScale;
		_currentLarge = large;
		IsInteractive = true;

		AuroraColor1 = BaseColor;
		AuroraColor2 = BaseColor * 0.5f;
		AuroraColor3 = BaseColor * 1.2f;

		TextElement = new UIText(text, textScale, large);
		TextElement.HAlign = 0.5f;
		TextElement.VAlign = 0.5f;
		Append(TextElement);

		SetPadding(8f); // standard padding to replace UITextPanel padding
		UpdateSize();
	}

	public void SetText(string text)
	{
		_currentText = text;
		TextElement.SetText(text);
		UpdateSize();
	}

	public override void Recalculate()
	{
		UpdateSize();
		base.Recalculate();
	}

	private void UpdateSize()
	{
		if (FixedWidth.HasValue)
		{
			Width.Set(FixedWidth.Value, 0f);
		}
		else
		{
			float textWidth = 100f; // Safe fallback
			if (_currentLarge && Terraria.GameContent.FontAssets.DeathText.IsLoaded)
			{
				Vector2 textSize = Terraria.UI.Chat.ChatManager.GetStringSize(Terraria.GameContent.FontAssets.DeathText.Value, _currentText, new Vector2(_currentScale));
				textWidth = textSize.X;
			}
			else if (!_currentLarge && Terraria.GameContent.FontAssets.MouseText.IsLoaded)
			{
				Vector2 textSize = Terraria.UI.Chat.ChatManager.GetStringSize(Terraria.GameContent.FontAssets.MouseText.Value, _currentText, new Vector2(_currentScale));
				textWidth = textSize.X;
			}
			Width.Set(textWidth + PaddingLeft + PaddingRight, 0f);
		}

		if (FixedHeight.HasValue)
		{
			Height.Set(FixedHeight.Value, 0f);
		}
		else
		{
			float textHeight = 20f;
			if (_currentLarge && Terraria.GameContent.FontAssets.DeathText.IsLoaded)
			{
				Vector2 textSize = Terraria.UI.Chat.ChatManager.GetStringSize(Terraria.GameContent.FontAssets.DeathText.Value, _currentText, new Vector2(_currentScale));
				textHeight = textSize.Y;
			}
			else if (!_currentLarge && Terraria.GameContent.FontAssets.MouseText.IsLoaded)
			{
				Vector2 textSize = Terraria.UI.Chat.ChatManager.GetStringSize(Terraria.GameContent.FontAssets.MouseText.Value, _currentText, new Vector2(_currentScale));
				textHeight = textSize.Y;
			}
			Height.Set(textHeight + PaddingTop + PaddingBottom, 0f);
		}
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
		
		// If font wasn't loaded during constructor, dimensions might be stuck at default. Update them.
		if (Width.Pixels == PaddingLeft + PaddingRight || Width.Pixels == 100f + PaddingLeft + PaddingRight)
		{
			float oldWidth = Width.Pixels;
			UpdateSize();
			if (Width.Pixels != oldWidth && Parent != null)
			{
				Parent.RecalculateChildren();
			}
		}

		bool visuallyHovered = IsMouseHovering || IsSelected;
		
		if (visuallyHovered)
		{
			_hoverTimer += 0.08f;
			if (_hoverTimer > 1f) _hoverTimer = 1f;
		}
		else
		{
			_hoverTimer -= 0.08f;
			if (_hoverTimer < 0f) _hoverTimer = 0f;
		}
		
		// Smooth color transition
		Color currentColor = Color.Lerp(BaseColor, HoverColor, _hoverTimer);
		AuroraColor1 = currentColor;
		AuroraColor2 = currentColor * 0.6f;
		AuroraColor3 = currentColor * 1.2f;
	}
	
	public override void MouseOver(UIMouseEvent evt)
	{
		base.MouseOver(evt);
		Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
	}
}
