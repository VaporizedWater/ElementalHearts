using ElementalHearts.Common.Players;
using Terraria;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Buffs;

/// <summary>
/// Buff granted while a player is within range of a <see cref="Tiles.Vital.VitalQuartzTile"/>.
/// Sets a flag on <see cref="VitalTilesPlayer"/>; the max-HP boost is applied during the
/// player's equip pass.
/// </summary>
public sealed class VitalQuartzAura : ModBuff
{
	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = false;
		Main.buffNoTimeDisplay[Type] = true;
		Main.buffNoSave[Type] = true;
	}

	public override void Update(Player player, ref int buffIndex)
	{
		player.GetModPlayer<VitalTilesPlayer>().VitalQuartzAuraActive = true;
	}
}
