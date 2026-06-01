using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Common.Players;

public sealed class TreasureAbilityPlayer : ModPlayer
{
	public bool Enabled;

	public override void ResetEffects()
	{
		if (Enabled)
		{
			Player.treasureMagnet = true;
		}
	}
}
