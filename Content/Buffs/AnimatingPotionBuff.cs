using ElementalHearts.Common.LifeShards;
using ElementalHearts.Common.Players;
using Terraria;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Buffs;

/// <summary>
/// Base class for the five Animating Potion buffs. A concrete buff only declares its
/// <see cref="Tier"/>; its icon is the matching sprite sitting beside it, and every frame
/// it's active it registers its tier with <see cref="HeartConsumptionPlayer"/>, which
/// applies the max-life and elemental-life bonuses during its stat pass.
/// </summary>
public abstract class AnimatingPotionBuff : ModBuff
{
	public abstract LifeShardTier Tier { get; }

	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = false;
		Main.buffNoTimeDisplay[Type] = false;
	}

	public override void Update(Player player, ref int buffIndex)
	{
		player.GetModPlayer<HeartConsumptionPlayer>().ApplyAnimatingPotion(Tier);
	}
}
