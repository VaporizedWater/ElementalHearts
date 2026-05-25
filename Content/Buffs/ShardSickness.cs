using Terraria;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Buffs;

/// <summary>
/// Cooldown debuff applied after a Life Shard is consumed via quick-heal. Kept separate
/// from vanilla <c>BuffID.PotionSickness</c> so shard healing and potion healing don't
/// gate each other — a player on shard cooldown can still chug a Greater Healing Potion,
/// and vice versa.
/// </summary>
public sealed class ShardSickness : ModBuff
{
	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = true;
		Main.buffNoTimeDisplay[Type] = false;
		Main.buffNoSave[Type] = false;
	}
}
