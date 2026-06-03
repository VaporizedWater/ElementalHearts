using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.Localization;
using ElementalHearts.Content.Items.Hearts;
using ElementalHearts.Common.Hearts;
using ElementalHearts.Common.Players;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;

namespace ElementalHearts.Common.UI.Elements;

public class UIHeartHoverCard : UIPanel
{
	private ElementalHeartItem _heart;
	private UIText _titleText;
	private UIText _tierText;
	private UIText _statsText;
	private UIText _loreText;
	private UIText _generationText;
	private UIHorizontalSeparator _sep1;
	private UIHorizontalSeparator _sep2;

	public UIHeartHoverCard()
	{
		Width.Set(350, 0f);
		Height.Set(250, 0f); // Will dynamically size
		BackgroundColor = new Color(15, 20, 38) * 0.95f;
		BorderColor = new Color(89, 116, 213);
		SetPadding(15f);

		_titleText = new UIText("Heart Name", 1.1f, false);
		_titleText.HAlign = 0.5f;
		_titleText.Top.Set(0, 0f);
		Append(_titleText);

		_tierText = new UIText("Rarity", 0.85f);
		_tierText.HAlign = 0.5f;
		_tierText.Top.Set(24, 0f);
		Append(_tierText);

		_sep1 = new UIHorizontalSeparator();
		_sep1.Width.Set(0, 1f);
		_sep1.Top.Set(45, 0f);
		_sep1.Color = new Color(89, 116, 213) * 0.7f;
		Append(_sep1);

		_statsText = new UIText("Stats go here", 0.9f);
		_statsText.HAlign = 0.5f;
		_statsText.Top.Set(58, 0f);
		Append(_statsText);

		_generationText = new UIText("", 0.9f);
		_generationText.HAlign = 0.5f;
		_generationText.Top.Set(82, 0f);
		Append(_generationText);

		_sep2 = new UIHorizontalSeparator();
		_sep2.Width.Set(0, 1f);
		_sep2.Top.Set(110, 0f);
		_sep2.Color = new Color(89, 116, 213) * 0.7f;
		Append(_sep2);

		_loreText = new UIText("Lore goes here", 0.85f);
		_loreText.Width.Set(0, 1f);
		_loreText.Top.Set(125, 0f);
		_loreText.IsWrapped = true;
		_loreText.TextColor = new Color(180, 180, 180);
		Append(_loreText);
	}

	public void SetHeart(ElementalHeartItem heart)
	{
		if (_heart == heart) return;
		_heart = heart;
		
		_titleText.SetText(heart.Item.Name);
		_titleText.TextColor = heart.Tier.GetEffectColor();
		
		_tierText.SetText($"{heart.Tier} Tier");
		_tierText.TextColor = heart.Tier.GetEffectColor() * 0.8f;
		BorderColor = heart.Tier.GetEffectColor();

		string stats;
		if (heart.HpGain > 0)
		{
			stats = $"Grants {heart.HpGain} Elemental HP";
		}
		else
		{
			string prefix = heart.IsActiveAbility ? "Grants an active ability:\n" : "Grants a passive ability:\n";
			string effect = "";
			
			if (heart is PotionHeartItem potionHeart)
			{
				effect = potionHeart.PermanentEffectText;
			}
			else
			{
				string tooltipKey = $"Mods.ElementalHearts.Items.{heart.Name}.Tooltip";
				if (Language.Exists(tooltipKey))
				{
					effect = Language.GetTextValue(tooltipKey);
				}
			}

			if (!string.IsNullOrWhiteSpace(effect))
			{
				// Strip "Permanently " so it reads cleanly like: "Grants an active ability:\nbounce on enemies."
				effect = effect.Replace("Permanently ", "");
				if (effect.Length > 0 && char.IsLower(effect[0]))
					effect = char.ToUpper(effect[0]) + effect.Substring(1);

				stats = prefix + effect;
			}
			else
			{
				stats = heart.IsActiveAbility ? "Grants an active ability" : "Grants a passive ability";
			}
		}
		_statsText.SetText(stats);

		// Dynamically adjust width to fit long ability descriptions
		float textWidth = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(stats).X * 0.9f;
		float newWidth = System.Math.Max(350f, textWidth + 40f); // 40f provides breathing room for the 15f padding
		Width.Set(newWidth, 0f);

		float extraSpace = stats.Contains('\n') ? 22f : 0f;
		_generationText.Top.Set(82f + extraSpace, 0f);
		_sep2.Top.Set(110f + extraSpace, 0f);
		_loreText.Top.Set(125f + extraSpace, 0f);

		if (heart is PotionHeartItem || heart.IsActiveAbility)
		{
			int cost = heart.ActiveAbilityDailyCost > 0 ? heart.ActiveAbilityDailyCost : heart.Tier.GetShardYield();
			_generationText.SetText($"-{cost} [i:{ModContent.ItemType<CommonLifeShard>()}] / day");
			_generationText.TextColor = new Color(255, 150, 150);
		}
		else
		{
			int yield = heart.Tier.GetShardYield();
			_generationText.SetText($"+{yield} [i:{ModContent.ItemType<CommonLifeShard>()}] / day");
			_generationText.TextColor = new Color(150, 255, 150);
		}

		string loreKey = $"Mods.ElementalHearts.Items.{heart.Name}.Lore";
		string lore = Language.Exists(loreKey) ? Language.GetTextValue(loreKey) : "A mysterious heart radiating strange elemental energy...";
		_loreText.SetText(lore);
		
		// Wait one frame to calculate height based on wrapped lore text
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
		
		float neededHeight = _loreText.Top.Pixels + _loreText.MinHeight.Pixels + 20f;
		if (Height.Pixels != neededHeight)
		{
			Height.Set(neededHeight, 0f);
		}

		// Follow mouse but place above the cursor so it doesn't overlap native tooltip
		Left.Set(Main.mouseX + 20, 0f);
		Top.Set(Main.mouseY - neededHeight - 10, 0f);

		// Prevent going off-screen
		if (Left.Pixels + Width.Pixels > Main.screenWidth)
			Left.Set(Main.screenWidth - Width.Pixels - 10, 0f);
		
		if (Top.Pixels < 0)
			Top.Set(10, 0f);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		try
		{
			base.Draw(spriteBatch);
		}
		catch (System.Exception e)
		{
			Main.NewTextMultiline(e.ToString(), c: Color.Red);
		}
	}
}
