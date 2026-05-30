using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using ElementalHearts.Common.Configs;

namespace ElementalHearts.Common.UI.Checklist;

[Autoload(Side = ModSide.Client)]
public class ChecklistUISystem : ModSystem
{
	internal UserInterface ChecklistInterface;
	internal ChecklistUIState ChecklistState;
	
	internal UserInterface ButtonInterface;
	internal HeartLogButtonUIState ButtonState;
	public static ModKeybind ToggleChecklistKeybind { get; private set; }

	public override void Load()
	{
		ToggleChecklistKeybind = KeybindLoader.RegisterKeybind(Mod, "Toggle Heart Checklist", "L");
		if (!Main.dedServ)
		{
			ChecklistInterface = new UserInterface();
			ChecklistState = new ChecklistUIState();
			ChecklistState.Activate();
			
			ButtonInterface = new UserInterface();
			ButtonState = new HeartLogButtonUIState();
			ButtonState.Activate();
			ButtonInterface.SetState(ButtonState);
		}
	}

	public override void Unload()
	{
		ChecklistInterface = null;
		ChecklistState = null;
		ButtonInterface = null;
		ButtonState = null;
		ToggleChecklistKeybind = null;
	}

	public static void ToggleUI()
	{
		if (!ElementalHeartsClientConfig.Instance.EnableHeartChecklist)
		{
			// Force hide if disabled
			var system = ModContent.GetInstance<ChecklistUISystem>();
			system?.ChecklistInterface?.SetState(null);
			return;
		}

		var sys = ModContent.GetInstance<ChecklistUISystem>();
		if (sys.ChecklistInterface.CurrentState != null)
			sys.ChecklistInterface.SetState(null);
		else
		{
			Main.playerInventory = false;
			sys.ChecklistState.Rebuild();
			sys.ChecklistInterface.SetState(sys.ChecklistState);
		}
	}

	public override void UpdateUI(GameTime gameTime)
	{
		if (ChecklistInterface?.CurrentState != null)
		{
			if (!ElementalHeartsClientConfig.Instance.EnableHeartChecklist)
			{
				ChecklistInterface.SetState(null); // Auto-close if disabled in config while open
			}
			else if (Main.playerInventory)
			{
				ChecklistInterface.SetState(null); // Close if the player opens their inventory (e.g. presses ESC)
			}
			else
			{
				ChecklistInterface.Update(gameTime);
			}
		}

		if (Main.playerInventory && ElementalHeartsClientConfig.Instance.EnableHeartChecklist)
		{
			ButtonInterface?.Update(gameTime);
		}
	}

	public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
	{
		int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		if (mouseTextIndex != -1)
		{
			layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
				"ElementalHearts: Checklist UI",
				delegate
				{
					if (ChecklistInterface?.CurrentState != null)
						ChecklistInterface.Draw(Main.spriteBatch, new GameTime());
					return true;
				},
				InterfaceScaleType.UI)
			);
			
			layers.Insert(mouseTextIndex + 1, new LegacyGameInterfaceLayer(
				"ElementalHearts: Heart Log Button",
				delegate
				{
					if (Main.playerInventory && ElementalHeartsClientConfig.Instance.EnableHeartChecklist)
						ButtonInterface?.Draw(Main.spriteBatch, new GameTime());
					return true;
				},
				InterfaceScaleType.UI)
			);
		}
	}
}
