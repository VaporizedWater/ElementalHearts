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

public class UIHeartHoverCard : AuroraPanel
{
	private UIList _list;
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
		Width.Set(380, 0f);
		Height.Set(250, 0f); // Will dynamically size
		AuroraColor1 = new Color(15, 20, 38) * 0.95f;
		AuroraColor2 = new Color(10, 15, 30) * 0.95f;
		AuroraColor3 = new Color(20, 25, 45) * 0.95f;
		BorderColor = new Color(89, 116, 213);
		PaddingLeft = 20f;
		PaddingRight = 20f;
		PaddingTop = 10f;
		PaddingBottom = 10f;
		IsInteractive = false;

		_list = new UIList();
		_list.Width.Set(0, 1f);
		_list.Height.Set(0, 1f);
		_list.ListPadding = 4f; // Reduced padding
		Append(_list);

		_titleText = new UIText("Heart Name", 1.1f, false);
		_titleText.HAlign = 0.5f;
		_list.Add(_titleText);

		_tierText = new UIText("Rarity", 0.85f);
		_tierText.HAlign = 0.5f;
		_list.Add(_tierText);

		_sep1 = new UIHorizontalSeparator();
		_sep1.Width.Set(0, 1f);
		_sep1.Color = new Color(89, 116, 213) * 0.7f;
		_list.Add(_sep1);

		_statsText = new UIText("Stats go here", 0.9f);
		_statsText.HAlign = 0.5f;
		_statsText.Width.Set(0, 1f);
		_statsText.IsWrapped = false; // Set to false to prevent tModLoader's layout/wrap height bugs
		_list.Add(_statsText);

		_generationText = new UIText("", 1f);
		_generationText.HAlign = 0.5f;
		_generationText.MarginTop = -4f; // Cancel out ListPadding of 4f for no spacing
		_list.Add(_generationText);

		_sep2 = new UIHorizontalSeparator();
		_sep2.Width.Set(0, 1f);
		_sep2.Color = new Color(89, 116, 213) * 0.7f;
		_list.Add(_sep2);

		_loreText = new UIText("Lore goes here", 0.85f);
		_loreText.Width.Set(0, 1f);
		_loreText.IsWrapped = false; // Set to false to prevent layout bugs
		_loreText.TextColor = new Color(180, 180, 180);
		_loreText.MarginBottom = 0f; // Removed bottom margin
		_list.Add(_loreText);
	}

	private string WrapText(string text, float scale, float maxWidth)
	{
		if (string.IsNullOrWhiteSpace(text)) return "";
		
		var font = Terraria.GameContent.FontAssets.MouseText.Value;
		string[] paragraphs = text.Split('\n');
		System.Collections.Generic.List<string> wrappedLines = new System.Collections.Generic.List<string>();
		
		foreach (string paragraph in paragraphs)
		{
			string[] words = paragraph.Split(' ');
			string currentLine = "";
			
			for (int i = 0; i < words.Length; i++)
			{
				string word = words[i];
				string testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
				float width = font.MeasureString(testLine).X * scale;
				
				if (width > maxWidth && !string.IsNullOrEmpty(currentLine))
				{
					wrappedLines.Add(currentLine);
					currentLine = word;
				}
				else
				{
					currentLine = testLine;
				}
			}
			
			if (!string.IsNullOrEmpty(currentLine))
			{
				wrappedLines.Add(currentLine);
			}
		}
		
		return string.Join("\n", wrappedLines);
	}

	public void SetHeart(ElementalHeartItem heart)
	{
		if (_heart == heart) return;
		_heart = heart;
		
		_titleText.SetText(heart.Item.Name);
		_titleText.TextColor = heart.Tier.GetEffectColor();
		
		_tierText.SetText($"{heart.Tier}");
		_tierText.TextColor = heart.Tier.GetEffectColor() * 0.8f;
		BorderColor = heart.Tier.GetEffectColor();

		HeartEffect consumptionEffect = HeartEffectRegistry.Get(heart.ConsumptionId);
		AuroraColors = consumptionEffect.Colors;
		IsPrismatic = consumptionEffect.Rainbow;
		AuroraBrightness = 0.5f; // Keep it dark enough so the white text remains legible

		string stats;
		if (heart.HpGain > 0)
		{
			stats = $"Grants {heart.HpGain} Elemental HP";
		}
		else
		{
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
				// Strip "Permanently " so it reads cleanly
				effect = effect.Replace("Permanently ", "");
				if (effect.Length > 0 && char.IsLower(effect[0]))
					effect = char.ToUpper(effect[0]) + effect.Substring(1);

				stats = effect;
			}
			else
			{
				stats = "";
			}
		}

		// Wrap stats text and set it
		string wrappedStats = WrapText(stats, 0.9f, 380f - 40f);
		_statsText.SetText(wrappedStats);

		// Measure text heights based on actual lines in the wrapped string
		float CalculateWrappedHeight(string wrappedText, float scale)
		{
			if (string.IsNullOrWhiteSpace(wrappedText)) return 0f;
			int lineCount = wrappedText.Split('\n').Length;
			return lineCount * (Terraria.GameContent.FontAssets.MouseText.Value.LineSpacing * scale);
		}
		
		_statsText.Height.Set(CalculateWrappedHeight(wrappedStats, 0.9f), 0f);

		if (heart is PotionHeartItem || heart.IsActiveAbility)
		{
			int cost = heart.ActiveAbilityDailyCost > 0 ? heart.ActiveAbilityDailyCost : heart.Tier.GetShardYield();
			_generationText.SetText($"-{cost}  [i:{ModContent.ItemType<CommonLifeShard>()}]  / day");
			_generationText.TextColor = new Color(255, 150, 150);
		}
		else
		{
			int yield = heart.Tier.GetShardYield();
			_generationText.SetText($"+{yield}  [i:{ModContent.ItemType<CommonLifeShard>()}]  / day");
			_generationText.TextColor = new Color(150, 255, 150);
		}

		string loreKey = $"Mods.ElementalHearts.Items.{heart.Name}.Lore";
		string lore = Language.Exists(loreKey) ? Language.GetTextValue(loreKey) : "A mysterious heart radiating strange elemental energy...";
		
		// Wrap lore text and set it
		string wrappedLore = WrapText(lore, 0.85f, 380f - 40f);
		_loreText.SetText(wrappedLore);
		_loreText.Height.Set(CalculateWrappedHeight(wrappedLore, 0.85f), 0f);
		
		// Force recalculation so UIList correctly measures all dynamic texts and stacks them without overlapping
		_list.Recalculate();
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
		
		float neededHeight = _list.GetTotalHeight() + PaddingTop + PaddingBottom;
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
