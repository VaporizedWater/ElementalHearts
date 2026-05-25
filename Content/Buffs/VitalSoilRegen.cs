using ElementalHearts.Common.Players;
using Terraria;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Buffs;

/// <summary>
/// Buff granted while a player stands on <see cref="Tiles.Vital.VitalSoilTile"/>. The buff
/// only sets a flag on <see cref="VitalTilesPlayer"/> — the regen scaling math happens in
/// that player's <c>UpdateLifeRegen</c> hook, so it composes cleanly with other regen
/// modifiers from potions and accessories.
/// </summary>
public sealed class VitalSoilRegen : ModBuff
{
	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = false;
		Main.buffNoTimeDisplay[Type] = true;
		Main.buffNoSave[Type] = true;
	}

	public override void Update(Player player, ref int buffIndex)
	{
		player.GetModPlayer<VitalTilesPlayer>().VitalSoilRegenActive = true;
	}
}
