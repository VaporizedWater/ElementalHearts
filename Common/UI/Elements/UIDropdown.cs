using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using System.Collections.Generic;
using System;

namespace ElementalHearts.Common.UI.Elements;

public class UIDropdown : UIElement
{
	private UIAnimatedButton _mainButton;
	private UIPanel _dropdownPanel;
	private UIList _optionsList;
	private UIScrollbar _scrollbar;
	
	private List<string> _options;
	private string _selectedOption;
	private Action<string> _onOptionSelected;
	private UIElement _rootPanel; // The panel to attach the dropdown list to
	private string _prefix;
	
	private bool _isOpen = false;

	public UIDropdown(string prefix, List<string> options, string initialOption, UIElement rootPanel, Action<string> onOptionSelected)
	{
		_prefix = prefix;
		_options = options;
		_selectedOption = initialOption;
		_rootPanel = rootPanel;
		_onOptionSelected = onOptionSelected;
		
		_mainButton = new UIAnimatedButton($"{prefix}: {_selectedOption} ▼", 0.8f);
		_mainButton.OnLeftClick += ToggleDropdown;
		Append(_mainButton);
		UpdateSize();

		_dropdownPanel = new UIPanel();
		_dropdownPanel.BackgroundColor = new Color(20, 26, 48) * 0.95f;
		_dropdownPanel.BorderColor = new Color(89, 116, 213);
		_dropdownPanel.SetPadding(5f);

		_optionsList = new UIList();
		_optionsList.Width.Set(-20, 1f);
		_optionsList.Height.Set(0, 1f);
		_optionsList.ListPadding = 5f;
		_dropdownPanel.Append(_optionsList);

		_scrollbar = new UIScrollbar();
		_scrollbar.SetView(100f, 1000f);
		_scrollbar.Height.Set(0, 1f);
		_scrollbar.HAlign = 1f;
		_dropdownPanel.Append(_scrollbar);
		_optionsList.SetScrollbar(_scrollbar);

		PopulateOptions();
	}

	private void PopulateOptions()
	{
		_optionsList.Clear();
		foreach (var option in _options)
		{
			string opt = option; // capture
			var optPanel = new UIAnimatedButton(opt, 0.8f);
			optPanel.Width.Set(0, 1f);
			optPanel.Height.Set(30, 0f);
			
			optPanel.IsSelected = (opt == _selectedOption);
			
			optPanel.OnLeftClick += (evt, element) => {
				Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
				_selectedOption = opt;
				_mainButton.SetText($"{_prefix}: {_selectedOption} ▼");
				UpdateSize();
				CloseDropdown();
				PopulateOptions(); // Rebuild options to update selected color
				_onOptionSelected?.Invoke(opt);
				if (Parent != null) Parent.RecalculateChildren();
			};
			
			_optionsList.Add(optPanel);
		}
	}

	private void ToggleDropdown(UIMouseEvent evt, UIElement listeningElement)
	{
		Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
		if (_isOpen) CloseDropdown();
		else OpenDropdown();
	}

	private void OpenDropdown()
	{
		_isOpen = true;
		
		var dimensions = GetDimensions();
		var rootDimensions = _rootPanel.GetDimensions();
		Vector2 relativePos = dimensions.Position() - rootDimensions.Position();
		
		_dropdownPanel.Width.Set(dimensions.Width, 0f);
		
		// Calculate height based on options count, capped at 250px
		float listHeight = Math.Min(_options.Count * 35f + 10f, 250f);
		_dropdownPanel.Height.Set(listHeight, 0f);
		
		// Position the dropdown below the main button, relative to root panel
		_dropdownPanel.Left.Set(relativePos.X, 0f);
		_dropdownPanel.Top.Set(relativePos.Y + dimensions.Height, 0f);

		_rootPanel.Append(_dropdownPanel);
	}

	public void CloseDropdown()
	{
		if (!_isOpen) return;
		_isOpen = false;
		if (_dropdownPanel.Parent != null)
		{
			_dropdownPanel.Remove();
		}
	}
	
	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
		
		// Close dropdown if mouse clicked outside of it
		if (_isOpen && Main.mouseLeft && Main.mouseLeftRelease)
		{
			if (!_dropdownPanel.ContainsPoint(Main.MouseScreen) && !_mainButton.ContainsPoint(Main.MouseScreen))
			{
				CloseDropdown();
			}
		}
	}

	private void UpdateSize()
	{
		_mainButton.Recalculate();
		Width.Set(_mainButton.GetOuterDimensions().Width, 0f);
		Height.Set(_mainButton.GetOuterDimensions().Height, 0f);
	}
}
