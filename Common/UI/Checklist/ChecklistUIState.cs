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
using ElementalHearts.Content.Items.LifeShards;
using ElementalHearts.Common.UI;

namespace ElementalHearts.Common.UI.Checklist;

public class ChecklistUIState : UIState
{
	public enum TabMode { Active, Passive }
	public enum SortMode { Tier, Alphabetical }
	public enum FilterMode { All, Unlocked, Locked, Potions, Calamity, Thorium, Consolaria, Vanilla, Zenith }

	private TabMode _tabMode = TabMode.Active;
	private SortMode _sortMode = SortMode.Alphabetical;
	private FilterMode _filterMode = FilterMode.All;
	private string _searchQuery = "";
	private bool _searchBarHasInitializedText = false;

	private UIPanel _mainPanel;
	private UIList _heartList;
	private UIScrollbar _scrollbar;
	private UIText _adminText;
	private UIPanel _adminButtonsContainer;

	private UITextPanel<string> _sortButton;
	private UITextPanel<string> _filterButton;
	private UISearchBar _searchBar;
	private UIPanel _searchBarContainer;

	private UITextPanel<string> _activeTabBtn;
	private UITextPanel<string> _passiveTabBtn;
	private UITextPanel<string> _settingsButton;
	private bool _isSettingsMode = false;
	private UIList _settingsList;
	private UIPanel _idlePanel;
	private UIText _generationText;
	private UIText _consumptionText;
	private UIText _profitText;
	private UIText _bankText;
	private UIText _limitText;
	private UIPanel _statsPanel;
	private UIText _elementalHpText;
	private UIText _worldTierText;
	private UIText _heartsActivatedText;
	private UITextPanel<string> _claimBtn;

	public override void OnInitialize()
	{
		_mainPanel = new UIPanel();
		_mainPanel.Width.Set(1300, 0f); // Restored width (+20%)
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

		_searchBarContainer = new UIPanel();
		_searchBarContainer.Width.Set(200, 0f);
		_searchBarContainer.Height.Set(35, 0f);
		_searchBarContainer.HAlign = 1f;
		_searchBarContainer.Top.Set(5, 0f);
		_searchBarContainer.Left.Set(-40, 0f);
		_searchBarContainer.BackgroundColor = new Color(63, 82, 151) * 0.7f;
		_searchBarContainer.BorderColor = new Color(89, 116, 213);
		_searchBarContainer.SetPadding(0);
		_mainPanel.Append(_searchBarContainer);

		_searchBar = new UISearchBar(Terraria.Localization.Language.GetText(""), 0.8f);
		_searchBar.Width.Set(-40, 1f);
		_searchBar.Height.Set(0, 1f);
		_searchBar.HAlign = 1f;
		_searchBar.VAlign = 0.5f;
		_searchBar.Left.Set(-10, 0f);
		_searchBar.Top.Set(0, 0f);
		
		UIImage searchIcon = new UIImage(Main.Assets.Request<Microsoft.Xna.Framework.Graphics.Texture2D>("Images/UI/Bestiary/Button_Search"));
		searchIcon.VAlign = 0.5f;
		searchIcon.Left.Set(8, 0f);
		searchIcon.ImageScale = 0.8f;
		_searchBarContainer.Append(searchIcon);
		_searchBar.OnContentsChanged += (contents) => {
			_searchQuery = contents;
			Rebuild();
		};
		_searchBar.OnUpdate += (element) => {
			if (!_searchBarHasInitializedText && _searchBar.GetDimensions().Width > 0)
			{
				_searchBarHasInitializedText = true;
				_searchBar.SetContents("");
			}
		};
		_searchBar.OnLeftClick += (evt, element) => {
			if (!_searchBar.IsWritingText)
				_searchBar.ToggleTakingText();
		};
		_searchBar.OnRightClick += (evt, element) => {
			_searchBar.SetContents("");
		};
		_searchBarContainer.OnLeftClick += (evt, element) => {
			if (!_searchBar.IsWritingText)
				_searchBar.ToggleTakingText();
		};
		_searchBarContainer.OnRightClick += (evt, element) => {
			_searchBar.SetContents("");
		};
		_searchBarContainer.Append(_searchBar);

		_activeTabBtn = new UITextPanel<string>("Active Buffs", 0.9f);
		_activeTabBtn.Width.Set(150, 0f);
		_activeTabBtn.Top.Set(5, 0f);
		_activeTabBtn.Left.Set(10, 0f);
		_activeTabBtn.BackgroundColor = new Color(73, 94, 171); // Active by default
		_activeTabBtn.BorderColor = new Color(89, 116, 213);
		_activeTabBtn.OnLeftClick += (evt, element) => {
			Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
			_tabMode = TabMode.Active;
			Rebuild();
		};
		_mainPanel.Append(_activeTabBtn);

		_passiveTabBtn = new UITextPanel<string>("Passive Collection", 0.9f);
		_passiveTabBtn.Width.Set(200, 0f);
		_passiveTabBtn.Top.Set(5, 0f);
		_passiveTabBtn.Left.Set(170, 0f);
		_passiveTabBtn.BackgroundColor = new Color(63, 82, 151) * 0.7f;
		_passiveTabBtn.BorderColor = new Color(89, 116, 213);
		_passiveTabBtn.OnLeftClick += (evt, element) => {
			Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
			_tabMode = TabMode.Passive;
			Rebuild();
		};
		_mainPanel.Append(_passiveTabBtn);

		_settingsButton = new UITextPanel<string>("⚙ Settings", 0.8f);
		_settingsButton.Width.Set(120, 0f);
		_settingsButton.Top.Set(5, 0f);
		_settingsButton.Left.Set(380, 0f);
		_settingsButton.BackgroundColor = new Color(63, 82, 151) * 0.7f;
		_settingsButton.BorderColor = new Color(89, 116, 213);
		_settingsButton.OnLeftClick += (evt, element) => {
			Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
			_isSettingsMode = !_isSettingsMode;
			_settingsButton.BackgroundColor = _isSettingsMode ? new Color(73, 94, 171) : new Color(63, 82, 151) * 0.7f;
			Rebuild();
		};
		_mainPanel.Append(_settingsButton);

		_sortButton = new UITextPanel<string>($"Sort: {_sortMode}", 0.8f);
		_sortButton.Width.Set(120, 0f);
		_sortButton.Top.Set(5, 0f);
		_sortButton.HAlign = 1f;
		_sortButton.Left.Set(-390, 0f);
		_sortButton.BackgroundColor = new Color(63, 82, 151) * 0.7f;
		_sortButton.BorderColor = new Color(89, 116, 213);
		_sortButton.OnLeftClick += (evt, element) => {
			Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
			_sortMode = (SortMode)(((int)_sortMode + 1) % 2);
			_sortButton.SetText($"Sort: {_sortMode}");
			Rebuild();
		};
		_mainPanel.Append(_sortButton);

		_filterButton = new UITextPanel<string>($"Filter: {_filterMode}", 0.8f);
		_filterButton.Width.Set(120, 0f);
		_filterButton.Top.Set(5, 0f);
		_filterButton.HAlign = 1f;
		_filterButton.Left.Set(-260, 0f);
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

		_statsPanel = new UIPanel();
		_statsPanel.Width.Set(0, 1f);
		_statsPanel.Height.Set(40, 0f);
		_statsPanel.Top.Set(55, 0f);
		_statsPanel.BackgroundColor = new Color(30, 38, 70) * 0.8f;
		_statsPanel.BorderColor = new Color(89, 116, 213);
		
		_elementalHpText = new UIText("", 0.9f);
		_elementalHpText.VAlign = 0.5f;
		_elementalHpText.Left.Set(20, 0f);
		_statsPanel.Append(_elementalHpText);

		_worldTierText = new UIText("", 0.9f);
		_worldTierText.VAlign = 0.5f;
		_worldTierText.HAlign = 0.5f;
		_statsPanel.Append(_worldTierText);

		_heartsActivatedText = new UIText("", 0.9f);
		_heartsActivatedText.VAlign = 0.5f;
		_heartsActivatedText.HAlign = 1f;
		_heartsActivatedText.Left.Set(-20, 0f);
		_statsPanel.Append(_heartsActivatedText);

		_heartList = new UIList();
		_heartList.Width.Set(-25, 1f);
		_heartList.Height.Set(-135, 1f);
		_heartList.Top.Set(60, 0f);
		_heartList.ListPadding = 8f;
		_mainPanel.Append(_heartList);

		_settingsList = new UIList();
		_settingsList.Width.Set(-25, 1f);
		_settingsList.Height.Set(-135, 1f);
		_settingsList.Top.Set(60, 0f);
		_settingsList.ListPadding = 12f;

		_scrollbar = new UIScrollbar();
		_scrollbar.SetView(100f, 1000f);
		_scrollbar.Height.Set(-135, 1f);
		_scrollbar.Top.Set(60, 0f);
		_scrollbar.HAlign = 1f;
		_mainPanel.Append(_scrollbar);
		_heartList.SetScrollbar(_scrollbar);
		_settingsList.SetScrollbar(_scrollbar);

		_idlePanel = new UIPanel();
		_idlePanel.Width.Set(0, 1f);
		_idlePanel.Height.Set(85, 0f);
		_idlePanel.VAlign = 1f;
		_idlePanel.BackgroundColor = new Color(30, 38, 70) * 0.8f;
		_idlePanel.BorderColor = new Color(89, 116, 213);
		_mainPanel.Append(_idlePanel);

		UIPanel leftSection = new UIPanel();
		leftSection.Width.Set(0, 0.45f);
		leftSection.Height.Set(0, 1f);
		leftSection.HAlign = 0f;
		leftSection.VAlign = 0.5f;
		leftSection.BackgroundColor = Color.Transparent;
		leftSection.BorderColor = Color.Transparent;
		leftSection.SetPadding(0);
		_idlePanel.Append(leftSection);

		_generationText = new UIText("Generation: 0 / day", 0.85f);
		_generationText.Top.Set(5, 0f);
		_generationText.Left.Set(10, 0f);
		_generationText.TextColor = new Color(150, 255, 150);
		leftSection.Append(_generationText);

		_consumptionText = new UIText("Consumption: 0 / day", 0.85f);
		_consumptionText.Top.Set(25, 0f);
		_consumptionText.Left.Set(10, 0f);
		_consumptionText.TextColor = new Color(255, 150, 150);
		leftSection.Append(_consumptionText);

		_profitText = new UIText("Profit: 0 / day", 0.85f);
		_profitText.Top.Set(45, 0f);
		_profitText.Left.Set(10, 0f);
		_profitText.TextColor = new Color(200, 200, 200);
		leftSection.Append(_profitText);

		UIPanel rightSection = new UIPanel();
		rightSection.Width.Set(0, 0.45f);
		rightSection.Height.Set(0, 1f);
		rightSection.HAlign = 1f;
		rightSection.VAlign = 0.5f;
		rightSection.BackgroundColor = Color.Transparent;
		rightSection.BorderColor = Color.Transparent;
		rightSection.SetPadding(0);
		_idlePanel.Append(rightSection);

		_bankText = new UIText("Bank: 0", 1.1f);
		_bankText.Top.Set(10, 0f);
		_bankText.Left.Set(20, 0f);
		_bankText.TextColor = new Color(255, 215, 0);
		rightSection.Append(_bankText);

		_limitText = new UIText("Limit: 0", 0.9f);
		_limitText.Top.Set(35, 0f);
		_limitText.Left.Set(20, 0f);
		_limitText.TextColor = new Color(180, 180, 180);
		rightSection.Append(_limitText);

		_claimBtn = new UITextPanel<string>("Claim", 0.9f);
		_claimBtn.Width.Set(100, 0f);
		_claimBtn.Height.Set(40, 0f);
		_claimBtn.VAlign = 0.5f;
		_claimBtn.HAlign = 1f;
		_claimBtn.Left.Set(-15, 0f);
		_claimBtn.BackgroundColor = new Color(63, 82, 151) * 0.7f;
		_claimBtn.BorderColor = new Color(89, 116, 213);
		_claimBtn.OnMouseOver += (evt, element) => {
			var idlePlayer = Main.LocalPlayer.GetModPlayer<IdleShardPlayer>();
			if (idlePlayer.GetPendingShards() < idlePlayer.GetCapacity() || idlePlayer.GetCapacity() == 0)
				_claimBtn.BackgroundColor = new Color(73, 94, 171);
		};
		_claimBtn.OnMouseOut += (evt, element) => {
			var idlePlayer = Main.LocalPlayer.GetModPlayer<IdleShardPlayer>();
			if (idlePlayer.GetPendingShards() < idlePlayer.GetCapacity() || idlePlayer.GetCapacity() == 0)
				_claimBtn.BackgroundColor = new Color(63, 82, 151) * 0.7f;
		};
		_claimBtn.OnLeftClick += (evt, element) => {
			var idlePlayer = Main.LocalPlayer.GetModPlayer<IdleShardPlayer>();
			if (idlePlayer.GetPendingShards() > 0) {
				Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item37);
				idlePlayer.ClaimShards();
				UpdateIdleText();
			}
		};
		rightSection.Append(_claimBtn);

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
		bool showStats = ElementalHeartsClientConfig.Instance.ShowDetailedHeartStats;
		if (showStats)
		{
			if (_statsPanel.Parent == null) _mainPanel.Append(_statsPanel);
		}
		else
		{
			if (_statsPanel.Parent != null) _mainPanel.RemoveChild(_statsPanel);
		}
		
		float listTopOffset = showStats ? 105f : 60f;
		float baseHeightOffset = showStats ? -180f : -135f;

		if (ElementalHeartsWorldConfig.Instance.AdminMode)
		{
			_mainPanel.BorderColor = new Color(255, 215, 0); // Gold border
			_adminText.SetText("- ADMIN MODE ACTIVE -");
			_mainPanel.Append(_adminButtonsContainer);
			_idlePanel.Top.Set(-50, 0f);
			
			_heartList.Height.Set(baseHeightOffset - 50f, 1f);
			_settingsList.Height.Set(baseHeightOffset - 50f, 1f);
			_scrollbar.Height.Set(baseHeightOffset - 50f, 1f);
		}
		else
		{
			_mainPanel.BorderColor = new Color(89, 116, 213);
			_adminText.SetText("");
			if (_adminButtonsContainer.Parent != null)
				_adminButtonsContainer.Remove();
			_idlePanel.Top.Set(0, 0f);
			
			_heartList.Height.Set(baseHeightOffset, 1f);
			_settingsList.Height.Set(baseHeightOffset, 1f);
			_scrollbar.Height.Set(baseHeightOffset, 1f);
		}

		_heartList.Top.Set(listTopOffset, 0f);
		_settingsList.Top.Set(listTopOffset, 0f);
		_scrollbar.Top.Set(listTopOffset, 0f);

		UpdateIdleText();
		UpdateDetailedStatsText();

		_activeTabBtn.BackgroundColor = _tabMode == TabMode.Active ? new Color(73, 94, 171) : new Color(63, 82, 151) * 0.7f;
		_passiveTabBtn.BackgroundColor = _tabMode == TabMode.Passive ? new Color(73, 94, 171) : new Color(63, 82, 151) * 0.7f;

		if (_isSettingsMode)
		{
			if (_heartList.Parent != null) _mainPanel.RemoveChild(_heartList);
			if (_settingsList.Parent == null) _mainPanel.Append(_settingsList);
			if (_sortButton.Parent != null) _mainPanel.RemoveChild(_sortButton);
			if (_filterButton.Parent != null) _mainPanel.RemoveChild(_filterButton);
			if (_searchBarContainer.Parent != null) _mainPanel.RemoveChild(_searchBarContainer);
			if (_activeTabBtn.Parent != null) _mainPanel.RemoveChild(_activeTabBtn);
			if (_passiveTabBtn.Parent != null) _mainPanel.RemoveChild(_passiveTabBtn);
			
			RebuildSettingsList();
			return;
		}
		else
		{
			if (_settingsList.Parent != null) _mainPanel.RemoveChild(_settingsList);
			if (_heartList.Parent == null) _mainPanel.Append(_heartList);
			if (_sortButton.Parent == null) _mainPanel.Append(_sortButton);
			if (_filterButton.Parent == null) _mainPanel.Append(_filterButton);
			if (_searchBarContainer.Parent == null) _mainPanel.Append(_searchBarContainer);
			if (_activeTabBtn.Parent == null) _mainPanel.Append(_activeTabBtn);
			if (_passiveTabBtn.Parent == null) _mainPanel.Append(_passiveTabBtn);
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
			bool hasToggle = heart is PotionHeartItem || heart.IsActiveAbility;
			if (_tabMode == TabMode.Active && !hasToggle) return false;
			if (_tabMode == TabMode.Passive && hasToggle) return false;

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
				if (heart is Content.Items.Hearts.BossHeartItem or Content.Items.Hearts.PotionHeartItem or Content.Items.CrossModHearts.CrossModHeartItem or Content.Items.Vanilla.Mythic.ZenithHeart)
					return false;
			}
			
			if (ElementalHeartsClientConfig.Instance.HideImpossibleHearts && !isUnlocked)
			{
				if (!IsHeartPossible(heart)) return false;
			}
			
			if (!string.IsNullOrWhiteSpace(_searchQuery))
			{
				if (!heart.Item.Name.ToLower().Contains(_searchQuery.ToLower()))
					return false;
			}
			
			return true;
		}).ToList();

		// Sorting
		if (_sortMode == SortMode.Alphabetical)
		{
			filteredHearts = filteredHearts.OrderBy(h => h.Item.Name).ToList();
		}
		else
		{
			// SortMode.Tier
			filteredHearts = filteredHearts.OrderBy(h => (int)h.Tier).ThenBy(h => h.Item.Name).ToList();
		}

		if (_tabMode == TabMode.Passive)
		{
			int passiveUnlocked = 0;
			int passiveTotal = 0;
			foreach (var h in allHearts) {
				bool isToggle = h is PotionHeartItem || h.IsActiveAbility;
				if (!isToggle) {
					bool isUnl = shared ? HeartConsumptionWorld.IsUnlocked(h.ConsumptionId) : player.IsUnlockedLocally(h.ConsumptionId);
					if (isUnl) passiveUnlocked++;
					if (isUnl || IsHeartPossible(h)) passiveTotal++;
				}
			}
			UIText trackerText = new UIText($"Passive Hearts Unlocked: {passiveUnlocked} / {passiveTotal}", 1.1f, true);
			trackerText.HAlign = 0.5f;
			trackerText.MarginTop = 15f;
			trackerText.MarginBottom = 15f;
			trackerText.TextColor = new Color(200, 200, 200);
			_heartList.Add(trackerText);
		}

		List<object> layoutItems = new List<object>();
		int itemsPerRow = _tabMode == TabMode.Active ? 3 : 20;

		if (_sortMode == SortMode.Tier)
		{
			var groups = filteredHearts.GroupBy(h => h.Tier).ToList();
			foreach (var group in groups)
			{
				string tierName = group.Key.ToString();
				layoutItems.Add($"- {tierName} -");
				
				var hearts = group.ToList();
				for (int i = 0; i < hearts.Count; i += itemsPerRow)
				{
					layoutItems.Add(hearts.GetRange(i, System.Math.Min(itemsPerRow, hearts.Count - i)));
				}
			}
		}
		else
		{
			for (int i = 0; i < filteredHearts.Count; i += itemsPerRow)
			{
				layoutItems.Add(filteredHearts.GetRange(i, System.Math.Min(itemsPerRow, filteredHearts.Count - i)));
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

				if (_tabMode == TabMode.Active)
				{
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
								UIToggleSwitch toggleBtn = new UIToggleSwitch(isConsumed);
								toggleBtn.VAlign = 0.5f;
								toggleBtn.HAlign = 1f;
								toggleBtn.Left.Set(-15, 0f);
								
								toggleBtn.OnStateChanged += (newState) => {
									if (newState)
									{
										var idlePlayer = Main.LocalPlayer.GetModPlayer<IdleShardPlayer>();
										idlePlayer.GetShardRates(out int gen, out int cons, out _);
										int cost = IdleShardPlayer.GetShardYield(heart.Tier);
										if (gen < cons + cost)
										{
											Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
											Main.NewText("Not enough shard generation to activate this heart.", Color.Red);
											Rebuild(); // Rebuild will reset the toggle to its correct actual state
											return;
										}
									}

									Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item4, Main.LocalPlayer.Center);
									if (!newState)
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
							else if (heart.IsActiveAbility)
							{
								UIToggleSwitch toggleBtn = new UIToggleSwitch(heart.IsAbilityEnabled);
								toggleBtn.VAlign = 0.5f;
								toggleBtn.HAlign = 1f;
								toggleBtn.Left.Set(-15, 0f);

								toggleBtn.OnStateChanged += (newState) => {
									if (newState)
									{
										var idlePlayer = Main.LocalPlayer.GetModPlayer<IdleShardPlayer>();
										idlePlayer.GetShardRates(out int gen, out int cons, out _);
										int cost = IdleShardPlayer.GetShardYield(heart.Tier);
										if (gen < cons + cost)
										{
											Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
											Main.NewText("Not enough shard generation to activate this heart.", Color.Red);
											Rebuild(); // Rebuild will reset the toggle to its correct actual state
											return;
										}
									}

									Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item4, Main.LocalPlayer.Center);
									heart.SetAbilityEnabled(newState);
									Rebuild();
								};
								heartRow.Append(toggleBtn);
							}
							else
							{
								UIText passiveText = new UIText("∞ Passive", 0.8f);
								passiveText.HAlign = 1f;
								passiveText.Left.Set(-15, 0f);
								passiveText.TextColor = new Color(150, 150, 150);
								passiveText.VAlign = 0.5f;
								heartRow.Append(passiveText);
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

						string displayName = isUnlocked || isTierUnlocked || isBossHeart ? heart.Item.Name : "???";
						UIText nameText = new UIText(displayName, isUnlocked ? 1f : 0.9f);
						nameText.VAlign = 0.5f;
						nameText.Left.Set(50, 0f);
						if (!isUnlocked) nameText.TextColor = Color.Gray;
						heartRow.Append(nameText);

						if (ElementalHeartsIdleConfig.Instance.EnableIdleGame && isUnlocked)
						{
							int rate = heart.ActiveAbilityDailyCost ?? IdleShardPlayer.GetShardYield(heart.Tier);
							bool isAbility = heart is PotionHeartItem || heart.IsActiveAbility;
							string prefix = isAbility ? "Cost: " : "";
							UIText rateText = new UIText($"{prefix}{rate}[i:{ModContent.ItemType<CommonLifeShard>()}]/day", 0.9f);
							rateText.VAlign = 0.5f;
							rateText.HAlign = 1f;
							rateText.Left.Set(-110, 0f); // Rigidly aligned for all
							heartRow.Append(rateText);
						}

						rowContainer.Append(heartRow);
					}
				}
				else
				{
					rowContainer.Height.Set(56, 0f);
					for (int j = 0; j < rowHearts.Count; j++)
					{
						var heart = rowHearts[j];
						bool isUnlocked = shared ? HeartConsumptionWorld.IsUnlocked(heart.ConsumptionId) : player.IsUnlockedLocally(heart.ConsumptionId);
						bool isTierUnlocked = unlockedTiers.Contains(heart.Tier);
						bool isBossHeart = heart is BossHeartItem;

						UIPanel gridSlot = new UIPanel();
						gridSlot.Width.Set(48, 0f);
						gridSlot.Height.Set(48, 0f);
						gridSlot.Left.Set(j * 56 + 10, 0f); // 56px spacing, 10px initial offset
						gridSlot.SetPadding(0);

						if (isUnlocked)
						{
							gridSlot.BackgroundColor = heart.Tier.GetEffectColor() * 0.4f;
							gridSlot.BorderColor = heart.Tier.GetEffectColor() * 1f;
						}
						else
						{
							gridSlot.BackgroundColor = new Color(10, 10, 10) * 0.6f;
							gridSlot.BorderColor = new Color(40, 40, 40);
							if (ElementalHeartsWorldConfig.Instance.AdminMode)
							{
								gridSlot.OnLeftClick += (evt, element) => {
									Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item4, Main.LocalPlayer.Center);
									if (shared) HeartConsumptionWorld.TryConsume(heart);
									else player.TryConsumeLocally(heart);
									Rebuild();
								};
								gridSlot.OnMouseOver += (evt, element) => gridSlot.BackgroundColor = new Color(50, 50, 50) * 0.8f;
								gridSlot.OnMouseOut += (evt, element) => gridSlot.BackgroundColor = new Color(10, 10, 10) * 0.6f;
							}
						}

						HeartIconElement icon = new HeartIconElement(heart.Item, isUnlocked);
						icon.HAlign = 0.5f;
						icon.VAlign = 0.5f;
						gridSlot.Append(icon);

						gridSlot.OnUpdate += (element) => {
							if (element.IsMouseHovering) {
								if (isUnlocked || isTierUnlocked || isBossHeart) {
									Main.HoverItem = heart.Item.Clone();
									Main.hoverItemName = heart.Item.Name;
								} else {
									Main.hoverItemName = "???";
								}
							}
						};

						rowContainer.Append(gridSlot);
					}
				}

				_heartList.Add(rowContainer);
			}
		}
	}

	private bool IsHeartPossible(ElementalHeartItem heart)
	{
		if (!string.IsNullOrEmpty(heart.SourceMod) && heart.SourceMod != "Terraria" && !ModLoader.HasMod(heart.SourceMod))
			return false;

		bool hasRecipe = false;
		for (int r = 0; r < Recipe.numRecipes; r++)
		{
			if (Main.recipe[r].createItem.type == heart.Item.type)
			{
				hasRecipe = true;
				break;
			}
		}
		bool isBossHeart = heart is Content.Items.Hearts.BossHeartItem;
		bool isVanilla = heart.SourceMod == null || heart.SourceMod == "Terraria";
		if (!hasRecipe && !isBossHeart && !isVanilla)
			return false;

		return true;
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

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
		if (Main.LocalPlayer != null && Main.LocalPlayer.active && _claimBtn != null)
		{
			var idlePlayer = Main.LocalPlayer.GetModPlayer<IdleShardPlayer>();
			int pending = idlePlayer.GetPendingShards();
			int capacity = idlePlayer.GetCapacity();

			if (pending >= capacity && capacity > 0)
			{
				float pulse = (float)(System.Math.Sin(Main.GlobalTimeWrappedHourly * 4f) + 1f) / 2f;
				_claimBtn.BackgroundColor = Color.Lerp(new Color(218, 165, 32), new Color(255, 215, 0), pulse);
				_claimBtn.BorderColor = Color.White;
			}
			else
			{
				if (!_claimBtn.IsMouseHovering)
					_claimBtn.BackgroundColor = new Color(63, 82, 151) * 0.7f;
				else
					_claimBtn.BackgroundColor = new Color(73, 94, 171);
				_claimBtn.BorderColor = new Color(89, 116, 213);
			}
		}

		if (Main.GameUpdateCount % 60 == 0)
		{
			UpdateIdleText();
			UpdateDetailedStatsText();
		}
	}

	private void UpdateDetailedStatsText()
	{
		if (_elementalHpText == null || !ElementalHeartsClientConfig.Instance.ShowDetailedHeartStats) return;
		
		var player = Main.LocalPlayer.GetModPlayer<HeartConsumptionPlayer>();
		bool shared = ElementalHeartsWorldConfig.Instance.SharedProgression;
		
		int activatedHearts = 0;
		int totalHearts = 0;
		int elementalHP = player.ActiveHpBonus;
		
		var allHearts = ModContent.GetContent<ElementalHeartItem>().ToList();
		foreach(var heart in allHearts) {
			bool isUnlocked = shared ? HeartConsumptionWorld.IsUnlocked(heart.ConsumptionId) : player.IsUnlockedLocally(heart.ConsumptionId);
			if (isUnlocked) activatedHearts++;

			bool isPossible = IsHeartPossible(heart);
			if (isPossible || isUnlocked) totalHearts++;
		}
		
		var currentTier = player.HighestTier;
		string tierStr = currentTier.HasValue ? currentTier.Value.ToString() : "None";
		
		_elementalHpText.SetText($"Elemental HP: {elementalHP}");
		_worldTierText.SetText($"World Tier: {tierStr}");
		_heartsActivatedText.SetText($"Hearts Activated: {activatedHearts} / {totalHearts}");
	}

	public void UpdateIdleText()
	{
		if (Main.LocalPlayer == null || !Main.LocalPlayer.active) return;
		var idlePlayer = Main.LocalPlayer.GetModPlayer<IdleShardPlayer>();
		
		int pending = idlePlayer.GetPendingShards();
		int capacity = idlePlayer.GetCapacity();
		
		idlePlayer.GetShardRates(out int generation, out int consumption, out int profit);

		if (_generationText != null)
		{
			string icon = $"[i:{ModContent.ItemType<CommonLifeShard>()}]";
			_generationText.SetText($"Generation: {generation} {icon} / day");
			_consumptionText.SetText($"Consumption: {consumption} {icon} / day");
			_profitText.SetText($"Profit: {profit} {icon} / day");

			string colorTag = (pending >= capacity && capacity > 0) ? "[c/32FF32:" : "";
			string colorTagEnd = (pending >= capacity && capacity > 0) ? "]" : "";
			_bankText.SetText($"Bank: {colorTag}{pending}{colorTagEnd} {icon}");
			_limitText.SetText($"Limit: {capacity} {icon}");
		}
	}

	private void RebuildSettingsList()
	{
		_settingsList.Clear();
		
		UIText header = new UIText("Heart Log Settings", 1.2f, true);
		header.HAlign = 0.5f;
		header.MarginBottom = 20f;
		header.TextColor = Color.Gold;
		_settingsList.Add(header);

		AddConfigToggle("Enable Elemental HP", () => ElementalHeartsClientConfig.Instance.EnableElementalHP, val => {
			ElementalHeartsClientConfig.Instance.EnableElementalHP = val;
			SaveConfig(ElementalHeartsClientConfig.Instance);
		});

		AddConfigToggle("Show Permanent Buffs", () => ElementalHeartsClientConfig.Instance.ShowPermanentBuffs, val => {
			ElementalHeartsClientConfig.Instance.ShowPermanentBuffs = val;
			SaveConfig(ElementalHeartsClientConfig.Instance);
		});

		AddConfigToggle("Hide Impossible Hearts", () => ElementalHeartsClientConfig.Instance.HideImpossibleHearts, val => {
			ElementalHeartsClientConfig.Instance.HideImpossibleHearts = val;
			SaveConfig(ElementalHeartsClientConfig.Instance);
		});
	}

	private void AddConfigToggle(string label, System.Func<bool> getter, System.Action<bool> setter)
	{
		UIPanel panel = new UIPanel();
		panel.Width.Set(0, 1f);
		panel.Height.Set(50, 0f);
		panel.BackgroundColor = new Color(30, 38, 70) * 0.8f;
		panel.BorderColor = new Color(89, 116, 213);
		
		UIText text = new UIText(label, 1f);
		text.VAlign = 0.5f;
		text.Left.Set(10, 0f);
		panel.Append(text);

		bool currentVal = getter();
		UIToggleSwitch toggleBtn = new UIToggleSwitch(currentVal);
		toggleBtn.VAlign = 0.5f;
		toggleBtn.HAlign = 1f;
		toggleBtn.Left.Set(-10, 0f);
		
		toggleBtn.OnStateChanged += (newState) => {
			setter(newState);
			Rebuild();
		};
		
		panel.Append(toggleBtn);
		_settingsList.Add(panel);
	}

	private void SaveConfig(Terraria.ModLoader.Config.ModConfig config)
	{
		var saveMethod = typeof(Terraria.ModLoader.Config.ConfigManager).GetMethod("Save", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
		if (saveMethod != null)
			saveMethod.Invoke(null, new object[] { config });
	}
}
