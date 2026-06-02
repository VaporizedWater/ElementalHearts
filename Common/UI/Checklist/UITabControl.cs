using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace ElementalHearts.Common.UI.Elements;

public class UITabControl : UIElement
{
	private UIElement _tabButtonContainer;
	
	private class TabData
	{
		public string Name;
		public UIElement Content;
		public UIAnimatedButton Button;
		public Action OnSelected;
	}
	
	private List<TabData> _tabs = new();
	private TabData _activeTab;
	private TabData _targetTab;
	
	private UIPanel _fadeOverlay;
	private float _fadeOpacity = 0f;
	private int _fadeState = 0; // 0: idle, 1: fading out, 2: fading in
	
	public UITabControl(UIElement tabButtonContainer)
	{
		_tabButtonContainer = tabButtonContainer;
		
		_fadeOverlay = new UIPanel();
		_fadeOverlay.Width.Set(0, 1f);
		_fadeOverlay.Height.Set(0, 1f);
		_fadeOverlay.BackgroundColor = Color.Transparent;
		_fadeOverlay.BorderColor = Color.Transparent;
	}
	
	public void AddTab(string name, UIElement content, Action onSelected = null)
	{
		// Force the content to fill the tab control exactly
		content.Width.Set(0, 1f);
		content.Height.Set(0, 1f);
		content.Top.Set(0, 0f);
		content.Left.Set(0, 0f);
		
		var tabData = new TabData {
			Name = name,
			Content = content,
			OnSelected = onSelected
		};
		
		tabData.Button = new UIAnimatedButton(name, 0.8f);
		tabData.Button.OnLeftClick += (evt, element) => {
			if (_activeTab != tabData && _fadeState == 0)
			{
				SelectTab(name);
			}
		};
		
		_tabs.Add(tabData);
		_tabButtonContainer.Append(tabData.Button);
		
		if (_tabs.Count == 1)
		{
			// Auto-select first tab without fade
			_activeTab = tabData;
			tabData.Button.IsSelected = true;
			Append(tabData.Content);
			tabData.OnSelected?.Invoke();
		}
	}
	
	public void SelectTab(string name)
	{
		var tab = _tabs.Find(t => t.Name == name);
		if (tab != null && tab != _activeTab && _fadeState == 0)
		{
			_targetTab = tab;
			_fadeState = 1; // start fade out
			Append(_fadeOverlay); // put overlay on top
		}
	}
	
	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
		
		if (_fadeState == 1)
		{
			_fadeOpacity += 0.1f;
			if (_fadeOpacity >= 1f)
			{
				_fadeOpacity = 1f;
				_fadeState = 2; // switch to fade in
				
				// Swap content
				if (_activeTab != null && _activeTab.Content.Parent == this)
					RemoveChild(_activeTab.Content);
					
				_activeTab.Button.IsSelected = false;
				_activeTab = _targetTab;
				_activeTab.Button.IsSelected = true;
				
				// Append new content
				Append(_activeTab.Content);
				
				// Move overlay to front again
				RemoveChild(_fadeOverlay);
				Append(_fadeOverlay);
				
				_activeTab.OnSelected?.Invoke();
			}
			_fadeOverlay.BackgroundColor = new Color(20, 26, 48) * _fadeOpacity;
		}
		else if (_fadeState == 2)
		{
			_fadeOpacity -= 0.1f;
			if (_fadeOpacity <= 0f)
			{
				_fadeOpacity = 0f;
				_fadeState = 0;
				RemoveChild(_fadeOverlay);
			}
			_fadeOverlay.BackgroundColor = new Color(20, 26, 48) * _fadeOpacity;
		}
	}
}
