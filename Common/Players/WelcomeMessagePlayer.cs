using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using ElementalHearts.Common.UI;

namespace ElementalHearts.Common.Players;

public class WelcomeMessagePlayer : ModPlayer
{
	public bool HideWelcomeMessage;

	public int WelcomeTimer = -1;

	public override void SaveData(TagCompound tag)
	{
		tag["HideWelcomeMessage"] = HideWelcomeMessage;
	}

	public override void LoadData(TagCompound tag)
	{
		HideWelcomeMessage = tag.GetBool("HideWelcomeMessage");
	}

	public override void OnEnterWorld()
	{
		if (Player.whoAmI == Main.myPlayer && !HideWelcomeMessage)
		{
			WelcomeTimer = 1260; // 15 seconds (900) + 6 seconds (360)
		}
	}

	public override void PostUpdate()
	{
		if (WelcomeTimer > 0)
		{
			WelcomeTimer--;
			if (WelcomeTimer == 360) // 15 seconds after joining
			{
				WelcomeUISystem.Show();
				Main.NewText("<Lite> Thanks for enjoying Elemental Hearts!", Microsoft.Xna.Framework.Color.White);
			}
			else if (WelcomeTimer == 240) // 17 seconds after joining
			{
				Main.NewText("<Lite> We need some spriters, so if you have any interest in pixel art, please join the Discord!", Microsoft.Xna.Framework.Color.White);
			}
			else if (WelcomeTimer == 120) // 19 seconds after joining
			{
				Main.NewText("<Lite> We also need testers, especially for multiplayer compatibility and cross-mod compatibility!", Microsoft.Xna.Framework.Color.White);
			}
			else if (WelcomeTimer == 0) // 21 seconds after joining
			{
				Main.NewText("<Lite> I will buy you Aseprite if you need it.", Microsoft.Xna.Framework.Color.White);
			}
		}
	}
}
