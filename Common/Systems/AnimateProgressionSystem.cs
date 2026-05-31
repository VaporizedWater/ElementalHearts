using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using ElementalHearts.Common.Hearts;
using ElementalHearts.Common.LifeShards;

namespace ElementalHearts.Common.Systems;

public sealed class AnimateProgressionSystem : ModSystem
{
	// 0 = Common, 1 = Uncommon, 2 = Rare, 3 = Epic, 4 = Legendary, 5 = Mythic
	public static int UnlockedTier { get; private set; } = 0;

	/// <summary>
	/// The world's progression tier as one of the six <em>world</em> tiers — the ladder Animate
	/// guards. This is deliberately <b>not</b> the full <see cref="HeartTier"/> set: world tier
	/// skips <see cref="HeartTier.Exotic"/> (a cross-mod heart rarity, not a progression gate), so
	/// <see cref="UnlockedTier"/>'s 0…5 index maps Common → Uncommon → Rare → Epic → Legendary →
	/// Mythic. Use this anywhere the player-facing world tier is shown; never read a consumed
	/// heart's tier for it (that can be Exotic and isn't what Animate unlocked).
	/// </summary>
	public static HeartTier CurrentWorldTier => WorldTiers[Math.Clamp(UnlockedTier, 0, WorldTiers.Length - 1)];

	private static readonly HeartTier[] WorldTiers =
	{
		HeartTier.Common, HeartTier.Uncommon, HeartTier.Rare,
		HeartTier.Epic, HeartTier.Legendary, HeartTier.Mythic,
	};

	public static Condition DownedLegendaryAnimate { get; private set; }

	public override void Load()
	{
		DownedLegendaryAnimate = new Condition("Mods.ElementalHearts.Conditions.DownedLegendaryAnimate", () => UnlockedTier >= 5);
	}

	public override void Unload()
	{
		DownedLegendaryAnimate = null;
	}

	public static void UnlockTier(int tier)
	{
		if (tier > UnlockedTier)
		{
			UnlockedTier = tier;
			if (Main.netMode == Terraria.ID.NetmodeID.Server)
			{
				NetMessage.SendData(Terraria.ID.MessageID.WorldData); // Sync world data
			}
		}
	}

	public static void ClearTier()
	{
		UnlockedTier = 0;
		if (Main.netMode == Terraria.ID.NetmodeID.Server)
		{
			NetMessage.SendData(Terraria.ID.MessageID.WorldData); // Sync world data
		}
	}

	public static void AdvanceTier()
	{
		if (UnlockedTier < 5)
		{
			UnlockedTier++;
			if (Main.netMode == Terraria.ID.NetmodeID.Server)
			{
				NetMessage.SendData(Terraria.ID.MessageID.WorldData);
			}
		}
	}

	public override void ClearWorld()
	{
		UnlockedTier = 0;
	}

	public override void SaveWorldData(TagCompound tag)
	{
		tag["AnimateTier"] = UnlockedTier;
	}

	public override void LoadWorldData(TagCompound tag)
	{
		UnlockedTier = tag.GetInt("AnimateTier");
	}

	public override void NetSend(BinaryWriter writer)
	{
		writer.Write((byte)UnlockedTier);
	}

	public override void NetReceive(BinaryReader reader)
	{
		UnlockedTier = reader.ReadByte();
	}
}
