using System;
using System.Linq;
using ElementalHearts.Common.Configs;
using ElementalHearts.Common.Hearts;
using ElementalHearts.Common.Systems;
using ElementalHearts.Content.Items.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using ElementalHearts.Common.Network;

namespace ElementalHearts.Common.Players;

public class IdleShardPlayer : ModPlayer
{
	// No longer tracks LastClaimTimeTicks per player.

	public void GetShardRates(out int generation, out int consumption, out int profit)
	{
		generation = 0;
		consumption = 0;

		bool shared = ElementalHeartsServerConfig.Instance.WorldGen.SharedProgression;
		var heartConsumptionPlayer = Player.GetModPlayer<HeartConsumptionPlayer>();

		foreach (ElementalHeartItem heart in HeartRegistry.All)
		{
			bool isUnlocked = shared ? HeartConsumptionWorld.IsUnlocked(heart.ConsumptionId) : heartConsumptionPlayer.IsUnlockedLocally(heart.ConsumptionId);
			if (!isUnlocked) continue;

			bool hasToggle = heart is PotionHeartItem || heart.IsActiveAbility;
			if (hasToggle)
			{
				// Active-ability hearts (e.g. Magnification, Jack-O'-Lantern) track their "on" state in
				// a dedicated per-character flag; potion hearts track theirs in the consumption ledger.
				bool isConsumed = heart.IsActiveAbility
					? heart.IsAbilityEnabled
					: (shared ? HeartConsumptionWorld.IsConsumed(heart.ConsumptionId) : heartConsumptionPlayer.IsConsumedLocally(heart.ConsumptionId));

				if (isConsumed)
				{
					consumption += heart.ActiveAbilityDailyCost;
				}
			}
			else
			{
				generation += heart.Tier.GetShardYield();
			}
		}

		profit = Math.Max(0, generation - consumption);
	}

	public int GetPendingShards()
	{
		if (!ElementalHeartsServerConfig.Instance.LifeShards.SystemEnabled)
			return 0;

		TimeSpan elapsed = DateTime.UtcNow - new DateTime(IdleShardWorld.LastClaimTimeTicks);
		double elapsedTerrariaDays = elapsed.TotalMinutes / 24.0;

		if (elapsedTerrariaDays <= 0)
			return 0;

		GetShardRates(out _, out _, out int profit);

		double shardsGenerated = elapsedTerrariaDays * profit;
		
		int pending = (int)Math.Floor(shardsGenerated);
		int cap = GetCapacity();

		return Math.Min(pending, cap);
	}

	public int GetCapacity()
	{
		int tier = AnimateProgressionSystem.UnlockedTier;
		return ElementalHeartsClientConfig.Instance.Idle.BaseCapacity + (tier * ElementalHeartsClientConfig.Instance.Idle.CapacityPerTier);
	}

	public void ClaimShards()
	{
		if (Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient)
		{
			ModPacket packet = Mod.GetPacket();
			packet.Write((byte)MessageType.ClaimIdleShards);
			packet.Send();
			return;
		}

		int pending = GetPendingShards();
		if (pending <= 0)
			return;

		Item shards = new Item();
		shards.SetDefaults(ModContent.ItemType<CommonLifeShard>());
		shards.stack = pending;

		bool absorbedFully = false;
		if (Main.netMode == Terraria.ID.NetmodeID.SinglePlayer)
		{
			absorbedFully = Player.GetModPlayer<LifeShardPlayer>().AbsorbShards(shards);
		}

		GetShardRates(out _, out _, out int profit);

		if (profit > 0)
		{
			double ratePerTerrariaDay = profit;
			int actuallyClaimed = pending - shards.stack;

			if (actuallyClaimed > 0)
			{
				TimeSpan elapsed = DateTime.UtcNow - new DateTime(IdleShardWorld.LastClaimTimeTicks);
				double elapsedTerrariaDays = elapsed.TotalMinutes / 24.0;
				double shardsGenerated = elapsedTerrariaDays * ratePerTerrariaDay;
				int cap = GetCapacity();

				if (shardsGenerated > cap)
				{
					double excessShards = shardsGenerated - cap;
					double excessDays = excessShards / ratePerTerrariaDay;
					IdleShardWorld.LastClaimTimeTicks += TimeSpan.FromMinutes(excessDays * 24.0).Ticks;
				}

				double terrariaDaysConsumed = actuallyClaimed / ratePerTerrariaDay;
				IdleShardWorld.LastClaimTimeTicks += TimeSpan.FromMinutes(terrariaDaysConsumed * 24.0).Ticks;
				
				if (!absorbedFully && shards.stack > 0)
				{
					if (Main.netMode == Terraria.ID.NetmodeID.Server)
						Item.NewItem(Player.GetSource_Misc("IdleClaim"), Player.Center, shards.type, shards.stack);
					else
						Player.QuickSpawnItem(Player.GetSource_Misc("IdleClaim"), shards, shards.stack);
				}

				if (Main.netMode == Terraria.ID.NetmodeID.Server)
				{
					ModPacket packet = Mod.GetPacket();
					packet.Write((byte)MessageType.SyncIdleShardTime);
					packet.Write(IdleShardWorld.LastClaimTimeTicks);
					packet.Send();
				}
			}
		}
	}

	/// <summary>
	/// The Piggy Bank Heart's "Sell" action: cash the whole banked balance out as coins (a flat
	/// 1 gold per shard) instead of claiming shard items. Coins are always accepted, so unlike
	/// <see cref="ClaimShards"/> the entire pending amount clears in one go. Server-authoritative —
	/// a multiplayer client just asks the server to run it.
	/// </summary>
	public void SellShards()
	{
		if (Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient)
		{
			ModPacket packet = Mod.GetPacket();
			packet.Write((byte)MessageType.SellIdleShards);
			packet.Send();
			return;
		}

		// The heart is what turns the bank into spendable coin; without it equipped there is
		// nothing to sell. (The server re-checks here so a client can't forge the cash-out.)
		if (!PiggyBankPlayer.IsActive(Player))
			return;

		int pending = GetPendingShards();
		if (pending <= 0)
			return;

		GetShardRates(out _, out _, out int profit);
		if (profit <= 0)
			return;

		double ratePerTerrariaDay = profit;

		// Burn the elapsed time the same way ClaimShards does: first any time the bank spent
		// overflowing its cap (otherwise it instantly refills), then the time worth the shards we
		// just cashed out.
		TimeSpan elapsed = DateTime.UtcNow - new DateTime(IdleShardWorld.LastClaimTimeTicks);
		double elapsedTerrariaDays = elapsed.TotalMinutes / 24.0;
		double shardsGenerated = elapsedTerrariaDays * ratePerTerrariaDay;
		int cap = GetCapacity();

		if (shardsGenerated > cap)
		{
			double excessDays = (shardsGenerated - cap) / ratePerTerrariaDay;
			IdleShardWorld.LastClaimTimeTicks += TimeSpan.FromMinutes(excessDays * 24.0).Ticks;
		}

		double daysConsumed = pending / ratePerTerrariaDay;
		IdleShardWorld.LastClaimTimeTicks += TimeSpan.FromMinutes(daysConsumed * 24.0).Ticks;

		GiveCoins((long)pending * PiggyBankPlayer.ShardCoinValue);

		if (Main.netMode == Terraria.ID.NetmodeID.Server)
		{
			ModPacket packet = Mod.GetPacket();
			packet.Write((byte)MessageType.SyncIdleShardTime);
			packet.Write(IdleShardWorld.LastClaimTimeTicks);
			packet.Send();
		}
	}

	/// <summary>Drops a copper-valued payout next to the player as real coin items, biggest
	/// denomination first, so it merges into the wallet exactly like vendor takings.</summary>
	private void GiveCoins(long copper)
	{
		(int type, int unit)[] coins =
		{
			(Terraria.ID.ItemID.PlatinumCoin, 1_000_000),
			(Terraria.ID.ItemID.GoldCoin, 10_000),
			(Terraria.ID.ItemID.SilverCoin, 100),
			(Terraria.ID.ItemID.CopperCoin, 1),
		};

		foreach (var (type, unit) in coins)
		{
			if (copper < unit)
				continue;

			int stack = (int)(copper / unit);
			copper -= (long)stack * unit;

			if (Main.netMode == Terraria.ID.NetmodeID.Server)
				Item.NewItem(Player.GetSource_Misc("PiggyBankSell"), Player.Center, type, stack);
			else
				Player.QuickSpawnItem(Player.GetSource_Misc("PiggyBankSell"), type, stack);
		}
	}
}
