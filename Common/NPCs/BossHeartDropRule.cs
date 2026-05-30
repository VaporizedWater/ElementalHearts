using System.Collections.Generic;
using ElementalHearts.Common.Configs;
using ElementalHearts.Common.Systems;
using Terraria;
using Terraria.GameContent.ItemDropRules;

namespace ElementalHearts.Common.NPCs;

/// <summary>
/// Custom drop rule for Boss Hearts. Evaluates drop chances based on <see cref="ElementalHeartsBossConfig"/>
/// and the <see cref="BossFirstKillWorld"/> state, and spawns the <see cref="BossHeartDropFx"/> on success.
/// </summary>
public class BossHeartDropRule : IItemDropRule
{
	public int NpcType { get; }
	public int HeartItemType { get; }
	public List<IItemDropRuleChainAttempt> ChainedRules { get; } = new();

	public BossHeartDropRule(int npcType, int heartItemType)
	{
		NpcType = npcType;
		HeartItemType = heartItemType;
	}

	public bool CanDrop(DropAttemptInfo info) => true;

	public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
	{
		bool isFirstKill = BossFirstKillWorld.IsFirstKill(NpcType);
		bool guaranteed = isFirstKill && ElementalHeartsBossConfig.Instance.BossHeartsGuaranteedOnFirstKill;
		float dropChance = ElementalHeartsBossConfig.Instance.BossHeartDropChance / 100f;

		bool rolled = info.rng.NextFloat() < dropChance;

		if (guaranteed || rolled)
		{
			int index = Item.NewItem(info.npc.GetSource_Loot(), info.npc.Hitbox, HeartItemType);

			if (index >= 0 && index < Main.maxItems)
				BossHeartDropFx.Spawn(Main.item[index].Center, HeartItemType);

			return new ItemDropAttemptResult { State = ItemDropAttemptResultState.Success };
		}

		return new ItemDropAttemptResult { State = ItemDropAttemptResultState.FailedRandomRoll };
	}

	public void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo)
	{
		float dropChance = ElementalHeartsBossConfig.Instance.BossHeartDropChance / 100f;
		float chance = dropChance * ratesInfo.parentDroprateChance;
		
		drops.Add(new DropRateInfo(HeartItemType, 1, 1, chance, ratesInfo.conditions));
		Chains.ReportDroprates(ChainedRules, chance, drops, ratesInfo);
	}
}
