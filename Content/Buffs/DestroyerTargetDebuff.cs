using Terraria;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Buffs;

public class DestroyerTargetDebuff : ModBuff
{
	public override string Texture => "Terraria/Images/Buff_" + Terraria.ID.BuffID.Ichor;

	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = true;
		Main.buffNoSave[Type] = true;
	}
}
