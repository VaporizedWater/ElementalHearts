using System;
using System.Collections.Generic;
using ElementalHearts.Common.Systems;
using ElementalHearts.Content.Items.BossSpawns;
using ElementalHearts.Content.NPCs.Bosses.Animate;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ElementalHearts.Common.CrossMod.BossChecklist;

/// <summary>
/// Two-part BossChecklist integration:
///
///  1. <see cref="LogAnimateBosses"/> registers this mod's five Animate bosses
///     (Common → Legendary) as full <c>LogBoss</c> entries with progression values,
///     downed booleans (read from <see cref="AnimateProgressionSystem"/>),
///     spawn items (the corresponding Menacing Heart), and collectible hearts.
///
///  2. <see cref="SubmitCollectiblesToOtherBosses"/> walks every (boss NPC → heart
///     item) mapping that <see cref="BossHeartDropRegistry"/> already knows about
///     and submits each heart as a collectible on the matching vanilla / Calamity /
///     Thorium / Consolaria entry. This is data-driven: new boss hearts added to
///     <see cref="BossHeartDropRegistry"/> automatically show up in the boss log
///     without edits here.
///
/// BossChecklist itself silently ignores submissions whose entry key isn't
/// registered (e.g. the user doesn't have Calamity loaded, or BossChecklist's
/// internal name for a boss changes in a future version), so the call is safe
/// even when only a subset of cross-mod targets are loaded.
/// </summary>
internal static class BossChecklistIntegration
{
	private const string BossChecklistModName = "BossChecklist";
	private const string LogBoss = "LogBoss";
	private const string SubmitEntryCollectibles = "SubmitEntryCollectibles";

	private const string LocalizationRoot = "Mods.ElementalHearts.BossChecklist";

	public static void Register(Mod elementalHearts)
	{
		if (!ModLoader.TryGetMod(BossChecklistModName, out Mod bossChecklist))
			return;

		try
		{
			LogAnimateBosses(elementalHearts, bossChecklist);
			SubmitCollectiblesToOtherBosses(elementalHearts, bossChecklist);
		}
		catch (Exception e)
		{
			elementalHearts.Logger.Error($"Failed to register with BossChecklist: {e.Message}\n{e.StackTrace}");
		}
	}

	// ── Part 1: Log the Animate boss family ──────────────────────────────────

	private static void LogAnimateBosses(Mod mod, Mod bossChecklist)
	{
		// DESIGN INTENT FOR ANIMATE BOSS PROGRESSION TIERS:
		// These progression values ensure Boss Checklist understands exactly where
		// the Animate bosses are intended to be fought in the vanilla progression:
		//   - Common Animate: Pre-Boss (1.5f, before Eye of Cthulhu)
		//   - Uncommon Animate: Pre-Skeletron (4.5f, before Skeletron)
		//   - Rare Animate: Pre-Wall of Flesh (6.5f, before Wall of Flesh)
		//   - Epic Animate: Pre-Plantera (12.5f, before Plantera)
		//   - Legendary Animate: Pre-Moon Lord (18.5f, before Moon Lord)
		//
		// We do not strictly hardcode vanilla boss defeats into the crafting recipes,
		// but these values place them in the correct Boss Checklist order.

		LogAnimate<CommonAnimate, CommonMenacingHeart, Content.Items.Vanilla.Pacified.CommonPacifiedHeart>(
			mod, bossChecklist, tier: 0, progression: 1.5f);

		LogAnimate<UncommonAnimate, UncommonMenacingHeart, Content.Items.Vanilla.Pacified.UncommonPacifiedHeart>(
			mod, bossChecklist, tier: 1, progression: 4.5f);

		LogAnimate<RareAnimate, RareMenacingHeart, Content.Items.Vanilla.Pacified.RarePacifiedHeart>(
			mod, bossChecklist, tier: 2, progression: 6.5f);

		LogAnimate<EpicAnimate, EpicMenacingHeart, Content.Items.Vanilla.Pacified.EpicPacifiedHeart>(
			mod, bossChecklist, tier: 3, progression: 12.5f);

		LogAnimate<LegendaryAnimate, LegendaryMenacingHeart, Content.Items.Vanilla.Pacified.LegendaryPacifiedHeart>(
			mod, bossChecklist, tier: 4, progression: 18.5f);
	}

	private static void LogAnimate<TBoss, TSpawnItem, TCollectibleHeart>(
		Mod mod,
		Mod bossChecklist,
		int tier,
		float progression)
		where TBoss : ModNPC
		where TSpawnItem : ModItem
		where TCollectibleHeart : ModItem
	{
		string internalName = typeof(TBoss).Name;
		int npcType = ModContent.NPCType<TBoss>();
		int spawnItem = ModContent.ItemType<TSpawnItem>();
		int collectible = ModContent.ItemType<TCollectibleHeart>();

		// The defeat flag advances when ProgressionTier+1 is unlocked (see
		// AnimateBoss.OnKill → AnimateProgressionSystem.UnlockTier).
		Func<bool> downed = () => AnimateProgressionSystem.UnlockedTier > tier;

		LocalizedText spawnInfo = Language.GetOrRegister(
			$"{LocalizationRoot}.{internalName}.SpawnInfo",
			() => $"Use a [i:{spawnItem}] anywhere. The Menacing Heart can only be crafted once the previous Animate tier has been defeated.");

		LocalizedText despawnMessage = Language.GetOrRegister(
			$"{LocalizationRoot}.{internalName}.DespawnMessage",
			() => "{0} fades from this world, its menace unanswered.");

		Dictionary<string, object> extras = new()
		{
			["spawnItems"] = spawnItem,
			["collectibles"] = new List<int> { collectible },
			["despawnMessage"] = despawnMessage,
			["spawnInfo"] = spawnInfo,
		};

		bossChecklist.Call(LogBoss, mod, internalName, progression, downed, npcType, extras);
	}

	// ── Part 2: Hand boss hearts to existing entries as collectibles ─────────

	private static void SubmitCollectiblesToOtherBosses(Mod mod, Mod bossChecklist)
	{
		// Keyed by BossChecklist entry key string ("ModSource InternalName"); value
		// is the list of heart item types this mod considers collectible from that
		// entry. We group up front so we can send one batched SubmitEntryCollectibles
		// call instead of one per boss.
		Dictionary<string, object> submissions = new();

		foreach (KeyValuePair<int, List<int>> mapping in BossHeartDropRegistry.AllDrops)
		{
			int npcType = mapping.Key;
			List<int> hearts = mapping.Value;
			if (hearts is null || hearts.Count == 0)
				continue;

			// Skip the Animate bosses here — they get their collectibles set inline
			// via the LogBoss call above. Submitting again would be harmless but
			// noisy in BossChecklist's internal lists.
			if (IsAnimateBoss(npcType))
				continue;

			string entryKey = TryGetEntryKey(npcType);
			if (entryKey is null)
				continue;

			// If a single boss maps to multiple hearts (e.g. Calamity Anahita /
			// Leviathan share entries, vanilla Everscream drops two hearts), merge
			// rather than overwrite so all of them surface as collectibles.
			if (submissions.TryGetValue(entryKey, out object existing) && existing is List<int> existingList)
			{
				foreach (int item in hearts)
				{
					if (!existingList.Contains(item))
						existingList.Add(item);
				}
			}
			else
			{
				submissions[entryKey] = new List<int>(hearts);
			}
		}

		if (submissions.Count == 0)
			return;

		bossChecklist.Call(SubmitEntryCollectibles, mod, submissions);
	}

	private static bool IsAnimateBoss(int npcType) =>
		npcType == ModContent.NPCType<CommonAnimate>()
		|| npcType == ModContent.NPCType<UncommonAnimate>()
		|| npcType == ModContent.NPCType<RareAnimate>()
		|| npcType == ModContent.NPCType<EpicAnimate>()
		|| npcType == ModContent.NPCType<LegendaryAnimate>();

	/// <summary>
	/// Maps a runtime NPC type to its BossChecklist entry key. For vanilla NPCs
	/// the format is <c>"Terraria " + InternalName</c>; for modded NPCs it is
	/// <c>"ModSourceName " + InternalName</c>. A handful of vanilla bosses use
	/// an entry-name override (e.g. <c>MoonLordCore → "MoonLord"</c>) — those
	/// are listed in <see cref="VanillaEntryKeyOverrides"/>.
	/// </summary>
	private static string TryGetEntryKey(int npcType)
	{
		if (npcType < NPCID.Count)
		{
			if (VanillaEntryKeyOverrides.TryGetValue(npcType, out string overrideName))
				return $"Terraria {overrideName}";

			string vanillaName = NPCID.Search.GetName(npcType);
			return string.IsNullOrEmpty(vanillaName) ? null : $"Terraria {vanillaName}";
		}

		ModNPC modNpc = NPCLoader.GetNPC(npcType);
		if (modNpc?.Mod is null)
			return null;

		return $"{modNpc.Mod.Name} {modNpc.Name}";
	}

	/// <summary>
	/// BossChecklist uses internal entry names that occasionally differ from the
	/// <see cref="NPCID"/> field name — most notably head-versus-body NPCs and
	/// Moon Lord's core. Anything not in this table falls back to the raw NPCID
	/// field name, which is what BossChecklist uses for the rest of vanilla.
	/// </summary>
	private static readonly Dictionary<int, string> VanillaEntryKeyOverrides = new()
	{
		{ NPCID.MoonLordCore, "MoonLord" },
		{ NPCID.QueenSlimeBoss, "QueenSlimeBoss" },
		{ NPCID.HallowBoss, "HallowBoss" },
		{ NPCID.CultistBoss, "CultistBoss" },
		{ NPCID.DD2Betsy, "DD2Betsy" },
		{ NPCID.PirateShip, "PirateShip" },
		{ NPCID.SkeletronHead, "Skeletron" },
	};
}
