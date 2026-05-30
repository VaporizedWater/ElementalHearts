using System;
using System.Linq;
using ElementalHearts.Common.Configs;
using ElementalHearts.Common.Systems;
using ElementalHearts.Content.Items.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ElementalHearts.Common.Players;

public class IdleShardPlayer : ModPlayer
{
	public long LastClaimTimeTicks;

	public override void Initialize()
	{
		LastClaimTimeTicks = DateTime.UtcNow.Ticks;
	}

	public override void SaveData(TagCompound tag)
	{
		tag["LastClaimTimeTicks"] = LastClaimTimeTicks;
	}

	public override void LoadData(TagCompound tag)
	{
		if (tag.ContainsKey("LastClaimTimeTicks"))
			LastClaimTimeTicks = tag.GetLong("LastClaimTimeTicks");
		else
			LastClaimTimeTicks = DateTime.UtcNow.Ticks;
	}

	public int GetPendingShards()
	{
		if (!ElementalHeartsIdleConfig.Instance.EnableIdleGame)
			return 0;

		TimeSpan elapsed = DateTime.UtcNow - new DateTime(LastClaimTimeTicks);
		double elapsedDays = elapsed.TotalDays;

		if (elapsedDays <= 0)
			return 0;

		int totalWeight = 0;
		bool shared = ElementalHeartsWorldConfig.Instance.SharedProgression;
		var heartConsumptionPlayer = Player.GetModPlayer<HeartConsumptionPlayer>();

		foreach (var heart in ModContent.GetContent<ElementalHeartItem>())
		{
			bool isUnlocked = shared ? HeartConsumptionWorld.IsUnlocked(heart.ConsumptionId) : heartConsumptionPlayer.IsUnlockedLocally(heart.ConsumptionId);
			if (isUnlocked)
			{
				totalWeight += (int)heart.Tier;
			}
		}

		double shardsGenerated = elapsedDays * totalWeight * ElementalHeartsIdleConfig.Instance.BaseShardsPerHeartPerDay;
		
		int pending = (int)Math.Floor(shardsGenerated);
		int cap = GetCapacity();

		return Math.Min(pending, cap);
	}

	public int GetCapacity()
	{
		int tier = AnimateProgressionSystem.UnlockedTier;
		return ElementalHeartsIdleConfig.Instance.BaseCapacity + (tier * ElementalHeartsIdleConfig.Instance.CapacityPerTier);
	}

	public void ClaimShards()
	{
		int pending = GetPendingShards();
		if (pending <= 0)
			return;

		Item shards = new Item();
		shards.SetDefaults(ModContent.ItemType<CommonLifeShard>());
		shards.stack = pending;

		bool absorbedFully = Player.GetModPlayer<LifeShardPlayer>().AbsorbShards(shards);

		int totalWeight = 0;
		bool shared = ElementalHeartsWorldConfig.Instance.SharedProgression;
		var heartConsumptionPlayer = Player.GetModPlayer<HeartConsumptionPlayer>();

		foreach (var heart in ModContent.GetContent<ElementalHeartItem>())
		{
			bool isUnlocked = shared ? HeartConsumptionWorld.IsUnlocked(heart.ConsumptionId) : heartConsumptionPlayer.IsUnlockedLocally(heart.ConsumptionId);
			if (isUnlocked)
			{
				totalWeight += (int)heart.Tier;
			}
		}

		if (totalWeight > 0)
		{
			double ratePerDay = totalWeight * ElementalHeartsIdleConfig.Instance.BaseShardsPerHeartPerDay;
			int actuallyClaimed = pending - shards.stack;

			if (actuallyClaimed > 0)
			{
				double daysConsumed = actuallyClaimed / ratePerDay;
				LastClaimTimeTicks += TimeSpan.FromDays(daysConsumed).Ticks;
				
				if (!absorbedFully && shards.stack > 0)
				{
					Player.QuickSpawnItem(Player.GetSource_Misc("IdleClaim"), shards, shards.stack);
				}
			}
		}
	}
}
