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

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.

public class ChecklistUIState : UIState
{
	public enum TabMode { Active, Passive, Milestones }
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
	private UITextPanel<string> _milestonesTabBtn;
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
	private UIText _abilitiesActiveText;
	private UIText _heartsActivatedText;
	private UITextPanel<string> _claimBtn;
	private BankBar _bankBar;

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
		_searchBarContainer.Height.Set(30, 0f);
		_searchBarContainer.HAlign = 1f;
		_searchBarContainer.Top.Set(40, 0f);
		_searchBarContainer.Left.Set(-40, 0f);
		_searchBarContainer.BackgroundColor = new Color(30, 38, 70) * 0.8f;
		_searchBarContainer.BorderColor = new Color(50, 60, 90);
		_searchBarContainer.SetPadding(0);
		_searchBarContainer.OnMouseOver += (evt, element) => {
			_searchBarContainer.BorderColor = new Color(89, 116, 213);
		};
		_mainPanel.Append(_searchBarContainer);

		_searchBar = new UISearchBar(Terraria.Localization.Language.GetText(""), 0.8f);
		_searchBar.Width.Set(-20, 1f);
		_searchBar.Height.Set(0, 1f);
		_searchBar.HAlign = 0.5f;
		_searchBar.VAlign = 0.5f;
		_searchBar.Left.Set(0, 0f);
		_searchBar.Top.Set(0, 0f);
		
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
			if (_searchBar.IsWritingText)
			{
				_searchBarContainer.BorderColor = new Color(89, 116, 213);
			}
			else if (!_searchBarContainer.IsMouseHovering)
			{
				_searchBarContainer.BorderColor = new Color(50, 60, 90);
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

		_activeTabBtn = new UITextPanel<string>("Active", 0.8f);
		_activeTabBtn.Width.Set(80, 0f);
		_activeTabBtn.Height.Set(30, 0f);
		_activeTabBtn.Top.Set(40, 0f);
		_activeTabBtn.Left.Set(10, 0f);
		_activeTabBtn.BackgroundColor = new Color(73, 94, 171); // Active by default
		_activeTabBtn.BorderColor = new Color(89, 116, 213);
		_activeTabBtn.OnLeftClick += (evt, element) => {
			Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
			_tabMode = TabMode.Active;
			Rebuild();
		};
		_mainPanel.Append(_activeTabBtn);

		_passiveTabBtn = new UITextPanel<string>("Passive", 0.8f);
		_passiveTabBtn.Width.Set(90, 0f);
		_passiveTabBtn.Height.Set(30, 0f);
		_passiveTabBtn.Top.Set(40, 0f);
		_passiveTabBtn.Left.Set(100, 0f);
		_passiveTabBtn.BackgroundColor = new Color(63, 82, 151) * 0.7f;
		_passiveTabBtn.BorderColor = new Color(89, 116, 213);
		_passiveTabBtn.OnLeftClick += (evt, element) => {
			Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
			_tabMode = TabMode.Passive;
			Rebuild();
		};
		_mainPanel.Append(_passiveTabBtn);

		_milestonesTabBtn = new UITextPanel<string>("Milestones", 0.8f);
		_milestonesTabBtn.Width.Set(110, 0f);
		_milestonesTabBtn.Height.Set(30, 0f);
		_milestonesTabBtn.Top.Set(40, 0f);
		_milestonesTabBtn.Left.Set(200, 0f);
		_milestonesTabBtn.BackgroundColor = new Color(63, 82, 151) * 0.7f;
		_milestonesTabBtn.BorderColor = new Color(89, 116, 213);
		_milestonesTabBtn.OnLeftClick += (evt, element) => {
			Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
			_tabMode = TabMode.Milestones;
			Rebuild();
		};
		_mainPanel.Append(_milestonesTabBtn);

		_settingsButton = new UITextPanel<string>("⚙", 0.8f);
		_settingsButton.Width.Set(36, 0f);
		_settingsButton.Height.Set(30, 0f);
		_settingsButton.Top.Set(40, 0f);
		_settingsButton.Left.Set(320, 0f);
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
		_sortButton.Height.Set(30, 0f);
		_sortButton.Top.Set(40, 0f);
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
		_filterButton.Height.Set(30, 0f);
		_filterButton.Top.Set(40, 0f);
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

		// Light the toolbar up on hover so it reads as clickable, the way the admin/claim buttons do.
		// Each call keeps the button's resting colour in sync with whether it's the selected tab/mode.
		AddNavHover(_activeTabBtn, () => _tabMode == TabMode.Active);
		AddNavHover(_passiveTabBtn, () => _tabMode == TabMode.Passive);
		AddNavHover(_milestonesTabBtn, () => _tabMode == TabMode.Milestones);
		AddNavHover(_settingsButton, () => _isSettingsMode);
		AddNavHover(_sortButton, () => false);
		AddNavHover(_filterButton, () => false);

		// Sits centred on the toolbar row's free gap (between the Settings button and the Sort button)
		// so it no longer collides with the title now that the title has the top row to itself.
		_adminText = new UIText("", 0.8f);
		_adminText.HAlign = 0.5f;
		_adminText.Top.Set(48, 0f);
		_adminText.TextColor = Color.Red;
		_mainPanel.Append(_adminText);

		UIHorizontalSeparator separator = new UIHorizontalSeparator();
		separator.Width.Set(0, 1f);
		separator.Top.Set(80, 0f);
		separator.Color = new Color(89, 116, 213) * 0.7f;
		_mainPanel.Append(separator);

		_statsPanel = new UIPanel();
		_statsPanel.Width.Set(0, 1f);
		_statsPanel.Height.Set(40, 0f);
		_statsPanel.Top.Set(90, 0f);
		_statsPanel.BackgroundColor = new Color(30, 38, 70) * 0.8f;
		_statsPanel.BorderColor = new Color(89, 116, 213);
		
		_elementalHpText = new UIText("", 0.9f);
		_elementalHpText.VAlign = 0.5f;
		_elementalHpText.Left.Set(20, 0f);
		_statsPanel.Append(_elementalHpText);

		_worldTierText = new UIText("", 0.9f);
		_worldTierText.VAlign = 0.5f;
		_worldTierText.HAlign = 0.34f;
		_statsPanel.Append(_worldTierText);

		_abilitiesActiveText = new UIText("", 0.9f);
		_abilitiesActiveText.VAlign = 0.5f;
		_abilitiesActiveText.HAlign = 0.67f;
		_statsPanel.Append(_abilitiesActiveText);

		_heartsActivatedText = new UIText("", 0.9f);
		_heartsActivatedText.VAlign = 0.5f;
		_heartsActivatedText.HAlign = 1f;
		_heartsActivatedText.Left.Set(-20, 0f);
		_statsPanel.Append(_heartsActivatedText);

		_heartList = new UIList();
		_heartList.Width.Set(-25, 1f);
		_heartList.Height.Set(-170, 1f);
		_heartList.Top.Set(95, 0f);
		_heartList.ListPadding = 8f;
		_mainPanel.Append(_heartList);

		_settingsList = new UIList();
		_settingsList.Width.Set(-25, 1f);
		_settingsList.Height.Set(-170, 1f);
		_settingsList.Top.Set(95, 0f);
		_settingsList.ListPadding = 12f;

		_scrollbar = new UIScrollbar();
		_scrollbar.SetView(100f, 1000f);
		_scrollbar.Height.Set(-170, 1f);
		_scrollbar.Top.Set(95, 0f);
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

		_bankText = new UIText("Bank", 0.85f);
		_bankText.Top.Set(2, 0f);
		_bankText.Left.Set(16, 0f);
		_bankText.TextColor = new Color(255, 215, 0);
		rightSection.Append(_bankText);

		// A filled gauge reads pending/capacity at a glance: it eases blue→cyan as it fills and
		// pulses gold the instant the bank is capped, so "go claim" carries across the screen.
		_bankBar = new BankBar();
		_bankBar.Left.Set(16, 0f);
		_bankBar.Top.Set(24, 0f);
		_bankBar.Width.Set(-150, 1f);
		_bankBar.Height.Set(26, 0f);
		rightSection.Append(_bankBar);

		// Still created so the live-number path in UpdateIdleText stays valid, but the bar now
		// shows the cap, so the standalone "Limit" line is intentionally left out of the layout.
		_limitText = new UIText("Limit: 0", 0.9f);
		_limitText.TextColor = new Color(180, 180, 180);

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
			bool shared = ElementalHeartsServerConfig.Instance.WorldGen.SharedProgression;
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

	// Resting and hover tints shared by the nav toolbar; the "active" tab/mode keeps the brighter
	// selected fill, so hover only lifts buttons that aren't already lit.
	private static readonly Color NavRest = new Color(63, 82, 151) * 0.7f;
	private static readonly Color NavSelected = new Color(73, 94, 171);
	private static readonly Color NavHover = new Color(93, 114, 191);

	/// <summary>Gives a toolbar button a hover lift that respects its selected state: brightens on
	/// mouse-over only when <paramref name="isSelected"/> is false, and restores to the right
	/// resting colour (selected fill vs. base) on mouse-out.</summary>
	private void AddNavHover(UITextPanel<string> btn, System.Func<bool> isSelected)
	{
		btn.OnMouseOver += (evt, element) => {
			if (!isSelected()) btn.BackgroundColor = NavHover;
		};
		btn.OnMouseOut += (evt, element) => {
			btn.BackgroundColor = isSelected() ? NavSelected : NavRest;
		};
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
		bool showStats = ElementalHeartsClientConfig.Instance.UI.ShowDetailedHeartStats;
		if (showStats)
		{
			if (_statsPanel.Parent == null) _mainPanel.Append(_statsPanel);
		}
		else
		{
			if (_statsPanel.Parent != null) _mainPanel.RemoveChild(_statsPanel);
		}
		
		float listTopOffset = showStats ? 140f : 95f;
		float baseHeightOffset = showStats ? -215f : -170f;

		if (ElementalHeartsServerConfig.Instance.WorldGen.AdminMode)
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
		_milestonesTabBtn.BackgroundColor = _tabMode == TabMode.Milestones ? new Color(73, 94, 171) : new Color(63, 82, 151) * 0.7f;

		if (_isSettingsMode)
		{
			if (_heartList.Parent != null) _mainPanel.RemoveChild(_heartList);
			if (_settingsList.Parent == null) _mainPanel.Append(_settingsList);
			if (_sortButton.Parent != null) _mainPanel.RemoveChild(_sortButton);
			if (_filterButton.Parent != null) _mainPanel.RemoveChild(_filterButton);
			if (_searchBarContainer.Parent != null) _mainPanel.RemoveChild(_searchBarContainer);
			if (_activeTabBtn.Parent != null) _mainPanel.RemoveChild(_activeTabBtn);
			if (_passiveTabBtn.Parent != null) _mainPanel.RemoveChild(_passiveTabBtn);
			if (_milestonesTabBtn.Parent != null) _mainPanel.RemoveChild(_milestonesTabBtn);
			
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
			if (_milestonesTabBtn.Parent == null) _mainPanel.Append(_milestonesTabBtn);
		}

		_heartList.Clear();

		if (_tabMode == TabMode.Milestones)
		{
			MilestonesUI.RebuildMilestonesList(_heartList, Rebuild);
			return;
		}

		var allHearts = ModContent.GetContent<ElementalHeartItem>().ToList();
		bool shared = ElementalHeartsServerConfig.Instance.WorldGen.SharedProgression;
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
			
			if (ElementalHeartsClientConfig.Instance.UI.HideImpossibleHearts && !isUnlocked)
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

						HoverablePanel heartRow = new HoverablePanel();
						heartRow.HoverItem = (isUnlocked || isTierUnlocked || isBossHeart) ? heart.Item : null;
						heartRow.FallbackName = "???";
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
										int cost = heart.ActiveAbilityDailyCost ?? IdleShardPlayer.GetShardYield(heart.Tier);
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
										int cost = heart.ActiveAbilityDailyCost ?? IdleShardPlayer.GetShardYield(heart.Tier);
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

							if (ElementalHeartsServerConfig.Instance.WorldGen.AdminMode)
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
						if (_tabMode == TabMode.Active && displayName.EndsWith(" Heart"))
						{
							displayName = displayName.Substring(0, displayName.Length - 6);
						}
						
						UIText nameText = new UIText(displayName, isUnlocked ? 1f : 0.9f);
						nameText.VAlign = 0.5f;
						nameText.Left.Set(50, 0f);
						if (!isUnlocked) nameText.TextColor = Color.Gray;
						heartRow.Append(nameText);

						if (ElementalHeartsClientConfig.Instance.Idle.EnableIdleGame && isUnlocked)
						{
							int rate = heart.ActiveAbilityDailyCost ?? IdleShardPlayer.GetShardYield(heart.Tier);
							bool isAbility = heart is PotionHeartItem || heart.IsActiveAbility;
							string prefix = isAbility ? "-" : "+";
							UIText rateText = new UIText($"{prefix}{rate} [i:{ModContent.ItemType<CommonLifeShard>()}] / day", 0.85f);
							rateText.TextColor = isAbility ? new Color(255, 150, 150) : new Color(150, 255, 150);
							rateText.VAlign = 0.5f;
							rateText.HAlign = 1f;
							rateText.Left.Set(-100, 0f); // Rigidly aligned for all
							heartRow.Append(rateText);
						}

						// Replaced by HoverablePanel logic

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

						HoverablePanel gridSlot = new HoverablePanel();
						gridSlot.HoverItem = (isUnlocked || isTierUnlocked || isBossHeart) ? heart.Item : null;
						gridSlot.FallbackName = "???";
						gridSlot.ShowGenerationRate = isUnlocked;
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
							if (ElementalHeartsServerConfig.Instance.WorldGen.AdminMode)
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

						// Replaced by HoverablePanel logic

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

	private class HoverablePanel : UIPanel
	{
		public Item HoverItem;
		public string FallbackName;

		/// <summary>When true, a hovered unlocked passive heart appends its "Generates N / day" idle yield line.</summary>
		public bool ShowGenerationRate;

		protected override void DrawSelf(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);
			if (IsMouseHovering)
			{
				if (HoverItem != null)
				{
					Main.HoverItem = HoverItem.Clone();
					Main.hoverItemName = HoverItem.Name;
					ElementalHeartItem.HideConsumedTooltip = true;
					ElementalHeartItem.ShowGenerationTooltip = ShowGenerationRate;
				}
				else if (FallbackName != null)
				{
					Main.hoverItemName = FallbackName;
				}
			}
		}
	}

	/// <summary>
	/// The idle-bank fill gauge. Reads live pending/capacity every frame and draws a flat track
	/// with a coloured fill — blue while filling, shifting to a pulsing gold the instant the bank
	/// is capped — with the running "have / cap" count centred on top. Hovering spells it out.
	/// </summary>
	private class BankBar : UIElement
	{
		protected override void DrawSelf(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
		{
			if (Main.LocalPlayer == null || !Main.LocalPlayer.active)
				return;

			var idle = Main.LocalPlayer.GetModPlayer<IdleShardPlayer>();
			int pending = idle.GetPendingShards();
			int capacity = idle.GetCapacity();
			bool full = capacity > 0 && pending >= capacity;
			float frac = capacity > 0 ? MathHelper.Clamp((float)pending / capacity, 0f, 1f) : 0f;

			var pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
			Rectangle track = GetDimensions().ToRectangle();

			// Recessed track.
			spriteBatch.Draw(pixel, track, new Color(8, 11, 24) * 0.95f);

			// Fill — width follows the fraction; colour eases blue→cyan as it climbs, then pulses
			// gold once capped so a full bank is unmistakable from a distance.
			int fillWidth = (int)(track.Width * frac);
			if (fillWidth > 0)
			{
				Color fill = Color.Lerp(new Color(58, 104, 198), new Color(120, 196, 255), frac);
				if (full)
				{
					float pulse = (float)(System.Math.Sin(Main.GlobalTimeWrappedHourly * 4f) + 1f) / 2f;
					fill = Color.Lerp(new Color(218, 165, 32), new Color(255, 224, 96), pulse);
				}
				spriteBatch.Draw(pixel, new Rectangle(track.X, track.Y, fillWidth, track.Height), fill);

				// A brighter leading edge gives the fill a sliver of depth.
				spriteBatch.Draw(pixel, new Rectangle(track.X + fillWidth - 2, track.Y, 2, track.Height), Color.White * 0.5f);
			}

			// 2px border, white when capped to echo the Claim button's "ready" state.
			Color border = full ? Color.White : new Color(89, 116, 213);
			spriteBatch.Draw(pixel, new Rectangle(track.X, track.Y, track.Width, 2), border);
			spriteBatch.Draw(pixel, new Rectangle(track.X, track.Bottom - 2, track.Width, 2), border);
			spriteBatch.Draw(pixel, new Rectangle(track.X, track.Y, 2, track.Height), border);
			spriteBatch.Draw(pixel, new Rectangle(track.Right - 2, track.Y, 2, track.Height), border);

			// Centred "have / cap" count, drawn with a soft border so it stays legible over any fill.
			string label = $"{pending} / {capacity}";
			float scale = 0.85f;
			Vector2 size = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(label) * scale;
			Vector2 pos = new Vector2(track.X + ((track.Width - size.X) / 2f), track.Y + ((track.Height - size.Y) / 2f));
			Utils.DrawBorderString(spriteBatch, label, pos, Color.White, scale);

			if (IsMouseHovering)
			{
				int percent = (int)(frac * 100f);
				Main.instance.MouseText($"{pending} / {capacity} Life Shards banked ({percent}% full)");
			}
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
		if (_elementalHpText == null || !ElementalHeartsClientConfig.Instance.UI.ShowDetailedHeartStats) return;
		
		var player = Main.LocalPlayer.GetModPlayer<HeartConsumptionPlayer>();
		bool shared = ElementalHeartsServerConfig.Instance.WorldGen.SharedProgression;
		
		int activatedHearts = 0;
		int totalHearts = 0;
		int abilitiesActive = 0;
		int elementalHP = player.ActiveHpBonus;

		var allHearts = ModContent.GetContent<ElementalHeartItem>().ToList();
		foreach(var heart in allHearts) {
			bool isUnlocked = shared ? HeartConsumptionWorld.IsUnlocked(heart.ConsumptionId) : player.IsUnlockedLocally(heart.ConsumptionId);
			if (isUnlocked) activatedHearts++;

			if (heart.IsActiveAbility && heart.IsAbilityEnabled) abilitiesActive++;

			bool isPossible = IsHeartPossible(heart);
			if (isPossible || isUnlocked) totalHearts++;
		}
		
		// World tier is what Animate has unlocked, not the highest heart the player happens to have
		// consumed — the latter is a HeartTier that can read "Exotic", which is a cross-mod heart
		// rarity and never a world tier. See AnimateProgressionSystem.CurrentWorldTier.
		string tierStr = AnimateProgressionSystem.CurrentWorldTier.ToString();

		int maxCapacity = HeartCapacitySystem.GetMaxCapacity() ?? int.MaxValue;
		int appliedHp = System.Math.Min(elementalHP, maxCapacity);
		string capacityStr = maxCapacity == int.MaxValue ? "∞" : maxCapacity.ToString();
		_elementalHpText.SetText($"Elemental HP: {appliedHp} / {capacityStr}");
		_worldTierText.SetText($"World Tier: {tierStr}");
		_abilitiesActiveText.SetText($"Abilities Active: {abilitiesActive}");
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
			_profitText.SetText($"Profit: {(profit > 0 ? "+" : "")}{profit} {icon} / day");

			// Profit reads green when you're net-positive and dims to grey at break-even, so the
			// colour alone tells you whether the bank is actually filling.
			_profitText.TextColor = profit > 0 ? new Color(150, 255, 150) : new Color(170, 170, 170);

			// The bank's running count now lives inside the progress bar (see BankBar); the
			// caption just flips green the moment you're capped and a claim is waiting.
			_bankText.SetText("Bank");
			_bankText.TextColor = (pending >= capacity && capacity > 0)
				? new Color(120, 255, 120)
				: new Color(255, 215, 0);
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

		AddConfigToggle("Show Detailed Stats Bar", () => ElementalHeartsClientConfig.Instance.UI.ShowDetailedHeartStats, val => {
			ElementalHeartsClientConfig.Instance.UI.ShowDetailedHeartStats = val;
			SaveConfig(ElementalHeartsClientConfig.Instance);
		});

		AddConfigToggle("Enable Elemental HP", () => ElementalHeartsClientConfig.Instance.UI.EnableElementalHP, val => {
			ElementalHeartsClientConfig.Instance.UI.EnableElementalHP = val;
			SaveConfig(ElementalHeartsClientConfig.Instance);
		});

		AddConfigToggle("Show Permanent Buffs", () => ElementalHeartsClientConfig.Instance.UI.ShowPermanentBuffs, val => {
			ElementalHeartsClientConfig.Instance.UI.ShowPermanentBuffs = val;
			SaveConfig(ElementalHeartsClientConfig.Instance);
		});

		AddConfigToggle("Hide Impossible Hearts", () => ElementalHeartsClientConfig.Instance.UI.HideImpossibleHearts, val => {
			ElementalHeartsClientConfig.Instance.UI.HideImpossibleHearts = val;
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
