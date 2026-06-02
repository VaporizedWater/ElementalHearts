using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace ElementalHearts.Common.UI.Elements;

public class UIAnimatedButton : UITextPanel<string>
{
	private Color _baseColor;
	private Color _hoverColor;
	private Color _baseBorder;
	private Color _hoverBorder;
	
	private float _hoverTimer = 0f;
	
	public bool IsSelected { get; set; }

	public UIAnimatedButton(string text, float textScale = 1f, bool large = false) : base(text, textScale, large)
	{
		_baseColor = new Color(63, 82, 151) * 0.7f;
		_hoverColor = new Color(73, 94, 171);
		_baseBorder = new Color(89, 116, 213);
		_hoverBorder = new Color(150, 170, 255);
		
		BackgroundColor = _baseColor;
		BorderColor = _baseBorder;
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
		
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
		BackgroundColor = Color.Lerp(_baseColor, _hoverColor, _hoverTimer);
		
		// Pulsing border on hover
		if (visuallyHovered)
		{
			float pulse = (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 6f) * 0.5f + 0.5f;
			BorderColor = Color.Lerp(_hoverBorder, Color.White, pulse * 0.4f * _hoverTimer);
		}
		else
		{
			BorderColor = Color.Lerp(_baseBorder, _hoverBorder, _hoverTimer);
		}
	}
	
	public override void MouseOver(UIMouseEvent evt)
	{
		base.MouseOver(evt);
		Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
	}
}
