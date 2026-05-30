using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.ModLoader;
using ElementalHearts.Content.Items.Hearts;
using ElementalHearts.Common.Configs;
using ElementalHearts.Common.Systems;
using ElementalHearts.Common.Players;
using ElementalHearts.Common.Hearts;

namespace ElementalHearts.Common.UI.Checklist;

public class ChecklistUIState : UIState
{
	public enum SortMode { Tier, Alphabetical, Mod }
	public enum FilterMode { All, Unlocked, Locked, Potions, Calamity, Thorium, Consolaria, Vanilla, Zenith }

	private SortMode _sortMode = SortMode.Alphabetical;
	private FilterMode _filterMode = FilterMode.All;

	private UIPanel _mainPanel;
	private UIList _heartList;
	private UIScrollbar _scrollbar;
	private UIText _adminText;
	private UIPanel _adminButtonsContainer;

	private UITextPanel<string> _sortButton;
	private UITextPanel<string> _filterButton;

	public override void OnInitialize()
	{
		_mainPanel = new UIPanel();
		_mainPanel.Width.Set(1080, 0f); // Reduced by 20%
		_mainPanel.Height.Set(780, 0f); // Increased by 30%
		_mainPanel.HAlign = 0.5f;
		_mainPanel.VAlign = 0.5f;
		_mainPanel.BackgroundColor = new Color(20, 26, 48) * 0.95f;
		_mainPanel.BorderColor = new Color(89, 116, 213);
		Append(_mainPanel);

		UIText title = new UIText("Heart Checklist", 1.2f);
		title.HAlign = 0.5f;
		title.Top.Set(5, 0f);
		title.TextColor = new Color(255, 215, 0);
		_mainPanel.Append(title);

		UIText closeButton = new UIText("X", 1.2f);
		closeButton.HAlign = 1f;
		closeButton.Top.Set(5, 0f);
		closeButton.OnLeftClick += (evt, element) => ChecklistUISystem.ToggleUI();
		closeButton.OnMouseOver += (evt, element) => closeButton.TextColor = Color.Red;
		closeButton.OnMouseOut += (evt, element) => closeButton.TextColor = Color.White;
		_mainPanel.Append(closeButton);

		_sortButton = new UITextPanel<string>($"Sort: {_sortMode}", 0.8f);
		_sortButton.Width.Set(150, 0f);
		_sortButton.Top.Set(5, 0f);
		_sortButton.Left.Set(10, 0f);
		_sortButton.BackgroundColor = new Color(63, 82, 151) * 0.7f;
		_sortButton.BorderColor = new Color(89, 116, 213);
		_sortButton.OnLeftClick += (evt, element) => {
			Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
			_sortMode = (SortMode)(((int)_sortMode + 1) % 3);
			_sortButton.SetText($"Sort: {_sortMode}");
			Rebuild();
		};
		_mainPanel.Append(_sortButton);

		_filterButton = new UITextPanel<string>($"Filter: {_filterMode}", 0.8f);
		_filterButton.Width.Set(150, 0f);
		_filterButton.Top.Set(5, 0f);
		_filterButton.Left.Set(170, 0f);
		_filterButton.BackgroundColor = new Color(63, 82, 151) * 0.7f;
		_filterButton.BorderColor = new Color(89, 116, 213);
		_filterButton.OnLeftClick += (evt, element) => {
			Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
			int nextMode = (int)_filterMode + 1;
			if (nextMode > (int)FilterMode.Zenith) nextMode = 0;
			_filterMode = (FilterMode)nextMode;
			_filterButton.SetText($"Filter: {_filterMode}");
			Rebuild();
		};
		_mainPanel.Append(_filterButton);

		_adminText = new UIText("", 0.9f);
		_adminText.HAlign = 0.5f;
		_adminText.Top.Set(28, 0f);
		_adminText.TextColor = Color.Red;
		_mainPanel.Append(_adminText);

		UIHorizontalSeparator separator = new UIHorizontalSeparator();
		separator.Width.Set(0, 1f);
		separator.Top.Set(45, 0f);
		separator.Color = new Color(89, 116, 213) * 0.7f;
		_mainPanel.Append(separator);

		_heartList = new UIList();
		_heartList.Width.Set(-25, 1f);
		_heartList.Height.Set(-60, 1f);
		_heartList.Top.Set(60, 0f);
		_heartList.ListPadding = 8f;
		_mainPanel.Append(_heartList);

		_scrollbar = new UIScrollbar();
		_scrollbar.SetView(100f, 1000f);
		_scrollbar.Height.Set(-60, 1f);
		_scrollbar.Top.Set(60, 0f);
		_scrollbar.HAlign = 1f;
		_mainPanel.Append(_scrollbar);
		_heartList.SetScrollbar(_scrollbar);

		_adminButtonsContainer = new UIPanel();
		_adminButtonsContainer.Width.Set(0, 1f);
		_adminButtonsContainer.Height.Set(45, 0f);
		_adminButtonsContainer.VAlign = 1f;
		_adminButtonsContainer.BackgroundColor = Color.Transparent;
		_adminButtonsContainer.BorderColor = Color.Transparent;
		_adminButtonsContainer.SetPadding(0f);

		var btnActivateAll = CreateAdminButton("Unlock All", 0.0f);
		btnActivateAll.OnLeftClick += (evt, element) => {
			Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
			var player = Main.LocalPlayer.GetModPlayer<HeartConsumptionPlayer>();
			bool shared = ElementalHeartsWorldConfig.Instance.SharedProgression;
			foreach (var heart in ModContent.GetContent<ElementalHeartItem>()) {
				if (shared) HeartConsumptionWorld.TryConsume(heart);
				else player.TryConsumeLocally(heart);
			}
			Rebuild();
		};
		_adminButtonsContainer.Append(btnActivateAll);

		var btnClearAll = CreateAdminButton("Clear All", 0.33f);
		btnClearAll.OnLeftClick += (evt, element) => {
			Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
			HeartConsumptionWorld.ClearAllHearts();
			Main.LocalPlayer.GetModPlayer<HeartConsumptionPlayer>().ClearWorldHp();
			Rebuild();
		};
		_adminButtonsContainer.Append(btnClearAll);

		var btnClearTier = CreateAdminButton("Clear Tier", 0.66f);
		btnClearTier.OnLeftClick += (evt, element) => {
			Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
			AnimateProgressionSystem.ClearTier();
			Rebuild();
		};
		_adminButtonsContainer.Append(btnClearTier);

		var btnAdvanceTier = CreateAdminButton("Adv Tier", 1.0f);
		btnAdvanceTier.OnLeftClick += (evt, element) => {
			Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
			AnimateProgressionSystem.AdvanceTier();
			Rebuild();
		};
		_adminButtonsContainer.Append(btnAdvanceTier);
	}

	private UITextPanel<string> CreateAdminButton(string text, float hAlign)
	{
		var btn = new UITextPanel<string>(text, 0.7f);
		btn.Width.Set(0, 0.23f);
		btn.Height.Set(0, 1f);
		btn.HAlign = hAlign;
		btn.VAlign = 0.5f;
		btn.BackgroundColor = new Color(63, 82, 151) * 0.7f;
		btn.BorderColor = new Color(89, 116, 213);
		btn.OnMouseOver += (evt, element) => {
			Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
			btn.BackgroundColor = new Color(73, 94, 171);
		};
		btn.OnMouseOut += (evt, element) => {
			btn.BackgroundColor = new Color(63, 82, 151) * 0.7f;
		};
		return btn;
	}

	public void Rebuild()
	{
		if (ElementalHeartsWorldConfig.Instance.AdminMode)
		{
			_mainPanel.BorderColor = new Color(255, 215, 0); // Gold border
			_adminText.SetText("- ADMIN MODE ACTIVE -");
			_mainPanel.Append(_adminButtonsContainer);
			_heartList.Height.Set(-110, 1f);
			_scrollbar.Height.Set(-110, 1f);
		}
		else
		{
			_mainPanel.BorderColor = new Color(89, 116, 213);
			_adminText.SetText("");
			if (_adminButtonsContainer.Parent != null)
				_adminButtonsContainer.Remove();
			_heartList.Height.Set(-60, 1f);
			_scrollbar.Height.Set(-60, 1f);
		}

		_heartList.Clear();

		var allHearts = ModContent.GetContent<ElementalHeartItem>().ToList();
		bool shared = ElementalHeartsWorldConfig.Instance.SharedProgression;
		var player = Main.LocalPlayer.GetModPlayer<HeartConsumptionPlayer>();

		var unlockedTiers = new System.Collections.Generic.HashSet<HeartTier>();
		foreach (var heart in allHearts)
		{
			bool isUnlocked = shared ? HeartConsumptionWorld.IsUnlocked(heart.ConsumptionId) : player.IsUnlockedLocally(heart.ConsumptionId);
			if (isUnlocked)
				unlockedTiers.Add(heart.Tier);
		}

		// Filtering
		var filteredHearts = allHearts.Where(heart => {
			bool isUnlocked = shared ? HeartConsumptionWorld.IsUnlocked(heart.ConsumptionId) : player.IsUnlockedLocally(heart.ConsumptionId);
			if (_filterMode == FilterMode.Unlocked && !isUnlocked) return false;
			if (_filterMode == FilterMode.Locked && isUnlocked) return false;
			if (_filterMode == FilterMode.Potions && !(heart is PotionHeartItem)) return false;
			
			if (_filterMode == FilterMode.Calamity && heart.SourceMod != "CalamityMod") return false;
			if (_filterMode == FilterMode.Thorium && heart.SourceMod != "ThoriumMod") return false;
			if (_filterMode == FilterMode.Consolaria && heart.SourceMod != "Consolaria") return false;
			if (_filterMode == FilterMode.Vanilla && heart.SourceMod != null) return false;
			if (_filterMode == FilterMode.Zenith)
			{
				if (heart is Content.Items.Hearts.BossHeartItem or Content.Items.Hearts.PotionHeartItem or Content.Items.Hearts.CrossMod.CrossModHeartItem or Content.Items.Hearts.Vanilla.Mythic.ZenithHeart)
					return false;
			}
			return true;
		}).ToList();

		// Sorting
		if (_sortMode == SortMode.Alphabetical)
		{
			filteredHearts = filteredHearts.OrderBy(h => h.Item.Name).ToList();
		}
		else if (_sortMode == SortMode.Mod)
		{
			filteredHearts = filteredHearts.OrderBy(h => {
				if (h.SourceMod == "CalamityMod") return 0;
				if (h.SourceMod == "ThoriumMod") return 1;
				if (h.SourceMod == "Consolaria") return 2;
				if (h.SourceMod == null) return 3; // Vanilla
				return 4;
			}).ThenBy(h => h.Tier).ThenBy(h => h.Item.Name).ToList();
		}
		else
		{
			// SortMode.Tier
			filteredHearts = filteredHearts.OrderBy(h => (int)h.Tier).ThenBy(h => h.Item.Name).ToList();
		}

		List<object> layoutItems = new List<object>();

		if (_sortMode == SortMode.Mod)
		{
			var groups = filteredHearts.GroupBy(h => h.SourceMod).ToList();
			foreach (var group in groups)
			{
				string modName = group.Key == null ? "Vanilla" : group.Key.Replace("Mod", "");
				layoutItems.Add($"- {modName} -");
				
				var hearts = group.ToList();
				for (int i = 0; i < hearts.Count; i += 3)
				{
					layoutItems.Add(hearts.GetRange(i, System.Math.Min(3, hearts.Count - i)));
				}
			}
		}
		else
		{
			for (int i = 0; i < filteredHearts.Count; i += 3)
			{
				layoutItems.Add(filteredHearts.GetRange(i, System.Math.Min(3, filteredHearts.Count - i)));
			}
		}

		foreach (var item in layoutItems)
		{
			if (item is string headerText)
			{
				UIText header = new UIText(headerText, 1.1f, true);
				header.HAlign = 0.5f;
				header.MarginTop = 15f;
				header.MarginBottom = 10f;
				header.TextColor = Color.Gold;
				_heartList.Add(header);
			}
			else if (item is List<ElementalHeartItem> rowHearts)
			{
				UIElement rowContainer = new UIElement();
				rowContainer.Width.Set(0, 1f);
				rowContainer.Height.Set(50, 0f);

				for (int j = 0; j < rowHearts.Count; j++)
				{
					var heart = rowHearts[j];
				
				bool isUnlocked = shared 
					? HeartConsumptionWorld.IsUnlocked(heart.ConsumptionId)
					: player.IsUnlockedLocally(heart.ConsumptionId);
				bool isConsumed = shared 
					? HeartConsumptionWorld.IsConsumed(heart.ConsumptionId)
					: player.IsConsumedLocally(heart.ConsumptionId);
				bool isTierUnlocked = unlockedTiers.Contains(heart.Tier);
				bool isBossHeart = heart is BossHeartItem;

				UIPanel heartRow = new UIPanel();
				heartRow.Width.Set(-10, 0.33f); // 33% width minus gap
				heartRow.Height.Set(50, 0f);
				heartRow.HAlign = j == 0 ? 0f : (j == 1 ? 0.5f : 1f);
				heartRow.SetPadding(5f);
				
				if (isUnlocked)
				{
					heartRow.BackgroundColor = heart.Tier.GetEffectColor() * (isConsumed ? 0.4f : 0.2f);
					heartRow.BorderColor = heart.Tier.GetEffectColor() * (isConsumed ? 1f : 0.5f);

					if (heart is PotionHeartItem potionHeart)
					{
						UITextPanel<string> toggleBtn = new UITextPanel<string>(isConsumed ? "Disable" : "Enable", 0.6f);
						toggleBtn.Width.Set(55, 0f);
						toggleBtn.Height.Set(24, 0f);
						toggleBtn.VAlign = 0.5f;
						toggleBtn.HAlign = 1f; // right align
						toggleBtn.Left.Set(-10, 0f); // pad from right
						toggleBtn.BackgroundColor = new Color(63, 82, 151) * 0.7f;
						toggleBtn.BorderColor = new Color(89, 116, 213);
						
						toggleBtn.OnMouseOver += (evt, element) => {
							Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
							toggleBtn.BackgroundColor = new Color(73, 94, 171);
						};
						toggleBtn.OnMouseOut += (evt, element) => {
							toggleBtn.BackgroundColor = new Color(63, 82, 151) * 0.7f;
						};
						toggleBtn.OnLeftClick += (evt, element) => {
							Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item4, Main.LocalPlayer.Center);
							if (isConsumed)
							{
								if (shared)
									HeartConsumptionWorld.TryDeactivate(potionHeart);
								else
									player.TryDeactivateLocally(potionHeart);
							}
							else
							{
								if (shared)
									HeartConsumptionWorld.TryConsume(potionHeart);
								else
									player.TryConsumeLocally(potionHeart);
							}
							Rebuild();
						};
						heartRow.Append(toggleBtn);
					}
				}
				else
				{
					heartRow.BackgroundColor = new Color(10, 10, 10) * 0.6f;
					heartRow.BorderColor = new Color(40, 40, 40);

					if (ElementalHeartsWorldConfig.Instance.AdminMode)
					{
						heartRow.OnLeftClick += (evt, element) => {
							Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item4, Main.LocalPlayer.Center);
							if (shared)
								HeartConsumptionWorld.TryConsume(heart);
							else
								player.TryConsumeLocally(heart);
							Rebuild();
						};
						heartRow.OnMouseOver += (evt, element) => heartRow.BackgroundColor = new Color(50, 50, 50) * 0.8f;
						heartRow.OnMouseOut += (evt, element) => heartRow.BackgroundColor = new Color(10, 10, 10) * 0.6f;
					}
				}

				HeartIconElement icon = new HeartIconElement(heart.Item, isUnlocked);
				icon.VAlign = 0.5f;
				icon.Left.Set(10, 0f);
				heartRow.Append(icon);

				UIText nameText = new UIText(isUnlocked || isTierUnlocked || isBossHeart ? heart.Item.Name : "???", isUnlocked ? 1f : 0.9f);
				nameText.VAlign = 0.5f;
				nameText.Left.Set(50, 0f);
				if (!isUnlocked) nameText.TextColor = Color.Gray;
				heartRow.Append(nameText);

				rowContainer.Append(heartRow);
			}

			_heartList.Add(rowContainer);
			}
		}
	}

	private class HeartIconElement : UIElement
	{
		public Item Item;
		public bool IsConsumed;

		public HeartIconElement(Item item, bool isConsumed)
		{
			Item = item;
			IsConsumed = isConsumed;
			Width.Set(32, 0f);
			Height.Set(32, 0f);
		}

		protected override void DrawSelf(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
		{
			CalculatedStyle dimensions = GetDimensions();
			Main.instance.LoadItem(Item.type);
			var texture = Terraria.GameContent.TextureAssets.Item[Item.type].Value;
			Rectangle frame = texture.Bounds;
			var anim = Main.itemAnimations[Item.type];
			if (anim != null) frame = anim.GetFrame(texture);

			float scale = 1f;
			if (frame.Width > 32 || frame.Height > 32)
				scale = 32f / System.Math.Max(frame.Width, frame.Height);

			Vector2 pos = dimensions.Position() + new Vector2(16, 16);
			Color color = IsConsumed ? Color.White : Color.Black;
			spriteBatch.Draw(texture, pos, frame, color, 0f, frame.Size() / 2f, scale, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
		}
	}
}
