using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace ElementalHearts.Common.UI;

[Autoload(Side = ModSide.Client)]
public class WelcomeUISystem : ModSystem
{
	internal static UserInterface WelcomeInterface;
	internal static WelcomeUIState WelcomeState;

	public override void Load()
	{
		if (!Main.dedServ)
		{
			WelcomeInterface = new UserInterface();
			WelcomeState = new WelcomeUIState();
			WelcomeState.Activate();
			// Initially hide the UI
			WelcomeInterface.SetState(null);
		}
	}

	public override void Unload()
	{
		WelcomeInterface = null;
		WelcomeState = null;
	}

	public static void Show()
	{
		WelcomeState?.ResetTimer();
		WelcomeInterface?.SetState(WelcomeState);
	}

	public static void Hide()
	{
		WelcomeInterface?.SetState(null);
	}

	public override void UpdateUI(GameTime gameTime)
	{
		if (WelcomeInterface?.CurrentState != null)
		{
			WelcomeInterface.Update(gameTime);
		}
	}

	public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
	{
		int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		if (mouseTextIndex != -1)
		{
			layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
				"ElementalHearts: Welcome UI",
				delegate
				{
					if (WelcomeInterface?.CurrentState != null)
					{
						WelcomeInterface.Draw(Main.spriteBatch, new GameTime());
					}
					return true;
				},
				InterfaceScaleType.UI)
			);
		}
	}
}
