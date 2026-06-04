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
using ElementalHearts.Common.UI.Elements;
using ElementalHearts.Common.UI.Effects;
using System;

namespace ElementalHearts.Common.UI.Checklist;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.

public class ChecklistUIState : UIState
{
	public static ChecklistUIState Instance;
	public UIHeartHoverCard HoverCard;
	public bool IsHoveringHeart;

	public enum TabMode { Active, Passive, Milestones }
	public enum SortMode { Tier, Alphabetical }
	public enum FilterMode { All, Unlocked, Locked, Potions, Calamity, Thorium, Consolaria, Vanilla, Zenith }

	private TabMode _tabMode = TabMode.Active;
	private SortMode _sortMode = SortMode.Alphabetical;
	private FilterMode _filterMode = FilterMode.All;
	private string _searchQuery = "";
	private bool _searchBarHasInitializedText = false;

	private AuroraPanel _mainPanel;
	private UITabControl _tabControl;
	private UIList _activeHeartList;
	private UIList _passiveHeartList;
	private UIList _milestonesHeartList;
	private UISmoothScrollbar _scrollbar;
	private UIText _adminText;
	private UIHorizontalList _adminButtonsContainer;

	private UIHorizontalList _leftToolbar;
	private UIHorizontalList _rightToolbar;
	private UIDropdown _sortDropdown;
	private UIDropdown _filterDropdown;
	private UISearchBar _searchBar;
	private AuroraPanel _searchBarContainer;

	private bool _isSettingsMode = false;
	private UIList _settingsList;
	private AuroraPanel _idlePanel;
	private UIText _generationText;
	private UIText _consumptionText;
	private UIText _profitText;
	private UIText _bankText;
	private UIText _limitText;
	private AuroraPanel _statsPanel;
	private UIText _elementalHpText;
	private UIText _worldTierText;
	private UIText _abilitiesActiveText;
	private UIText _heartsActivatedText;
	private UIAnimatedButton _claimBtn;
	private UIAnimatedButton _sellBtn;
	private UIElement _idleRightSection;
	private BankBar _bankBar;

	private UIFlexVertical _bodyFlex;
	private UIElement _statsWrapper;
	private UIElement _adminWrapper;

	public override void OnInitialize()
	{
		Instance = this;

		_mainPanel = new AuroraPanel();
		_mainPanel.Width.Set(0, 0.95f);
		_mainPanel.MaxWidth.Set(1300, 0f);
		_mainPanel.Height.Set(0, 0.85f);
		_mainPanel.MaxHeight.Set(860, 0f);
		_mainPanel.HAlign = 0.5f;
		_mainPanel.VAlign = 0.5f;
		_mainPanel.AuroraColor1 = new Color(80, 85, 95);
		_mainPanel.AuroraColor2 = new Color(60, 65, 75);
		_mainPanel.AuroraColor3 = new Color(90, 95, 105);
		Append(_mainPanel);



		_bodyFlex = new UIFlexVertical();
		_bodyFlex.Width.Set(-40, 1f);
		_bodyFlex.Height.Set(-110, 1f); // Reduce gap between bottom stats and scroll panel
		_bodyFlex.Left.Set(20, 0f);
		_bodyFlex.Top.Set(15, 0f); // Reduced top padding
		_bodyFlex.ListPadding = 10f;
		_mainPanel.Append(_bodyFlex);

		_searchBarContainer = new AuroraPanel();
		_searchBarContainer.Width.Set(200, 0f);
		_searchBarContainer.Height.Set(30, 0f);
		_searchBarContainer.AuroraColor1 = new Color(20, 26, 48);
		_searchBarContainer.AuroraColor2 = new Color(30, 38, 70);
		_searchBarContainer.AuroraColor3 = new Color(15, 20, 35);
		_searchBarContainer.SetPadding(0);
		_searchBarContainer.OnMouseOver += (evt, element) => {
			_searchBarContainer.AuroraColor1 = new Color(30, 40, 80);
		};
		_searchBarContainer.OnMouseOut += (evt, element) => {
			if (!_searchBar.IsWritingText)
				_searchBarContainer.AuroraColor1 = new Color(20, 26, 48);
		};

		_searchBar = new UISearchBar(Terraria.Localization.Language.GetText("Mods.ElementalHearts.UI.SearchHearts"), 0.8f);
		_searchBar.Width.Set(-20, 1f);
		_searchBar.Height.Set(0, 1f);
		_searchBar.HAlign = 0.5f;
		_searchBar.VAlign = 0.5f;
		_searchBar.Left.Set(0, 0f);
		_searchBar.Top.Set(0, 0f);
		
		_searchBar.OnContentsChanged += (contents) => {
			_searchQuery = contents;
			if (_searchQuery != null)
			{
				// Strip zero-width spaces, control characters, and other invisible text listener artifacts
				_searchQuery = new string(_searchQuery.Where(c => c >= 32 && c != 127 && c != '\u200b').ToArray());
			}
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
				_searchBarContainer.AuroraColor1 = new Color(30, 40, 80);
			}
			else if (!_searchBarContainer.IsMouseHovering)
			{
				_searchBarContainer.AuroraColor1 = new Color(20, 26, 48);
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

		UIElement toolbarsRow = new UIElement();
		toolbarsRow.Width.Set(0, 1f);
		toolbarsRow.Height.Set(30, 0f);
		_bodyFlex.Append(toolbarsRow);

		_leftToolbar = new UIHorizontalList();
		_leftToolbar.Width.Set(0, 0.5f);
		_leftToolbar.Height.Set(30, 0f);
		_leftToolbar.ListPadding = 10f;
		toolbarsRow.Append(_leftToolbar);

		_rightToolbar = new UIHorizontalList();
		_rightToolbar.Width.Set(0, 0.5f);
		_rightToolbar.Height.Set(30, 0f);
		_rightToolbar.HAlign = 1f;
		_rightToolbar.RightAligned = true;
		_rightToolbar.ListPadding = 10f;
		toolbarsRow.Append(_rightToolbar);

		UIHorizontalSeparator separator = new UIHorizontalSeparator();
		separator.Width.Set(0, 1f);
		separator.Color = new Color(89, 116, 213) * 0.7f;
		_bodyFlex.Append(separator);

		_statsWrapper = new UIElement();
		_statsWrapper.Width.Set(0, 1f);
		_statsWrapper.Height.Set(0, 0f); // dynamically expanded
		_bodyFlex.Append(_statsWrapper);

		_statsPanel = new AuroraPanel();
		_statsPanel.Width.Set(0, 1f);
		_statsPanel.Height.Set(40, 0f);
		_statsPanel.AuroraColor1 = new Color(15, 20, 35);
		_statsPanel.AuroraColor2 = new Color(20, 26, 48);
		_statsPanel.AuroraColor3 = new Color(10, 15, 25);
		
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

		UIElement tabContainer = new UIElement();
		tabContainer.Width.Set(0, 1f);
		tabContainer.Height.Set(0, 1f); // UIFlexVertical will stretch this to fill!
		tabContainer.OverflowHidden = true;
		_bodyFlex.Append(tabContainer);

		_tabControl = new UITabControl(_leftToolbar);
		_tabControl.Width.Set(0, 1f);
		_tabControl.Height.Set(0, 1f);
		tabContainer.Append(_tabControl);
		
		_scrollbar = new UISmoothScrollbar();
		_scrollbar.SetView(100f, 1000f);
		_scrollbar.Height.Set(0, 1f);
		_scrollbar.HAlign = 1f;
		_scrollbar.Width.Set(0f, 0f);
		_scrollbar.Left.Set(5000f, 0f); // Positioned offscreen and set to 0 width
		tabContainer.Append(_scrollbar);
		
		_activeHeartList = new UIList();
		_activeHeartList.ListPadding = 8f;
		_activeHeartList.SetScrollbar(_scrollbar);
		
		_passiveHeartList = new UIList();
		_passiveHeartList.ListPadding = 8f;
		_passiveHeartList.SetScrollbar(_scrollbar);
		
		_milestonesHeartList = new UIList();
		_milestonesHeartList.ListPadding = 8f;
		_milestonesHeartList.SetScrollbar(_scrollbar);
		
		_settingsList = new UIList();
		_settingsList.ListPadding = 12f;
		_settingsList.SetScrollbar(_scrollbar);
		
		_sortDropdown = new UIDropdown("Sort", new List<string> { "Tier", "Alphabetical" }, _sortMode.ToString(), _mainPanel, (selected) => {
			_sortMode = Enum.Parse<SortMode>(selected);
			RebuildHeartLists();
		});

		var filterOptions = new List<string> { "All", "Unlocked", "Locked", "Potions", "Calamity", "Thorium", "Consolaria", "Vanilla", "Zenith" };
		_filterDropdown = new UIDropdown("Filter", filterOptions, _filterMode.ToString(), _mainPanel, (selected) => {
			_filterMode = Enum.Parse<FilterMode>(selected);
			RebuildHeartLists();
		});

		_tabControl.AddTab("Active", _activeHeartList, () => {
			_tabMode = TabMode.Active;
			_isSettingsMode = false;
			RebuildToolbars();
			RebuildHeartLists();
		});
		
		_tabControl.AddTab("Passive", _passiveHeartList, () => {
			_tabMode = TabMode.Passive;
			_isSettingsMode = false;
			RebuildToolbars();
			RebuildHeartLists();
		});
		
		_tabControl.AddTab("Milestones", _milestonesHeartList, () => {
			_tabMode = TabMode.Milestones;
			_isSettingsMode = false;
			RebuildToolbars();
			RebuildHeartLists();
		});
		
		_tabControl.AddTab("⚙", _settingsList, () => {
			_isSettingsMode = true;
			RebuildToolbars();
			RebuildSettingsList();
		});

		RebuildToolbars();

		_adminWrapper = new UIElement();
		_adminWrapper.Width.Set(0, 1f);
		_adminWrapper.Height.Set(0, 0f); // dynamically expanded
		_bodyFlex.Append(_adminWrapper);

		_adminText = new UIText("", 0.8f);
		_adminText.HAlign = 0.5f;
		_adminText.Top.Set(-15, 0f); // slight offset from grid
		_adminText.TextColor = Color.Red;
		_adminWrapper.Append(_adminText);

		_adminButtonsContainer = new UIHorizontalList();
		_adminButtonsContainer.Width.Set(0, 1f);
		_adminButtonsContainer.Height.Set(45, 0f);
		_adminButtonsContainer.ListPadding = 8f; // Automatically space buttons
		_adminButtonsContainer.SetPadding(0f);

		_idlePanel = new AuroraPanel();
		_idlePanel.Width.Set(0, 1f);
		_idlePanel.Height.Set(85, 0f);
		_idlePanel.VAlign = 1f;
		_idlePanel.AuroraColor1 = new Color(15, 20, 35);
		_idlePanel.AuroraColor2 = new Color(20, 26, 48);
		_idlePanel.AuroraColor3 = new Color(10, 15, 25);
		_idlePanel.SetPadding(0);
		_mainPanel.Append(_idlePanel);

		UIPanel leftSection = new UIPanel();
		leftSection.Width.Set(0, 0.40f); // 40% width
		leftSection.Height.Set(0, 1f);
		leftSection.HAlign = 0f;
		leftSection.VAlign = 0.5f;
		leftSection.BackgroundColor = Color.Transparent;
		leftSection.BorderColor = Color.Transparent;
		leftSection.SetPadding(0);
		leftSection.Left.Set(20f, 0f); // Push right by 20px to align with _statsPanel / list items
		_idlePanel.Append(leftSection);

		_generationText = new UIText("Generation: 0 / day", 0.85f);
		_generationText.TextColor = new Color(150, 255, 150);
		_generationText.Top.Set(14f, 0f);
		_generationText.Left.Set(0f, 0f);
		leftSection.Append(_generationText);

		_consumptionText = new UIText("Consumption: 0 / day", 0.85f);
		_consumptionText.TextColor = new Color(255, 150, 150);
		_consumptionText.Top.Set(35f, 0f);
		_consumptionText.Left.Set(0f, 0f);
		leftSection.Append(_consumptionText);

		_profitText = new UIText("Profit: 0 / day", 0.85f);
		_profitText.TextColor = new Color(200, 200, 200);
		_profitText.Top.Set(56f, 0f);
		_profitText.Left.Set(0f, 0f);
		leftSection.Append(_profitText);

		UIPanel rightSection = new UIPanel();
		rightSection.Width.Set(0, 0.55f); // 55% width
		rightSection.Height.Set(0, 1f);
		rightSection.HAlign = 1f;
		rightSection.VAlign = 0.5f;
		rightSection.BackgroundColor = Color.Transparent;
		rightSection.BorderColor = Color.Transparent;
		rightSection.SetPadding(0);
		rightSection.Left.Set(-20f, 0f); // Inset from right by 20px to align with _statsPanel / list items
		_idlePanel.Append(rightSection);

		_bankText = new UIText("Bank", 0.85f);
		_bankText.TextColor = new Color(255, 215, 0);
		_bankText.Top.Set(14f, 0f);
		_bankText.Left.Set(0f, 0f);
		rightSection.Append(_bankText);

		_bankBar = new BankBar();
		_bankBar.Top.Set(35f, 0f);
		_bankBar.Left.Set(0f, 0f);
		_bankBar.Width.Set(-105f, 1f); // Width scales dynamically, leaving 105px on the right
		_bankBar.Height.Set(26f, 0f);
		rightSection.Append(_bankBar);

		_limitText = new UIText("Limit: 0", 0.9f);
		_limitText.TextColor = new Color(180, 180, 180);

		_claimBtn = new UIAnimatedButton("Claim", 0.9f);
		_claimBtn.FixedWidth = 90f;
		_claimBtn.FixedHeight = 26f;
		_claimBtn.Top.Set(35f, 0f);
		_claimBtn.HAlign = 1f;
		_claimBtn.Left.Set(0f, 0f); // Positioned exactly at the right of the rightSection (HAlign = 1f handles this)
		_claimBtn.BaseColor = new Color(20, 26, 48);
		_claimBtn.HoverColor = new Color(50, 65, 120);
		_claimBtn.OnLeftClick += (evt, element) => {
			var idlePlayer = Main.LocalPlayer.GetModPlayer<IdleShardPlayer>();
			if (idlePlayer.GetPendingShards() > 0) {
				Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item37);
				idlePlayer.ClaimShards();
				UpdateIdleText();
			}
		};
		rightSection.Append(_claimBtn);

		// Unlocked only by the Piggy Bank Heart: cashes the banked shards out as coins (1 gold each)
		// instead of claiming them as items. Tucked directly under "Claim" and attached/detached on
		// demand in Update so it simply isn't there for characters without the heart.
		_idleRightSection = rightSection;
		_sellBtn = new UIAnimatedButton("Sell", 0.9f);
		_sellBtn.FixedWidth = 90f;
		_sellBtn.FixedHeight = 22f;
		_sellBtn.Top.Set(63f, 0f);
		_sellBtn.HAlign = 1f;
		_sellBtn.Left.Set(0f, 0f);
		_sellBtn.BaseColor = new Color(48, 38, 12);   // muted piggy-bank gold
		_sellBtn.HoverColor = new Color(120, 95, 30);
		_sellBtn.OnLeftClick += (evt, element) => {
			var idlePlayer = Main.LocalPlayer.GetModPlayer<IdleShardPlayer>();
			if (idlePlayer.GetPendingShards() > 0) {
				Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Coins);
				idlePlayer.SellShards();
				UpdateIdleText();
			}
		};

		var btnActivateAll = CreateAdminButton("Unlock All");
		btnActivateAll.OnLeftClick += (evt, element) => {
			Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
			var player = Main.LocalPlayer.GetModPlayer<HeartConsumptionPlayer>();
			bool shared = ElementalHeartsServerConfig.Instance.WorldGen.SharedProgression;
			foreach (ElementalHeartItem heart in HeartRegistry.All) {
				if (shared) HeartConsumptionWorld.TryConsume(heart);
				else player.TryConsumeLocally(heart);
			}
			Rebuild();
		};
		_adminButtonsContainer.Append(btnActivateAll);

		var btnClearAll = CreateAdminButton("Clear All");
		btnClearAll.OnLeftClick += (evt, element) => {
			Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
			HeartConsumptionWorld.ClearAllHearts();
			Main.LocalPlayer.GetModPlayer<HeartConsumptionPlayer>().ClearWorldHp();
			Rebuild();
		};
		_adminButtonsContainer.Append(btnClearAll);

		var btnClearTier = CreateAdminButton("Clear Tier");
		btnClearTier.OnLeftClick += (evt, element) => {
			Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
			AnimateProgressionSystem.ClearTier();
			Rebuild();
		};
		_adminButtonsContainer.Append(btnClearTier);

		var btnAdvanceTier = CreateAdminButton("Adv Tier");
		btnAdvanceTier.OnLeftClick += (evt, element) => {
			Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
			AnimateProgressionSystem.AdvanceTier();
			Rebuild();
		};
		_adminButtonsContainer.Append(btnAdvanceTier);

		HoverCard = new UIHeartHoverCard();
		HoverCard.Left.Set(-2000, 0f);
		Append(HoverCard);
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

	private UIAnimatedButton CreateAdminButton(string text)
	{
		var btn = new UIAnimatedButton(text, 0.7f);
		btn.Width.Set(0, 0.24f); // approx 24% width for 4 buttons to fit evenly
		btn.Height.Set(0, 1f);
		btn.VAlign = 0.5f;
		btn.BaseColor = new Color(40, 20, 20); // Reddish for admin
		btn.HoverColor = new Color(80, 30, 30);
		return btn;
	}

	public void Rebuild()
	{
		bool showStats = ElementalHeartsClientConfig.Instance.UI.ShowDetailedHeartStats;
		if (showStats)
		{
			if (_statsPanel.Parent == null) _statsWrapper.Append(_statsPanel);
			_statsWrapper.Height.Set(40, 0f);
		}
		else
		{
			if (_statsPanel.Parent != null) _statsPanel.Remove();
			_statsWrapper.Height.Set(0, 0f);
		}

		if (ElementalHeartsServerConfig.Instance.WorldGen.AdminMode)
		{
			_mainPanel.BorderColor = new Color(255, 215, 0); // Gold border
			_adminText.SetText("- ADMIN MODE ACTIVE -");
			if (_adminButtonsContainer.Parent == null) _adminWrapper.Append(_adminButtonsContainer);
			_adminWrapper.Height.Set(45, 0f);
		}
		else
		{
			_mainPanel.BorderColor = new Color(89, 116, 213);
			_adminText.SetText("");
			if (_adminButtonsContainer.Parent != null) _adminButtonsContainer.Remove();
			_adminWrapper.Height.Set(0, 0f);
		}

		_bodyFlex.Recalculate(); // Force layout update

		UpdateIdleText();
		UpdateDetailedStatsText();
		RebuildHeartLists();
	}

	private void RebuildToolbars()
	{
		if (_isSettingsMode)
		{
			if (_sortDropdown.Parent != null) _sortDropdown.Remove();
			if (_filterDropdown.Parent != null) _filterDropdown.Remove();
			if (_searchBarContainer.Parent != null) _searchBarContainer.Remove();
		}
		else
		{
			if (_searchBarContainer.Parent == null) _rightToolbar.Append(_searchBarContainer);
			if (_sortDropdown.Parent == null) _rightToolbar.Append(_sortDropdown);
			if (_filterDropdown.Parent == null) _rightToolbar.Append(_filterDropdown);
		}
	}

	public void RebuildHeartLists()
	{
		_activeHeartList.Clear();
		_passiveHeartList.Clear();
		_milestonesHeartList.Clear();

		if (Main.LocalPlayer == null || !Main.LocalPlayer.active || Main.gameMenu)
			return;
			
		_mainPanel.BorderColor = AnimateProgressionSystem.CurrentWorldTier.GetEffectColor();

		if (_tabMode == TabMode.Milestones)
		{
			MilestonesUI.RebuildMilestonesList(_milestonesHeartList, RebuildHeartLists);
			return;
		}

		UIList currentList = _tabMode == TabMode.Active ? _activeHeartList : _passiveHeartList;

		IReadOnlyList<ElementalHeartItem> allHearts = HeartRegistry.All;
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
				string localizedName = Lang.GetItemNameValue(heart.Type);
				if (!localizedName.ToLower().Contains(_searchQuery.ToLower()))
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


		if (_sortMode == SortMode.Tier)
		{
			var groups = filteredHearts.GroupBy(h => h.Tier).ToList();
			foreach (var group in groups)
			{
				UIWrapList wrapList = new UIWrapList();
				wrapList.Width.Set(0, 1f);
				wrapList.ItemSpacing = 10f;
				wrapList.LineSpacing = 10f;
				currentList.Add(wrapList);

				foreach (var heart in group)
				{
					AddHeartToWrapList(wrapList, heart, shared, player, unlockedTiers);
				}
			}
		}
		else
		{
			UIWrapList wrapList = new UIWrapList();
			wrapList.Width.Set(0, 1f);
			wrapList.ItemSpacing = 10f;
			wrapList.LineSpacing = 10f;
			currentList.Add(wrapList);

			foreach (var heart in filteredHearts)
			{
				AddHeartToWrapList(wrapList, heart, shared, player, unlockedTiers);
			}
		}
	}

	private void AddHeartToWrapList(UIWrapList wrapList, ElementalHeartItem heart, bool shared, HeartConsumptionPlayer player, System.Collections.Generic.HashSet<HeartTier> unlockedTiers)
	{
		bool isUnlocked = shared 
			? HeartConsumptionWorld.IsUnlocked(heart.ConsumptionId)
			: player.IsUnlockedLocally(heart.ConsumptionId);
		bool isConsumed = shared 
			? HeartConsumptionWorld.IsConsumed(heart.ConsumptionId)
			: player.IsConsumedLocally(heart.ConsumptionId);
		bool isTierUnlocked = unlockedTiers.Contains(heart.Tier);
		bool isBossHeart = heart is BossHeartItem;

		if (_tabMode == TabMode.Active)
		{
			HoverablePanel heartRow = new HoverablePanel();
			heartRow.HoverItem = (isUnlocked || isTierUnlocked || isBossHeart) ? heart.Item : null;
			heartRow.FallbackName = "???";

			string displayName = isUnlocked || isTierUnlocked || isBossHeart ? heart.Item.Name : "???";
			if (displayName.EndsWith(" Heart"))
			{
				displayName = displayName.Substring(0, displayName.Length - 6);
			}

			float nameScale = isUnlocked ? 1f : 0.9f;
			float rawNameWidth = Terraria.GameContent.FontAssets.MouseText.IsLoaded ? Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(displayName).X : 100f;

			float rateWidth = 0f;
			int rate = heart.ActiveAbilityDailyCost > 0 ? heart.ActiveAbilityDailyCost : heart.Tier.GetShardYield();
			bool isAbility = heart is PotionHeartItem || heart.IsActiveAbility;
			string prefix = isAbility ? "-" : "+";
			string rateString = $"{prefix}{rate} [i:{ModContent.ItemType<CommonLifeShard>()}] / day";
			
			if (ElementalHeartsServerConfig.Instance.LifeShards.SystemEnabled && isUnlocked)
			{
				rateWidth = Terraria.GameContent.FontAssets.MouseText.IsLoaded ? Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(rateString).X * 0.85f : 60f;
			}
			else if (isUnlocked && !isAbility)
			{
				rateWidth = Terraria.GameContent.FontAssets.MouseText.IsLoaded ? Terraria.GameContent.FontAssets.MouseText.Value.MeasureString("∞ Passive").X * 0.8f : 50f;
			}

			float availableNameWidth = 380f - 50f - 15f - rateWidth - (isAbility ? 45f : 0f) - 10f;
			if (rawNameWidth * nameScale > availableNameWidth && availableNameWidth > 10f)
			{
				nameScale = availableNameWidth / rawNameWidth;
			}

			heartRow.Width.Set(380f, 0f);
			heartRow.Height.Set(50, 0f);
			heartRow.SetPadding(5f);
			heartRow.BorderColor = AnimateProgressionSystem.CurrentWorldTier.GetEffectColor();
			
			if (isUnlocked)
			{
				HeartEffect effect = HeartEffectRegistry.Get(heart.ConsumptionId);
				heartRow.AuroraColors = effect.Colors;
				heartRow.IsPrismatic = effect.Rainbow;
				
				bool isActive = true;
				if (heart is PotionHeartItem) isActive = isConsumed;
				else if (heart.IsActiveAbility) isActive = heart.IsAbilityEnabled;

				heartRow.AuroraBrightness = isActive ? 1.0f : 0.25f;
				heartRow.IsInteractive = true; // allow hover

				if (heart is PotionHeartItem potionHeart)
				{
					heartRow.OnLeftClick += (evt, element) => {
						bool newState = !isConsumed;
						if (newState)
						{
							var idlePlayer = Main.LocalPlayer.GetModPlayer<IdleShardPlayer>();
							idlePlayer.GetShardRates(out int gen, out int cons, out _);
							int cost = heart.ActiveAbilityDailyCost > 0 ? heart.ActiveAbilityDailyCost : heart.Tier.GetShardYield();
							if (gen < cons + cost)
							{
								Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
								Main.NewText("Not enough shard generation to activate this heart.", Color.Red);
								return;
							}
						}

						Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item4, Main.LocalPlayer.Center);
						if (!newState)
						{
							if (shared) HeartConsumptionWorld.TryDeactivate(potionHeart);
							else player.TryDeactivateLocally(potionHeart);
						}
						else
						{
							if (shared) HeartConsumptionWorld.TryConsume(potionHeart);
							else player.TryConsumeLocally(potionHeart);
						}
						Rebuild();
					};
				}
				else if (heart.IsActiveAbility)
				{
					heartRow.OnLeftClick += (evt, element) => {
						bool newState = !heart.IsAbilityEnabled;
						if (newState)
						{
							var idlePlayer = Main.LocalPlayer.GetModPlayer<IdleShardPlayer>();
							idlePlayer.GetShardRates(out int gen, out int cons, out _);
							int cost = heart.ActiveAbilityDailyCost > 0 ? heart.ActiveAbilityDailyCost : heart.Tier.GetShardYield();
							if (gen < cons + cost)
							{
								Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
								Main.NewText("Not enough shard generation to activate this heart.", Color.Red);
								return;
							}
						}

						Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item4, Main.LocalPlayer.Center);
						heart.SetAbilityEnabled(newState);
						Rebuild();
					};
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
				heartRow.AuroraColor1 = new Color(15, 15, 15);
				heartRow.AuroraColor2 = new Color(10, 10, 10);
				heartRow.AuroraColor3 = new Color(20, 20, 20);

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
					heartRow.OnMouseOver += (evt, element) => {
						heartRow.AuroraColor1 = new Color(50, 50, 50);
						heartRow.AuroraColor2 = new Color(40, 40, 40);
						heartRow.AuroraColor3 = new Color(60, 60, 60);
					};
					heartRow.OnMouseOut += (evt, element) => {
						heartRow.AuroraColor1 = new Color(15, 15, 15);
						heartRow.AuroraColor2 = new Color(10, 10, 10);
						heartRow.AuroraColor3 = new Color(20, 20, 20);
					};
				}
			}

			HeartIconElement icon = new HeartIconElement(heart.Item, isUnlocked);
			icon.VAlign = 0.5f;
			icon.Left.Set(10, 0f);
			heartRow.Append(icon);
			
			UIText nameText = new UIText(displayName, nameScale);
			nameText.VAlign = 0.5f;
			nameText.Left.Set(50, 0f);
			if (!isUnlocked) nameText.TextColor = Color.Gray;
			heartRow.Append(nameText);

			if (ElementalHeartsServerConfig.Instance.LifeShards.SystemEnabled && isUnlocked)
			{
				UIText rateText = new UIText(rateString, 0.85f);
				rateText.TextColor = isAbility ? new Color(255, 150, 150) : new Color(150, 255, 150);
				rateText.VAlign = 0.5f;
				rateText.HAlign = 1f;
				rateText.Left.Set(-20f, 0f); // Put shard cost all the way to the right alignment
				heartRow.Append(rateText);
			}

			wrapList.Append(heartRow);
		}
		else
		{
			HoverablePanel gridSlot = new HoverablePanel();
			gridSlot.HoverItem = (isUnlocked || isTierUnlocked || isBossHeart) ? heart.Item : null;
			gridSlot.FallbackName = "???";
			gridSlot.ShowGenerationRate = isUnlocked;
			gridSlot.Width.Set(48, 0f);
			gridSlot.Height.Set(48, 0f);
			gridSlot.SetPadding(0);

			if (isUnlocked)
			{
				HeartEffect effect = HeartEffectRegistry.Get(heart.ConsumptionId);
				gridSlot.AuroraColors = effect.Colors;
				gridSlot.IsPrismatic = effect.Rainbow;
				gridSlot.AuroraBrightness = 0.6f;
			}
			else
			{
				gridSlot.AuroraColor1 = new Color(20, 25, 40) * 0.7f;
				gridSlot.AuroraColor2 = new Color(15, 20, 32) * 0.5f;
				gridSlot.AuroraColor3 = new Color(25, 30, 48) * 0.9f;
				
				if (ElementalHeartsServerConfig.Instance.WorldGen.AdminMode)
				{
					gridSlot.OnLeftClick += (evt, element) => {
						Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item4, Main.LocalPlayer.Center);
						if (shared) HeartConsumptionWorld.TryConsume(heart);
						else player.TryConsumeLocally(heart);
						Rebuild();
					};
					gridSlot.OnMouseOver += (evt, element) => gridSlot.AuroraColor1 = new Color(50, 50, 50) * 0.8f;
					gridSlot.OnMouseOut += (evt, element) => gridSlot.AuroraColor1 = new Color(10, 10, 10) * 0.6f;
				}
			}

			HeartIconElement icon = new HeartIconElement(heart.Item, isUnlocked);
			icon.HAlign = 0.5f;
			icon.VAlign = 0.5f;
			gridSlot.Append(icon);

			wrapList.Append(gridSlot);
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

	public class HeartIconElement : UIElement
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

			float targetSize = System.Math.Min(dimensions.Width, dimensions.Height);
			float scale = 1f;
			if (frame.Width > targetSize || frame.Height > targetSize)
				scale = targetSize / System.Math.Max(frame.Width, frame.Height);
			else
				scale = targetSize / System.Math.Max(frame.Width, frame.Height); // Force scale up if it's smaller, or maybe just 1f. Let's just scale to fit!
			
			// Actually, just scale to exactly fit the target box:
			scale = targetSize / System.Math.Max(frame.Width, frame.Height);

			Vector2 pos = dimensions.Center();
			Color color = IsConsumed ? Color.White : Color.White * 0.15f; // Draw as a dark grey silhouette instead of pure invisible black
			spriteBatch.Draw(texture, pos, frame, color, 0f, frame.Size() / 2f, scale, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
		}
	}

	private class HoverablePanel : AuroraPanel
	{
		public Item HoverItem;
		public string FallbackName;

		public HoverablePanel()
		{
			IsInteractive = true;
		}

		/// <summary>When true, a hovered unlocked passive heart appends its "Generates N / day" idle yield line.</summary>
		public bool ShowGenerationRate;

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			if (IsMouseHovering && HoverItem != null)
			{
				if (HoverItem.ModItem is ElementalHeartItem heartItem && ChecklistUIState.Instance != null)
				{
					ChecklistUIState.Instance.IsHoveringHeart = true;
					ChecklistUIState.Instance.HoverCard.SetHeart(heartItem);
				}
			}
		}

		protected override void DrawSelf(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
		{
			try
			{
				base.DrawSelf(spriteBatch);
				if (IsMouseHovering)
				{
					if (HoverItem != null)
					{
						if (HoverItem.ModItem is ElementalHeartItem heartItem && ChecklistUIState.Instance != null)
						{
							// Custom HoverCard handles everything; do NOT set Main.HoverItem to avoid vanilla tooltip overlap
						}
						else
						{
							Main.HoverItem = HoverItem.Clone();
							Main.hoverItemName = HoverItem.Name;
							ElementalHeartItem.HideConsumedTooltip = true;
							ElementalHeartItem.ShowGenerationTooltip = ShowGenerationRate;
						}
					}
					else if (FallbackName != null)
					{
						Main.hoverItemName = FallbackName;
					}
				}
			}
			catch (System.Exception e)
			{
				Main.NewTextMultiline(e.ToString(), c: Color.Red);
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

			Rectangle track = GetDimensions().ToRectangle();

			Color bgColor = new Color(8, 11, 24) * 0.95f;
			Color borderColor = full ? Color.White : new Color(89, 116, 213);
			Color fill1 = new Color(58, 104, 198);
			Color fill2 = new Color(120, 196, 255);
			Color pulseColor = new Color(255, 215, 0);

			PremiumProgressBarEffectSystem.Draw(
				spriteBatch, 
				track, 
				frac, 
				2f, 
				bgColor, 
				borderColor, 
				fill1, 
				fill2, 
				pulseColor, 
				full
			);

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
		IsHoveringHeart = false;
		base.Update(gameTime);

		if (_mainPanel != null)
		{
			UIList currentList;
			if (_isSettingsMode) currentList = _settingsList;
			else if (_tabMode == TabMode.Active) currentList = _activeHeartList;
			else if (_tabMode == TabMode.Passive) currentList = _passiveHeartList;
			else if (_tabMode == TabMode.Milestones) currentList = _milestonesHeartList;
			else currentList = _activeHeartList;

			float availableWidth = _mainPanel.GetInnerDimensions().Width - 40f;
			if (availableWidth <= 0f)
			{
				availableWidth = System.Math.Min(1300f, Main.screenWidth * 0.95f) - 40f;
			}

			float listHeight = 0f;
			if (currentList != null)
			{
				foreach (var child in currentList)
				{
					if (child is UIWrapList wrapList)
					{
						float currentY = 0f;
						float currentRowWidth = 0f;
						float rowMaxHeight = 0f;
						int itemsInRow = 0;
						
						foreach (var item in wrapList.Children)
						{
							float itemWidth = item.Width.Pixels;
							if (itemWidth <= 0f) itemWidth = item.GetOuterDimensions().Width;
							if (itemWidth <= 0f) itemWidth = 48f; // fallback for grid slot
							
							float itemHeight = item.Height.Pixels;
							if (itemHeight <= 0f) itemHeight = item.GetOuterDimensions().Height;
							if (itemHeight <= 0f) itemHeight = 48f; // fallback
							
							float spacingNeeded = itemsInRow > 0 ? wrapList.ItemSpacing : 0f;
							
							if (currentRowWidth + spacingNeeded + itemWidth > availableWidth && itemsInRow > 0)
							{
								currentY += rowMaxHeight + wrapList.LineSpacing;
								rowMaxHeight = 0f;
								currentRowWidth = 0f;
								spacingNeeded = 0f;
								itemsInRow = 0;
							}
							
							currentRowWidth += spacingNeeded + itemWidth;
							if (itemHeight > rowMaxHeight) rowMaxHeight = itemHeight;
							itemsInRow++;
						}
						
						if (itemsInRow > 0)
						{
							currentY += rowMaxHeight;
						}
						
						listHeight += currentY + currentList.ListPadding;
					}
					else
					{
						listHeight += child.GetOuterDimensions().Height + currentList.ListPadding;
					}
				}
			}

			// Dynamically sum all fixed height elements inside the panel
			float fixedHeights = 15f + 85f + 10f; // bodyFlex Top (15) + idlePanel Height (85) + bottom margin (10)
			foreach (var element in _bodyFlex.Children)
			{
				if (element.Height.Percent != 1f) // tabContainer has Height.Percent = 1f
				{
					fixedHeights += element.GetOuterDimensions().Height + _bodyFlex.ListPadding;
				}
			}

			float targetHeight = fixedHeights + listHeight;
			float maxHeight = System.Math.Min(860f, Main.screenHeight * 0.85f);
			targetHeight = Microsoft.Xna.Framework.MathHelper.Clamp(targetHeight, 350f, maxHeight);

			if (System.Math.Abs(_mainPanel.Height.Pixels - targetHeight) > 2f)
			{
				_mainPanel.Height.Set(targetHeight, 0f);
				_mainPanel.Recalculate();
			}
		}
		
		if (!IsHoveringHeart && HoverCard != null)
		{
			HoverCard.Left.Set(-2000, 0f);
		}

		if (Main.LocalPlayer != null && Main.LocalPlayer.active && _claimBtn != null)
		{
			var idlePlayer = Main.LocalPlayer.GetModPlayer<IdleShardPlayer>();
			int pending = idlePlayer.GetPendingShards();
			int capacity = idlePlayer.GetCapacity();

			if (pending >= capacity && capacity > 0)
			{
				float pulse = (float)(System.Math.Sin(Main.GlobalTimeWrappedHourly * 4f) + 1f) / 2f;
				_claimBtn.BaseColor = Color.Lerp(new Color(150, 120, 20), new Color(255, 215, 0), pulse);
				_claimBtn.HoverColor = new Color(255, 235, 100);
			}
			else
			{
				_claimBtn.BaseColor = new Color(20, 26, 48);
				_claimBtn.HoverColor = new Color(50, 65, 120);
			}

			// The "Sell" button exists only while the Piggy Bank ability is equipped. Attach/detach on
			// the transition rather than every frame so we never churn the layout for nothing.
			if (_sellBtn != null && _idleRightSection != null)
			{
				bool active = PiggyBankPlayer.IsActive(Main.LocalPlayer);
				bool attached = _sellBtn.Parent == _idleRightSection;
				if (active && !attached)
				{
					_idleRightSection.Append(_sellBtn);
					_bankBar.Top.Set(42f, 0f);
					_bankBar.Height.Set(28f, 0f);
					_claimBtn.Top.Set(30f, 0f);
					_claimBtn.FixedHeight = 24f;
					_sellBtn.Top.Set(58f, 0f);
					_sellBtn.FixedHeight = 24f;
					_idleRightSection.Recalculate();
				}
				else if (!active && attached)
				{
					_sellBtn.Remove();
					_bankBar.Top.Set(35f, 0f);
					_bankBar.Height.Set(26f, 0f);
					_claimBtn.Top.Set(35f, 0f);
					_claimBtn.FixedHeight = 26f;
					_idleRightSection.Recalculate();
				}
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

		IReadOnlyList<ElementalHeartItem> allHearts = HeartRegistry.All;
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
		AuroraPanel panel = new AuroraPanel();
		panel.Width.Set(0, 1f);
		panel.Height.Set(50, 0f);
		panel.AuroraColor1 = new Color(15, 20, 35);
		panel.AuroraColor2 = new Color(20, 26, 48);
		panel.AuroraColor3 = new Color(10, 15, 25);
		panel.BorderColor = new Color(89, 116, 213);
		panel.IsInteractive = false;
		
		UIText text = new UIText(label, 1f);
		text.VAlign = 0.5f;
		text.Left.Set(10, 0f);
		panel.Append(text);

		bool currentVal = getter();
		UIToggleSwitch toggleBtn = new UIToggleSwitch(currentVal);
		toggleBtn.VAlign = 0.5f;
		toggleBtn.HAlign = 1f;
		toggleBtn.Left.Set(-25, 0f); // Prevent outer border overlap
		
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
