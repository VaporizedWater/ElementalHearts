using System.Collections.Generic;
using System.Linq;
using ElementalHearts.Common.Hearts;
using ElementalHearts.Common.LifeShards;
using ElementalHearts.Common.Systems;
using ElementalHearts.Content.Items.Potions;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using ElementalHearts.Common.Configs;
using ElementalHearts.Content.Items.Hearts;

namespace ElementalHearts.Common.Players;

/// <summary>
/// Per-character ledger of which world hearts this character has already received HP from.
/// HP is granted dynamically each frame via <see cref="ModPlayer.ModifyMaxStats"/> rather
/// than written into <see cref="Player.statLifeMax"/> directly, so it can't be clobbered
/// by other systems recomputing the stat.
/// </summary>
public sealed class HeartConsumptionPlayer : ModPlayer
{
	/// <summary>
	/// Compound "worldGuid|heartId" entries this character has already received HP for.
	/// Keyed by world ID so the same heart name can grant HP independently in different worlds.
	/// </summary>
	public HashSet<string> WorldHpApplied { get; private set; } = new();

	/// <summary>
	/// Compound "worldGuid|heartId" entries this character has unlocked permanently.
	/// Kept even if the HP effect is toggled off.
	/// </summary>
	public HashSet<string> WorldUnlocked { get; private set; } = new();

	/// <summary>
	/// Compound "worldGuid|milestoneId" entries this character has claimed rewards for.
	/// </summary>
	public HashSet<string> ClaimedMilestones { get; private set; } = new();

	/// <summary>Cached sum of HP bonuses applicable in the current world.</summary>
	private int _bonus;
	public int ActiveHpBonus => _bonus;

	/// <summary>
	/// Highest <see cref="HeartTier"/> among the current world's hearts this character has
	/// been granted, or null when none. Maintained alongside <see cref="_bonus"/> and read
	/// by the life-bar overlay (<see cref="Common.UI.PlayerHeartOverlay"/>) to colour the
	/// player's UI hearts. Only meaningful for the local player.
	/// </summary>
	public HeartTier? HighestTier { get; private set; }

	/// <summary>Raises <see cref="HighestTier"/> if <paramref name="heartId"/> outranks it.</summary>
	private void BumpHighestTier(string heartId)
	{
		if (HeartRegistry.GetTier(heartId) is HeartTier tier && (HighestTier is not HeartTier current || tier > current))
			HighestTier = tier;
	}

	// WorldKey is hit on hot paths — every tooltip frame for a hovered heart (CanUseItem,
	// ModifyTooltips) and once per stored grant in RecomputeBonus. Formatting the 32-char
	// world GUID each time allocated a fresh string per call; memoize it and only reformat
	// when the active world actually changes.
	private static System.Guid _cachedWorldGuid;
	private static string? _cachedWorldPrefix;

	private static string WorldPrefix
	{
		get
		{
			System.Guid current = Main.ActiveWorldFileData.UniqueId;
			if (_cachedWorldPrefix == null || current != _cachedWorldGuid)
			{
				_cachedWorldGuid = current;
				_cachedWorldPrefix = $"{current:N}|";
			}

			return _cachedWorldPrefix;
		}
	}

	private static string WorldKey(string heartId) => WorldPrefix + heartId;

	public bool IsConsumedLocally(string heartId) => WorldHpApplied.Contains(WorldKey(heartId));
	public bool IsUnlockedLocally(string heartId) => WorldUnlocked.Contains(WorldKey(heartId));
	
	public bool IsMilestoneClaimedLocally(string milestoneId) => ClaimedMilestones.Contains(WorldKey(milestoneId));
	public void ClaimMilestoneLocally(string milestoneId) => ClaimedMilestones.Add(WorldKey(milestoneId));

	/// <summary>
	/// Apply any hearts consumed in the current world that this character hasn't yet
	/// received HP from. Coalesces the heal into a single <see cref="Player.HealEffect"/>
	/// call so late-joining players don't get a flurry of stacked popups.
	/// </summary>
	public void ReconcileWorldHp()
	{
		if (Player.whoAmI != Main.myPlayer)
			return;

		if (!ElementalHeartsServerConfig.Instance.WorldGen.SharedProgression)
			return;

		int currentApplied = System.Math.Min(_bonus, HeartCapacitySystem.GetMaxCapacity() ?? int.MaxValue);
		int oldBonus = _bonus;
		foreach (string id in HeartConsumptionWorld.Unlocked)
		{
			WorldUnlocked.Add(WorldKey(id));
		}

		foreach (string id in HeartConsumptionWorld.Consumed)
		{
			if (WorldHpApplied.Add(WorldKey(id)))
			{
				int hp = HeartRegistry.GetHp(id);
				_bonus += hp;
				BumpHighestTier(id);
			}
		}

		int newApplied = System.Math.Min(_bonus, HeartCapacitySystem.GetMaxCapacity() ?? int.MaxValue);
		int effectiveGained = newApplied - currentApplied;

		if (effectiveGained <= 0)
			return;

		Player.statLife += effectiveGained;
		// broadcast: false — every client independently reconciles, so a broadcast
		// from each one would N²-multiply the popup across players.
		Player.HealEffect(effectiveGained, broadcast: false);

		if ((oldBonus / 100) < (_bonus / 100))
		{
			PlayMilestoneFlourish();
		}
	}

	/// <summary>
	/// Drops every HP grant this character received from the current world. Called when
	/// the world's consumed-heart registry is wiped, so cleared hearts also surrender
	/// the max-HP they gave. The game clamps <see cref="Player.statLife"/> down to the
	/// new maximum on its next stat pass.
	/// </summary>
	public void ClearWorldHp()
	{
		if (Player.whoAmI != Main.myPlayer)
			return;

		string prefix = WorldPrefix;
		WorldHpApplied.RemoveWhere(key => key.StartsWith(prefix));
		WorldUnlocked.RemoveWhere(key => key.StartsWith(prefix));
		ClaimedMilestones.RemoveWhere(key => key.StartsWith(prefix));
		_bonus = 0;
		HighestTier = null;
	}

	/// <summary>
	/// Symmetrical inverse of <see cref="ReconcileWorldHp"/> for a single heart: drops
	/// the HP a toggleable heart granted this character and forgets the credit so the
	/// next consumption will grant it again. Called when a heart is deactivated locally
	/// (see <see cref="HeartConsumptionWorld.Unrecord"/>).
	/// </summary>
	public void HandleHeartDeactivated(string heartId)
	{
		if (Player.whoAmI != Main.myPlayer)
			return;

		string key = WorldKey(heartId);
		if (!WorldHpApplied.Remove(key))
			return;

		// Removing a grant can lower both the HP bonus and the highest tier, and HP is live
		// (config-dependent), so re-derive both from the remaining current-world grants
		// rather than trying to back out a single value.
		RecomputeBonus();
	}

	public bool TryConsumeLocally(ElementalHeartItem heart)
	{
		if (Player.whoAmI != Main.myPlayer)
			return false;

		string id = heart.ConsumptionId;
		WorldUnlocked.Add(WorldKey(id));
		if (WorldHpApplied.Add(WorldKey(id)))
		{
			int currentApplied = System.Math.Min(_bonus, HeartCapacitySystem.GetMaxCapacity() ?? int.MaxValue);
			int oldBonus = _bonus;
			int hp = HeartRegistry.GetHp(id);
			_bonus += hp;
			
			int newApplied = System.Math.Min(_bonus, HeartCapacitySystem.GetMaxCapacity() ?? int.MaxValue);
			int effectiveHp = newApplied - currentApplied;
			
			if (effectiveHp > 0)
			{
				Player.statLife += effectiveHp;
				Player.HealEffect(effectiveHp, broadcast: false);
			}
			BumpHighestTier(id);
			UI.Checklist.HeartLogButtonUIState.HasUnseenContent = true;

			if ((oldBonus / 100) < (_bonus / 100))
			{
				PlayMilestoneFlourish();
			}

			return true;
		}

		return false;
	}

	private void PlayMilestoneFlourish()
	{
		Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item92.WithVolumeScale(0.8f).WithPitchOffset(0.2f), Player.Center);
		ScreenFlashSystem.Flash(Microsoft.Xna.Framework.Color.Gold, 0.4f, 6f, 20f, default, 0.45f);

		for (int i = 0; i < 40; i++)
		{
			Dust dust = Dust.NewDustPerfect(
				Player.Center,
				Terraria.ID.DustID.GoldCoin,
				Main.rand.NextVector2Circular(6f, 6f),
				0, Microsoft.Xna.Framework.Color.White, 1.5f);
			dust.noGravity = true;
			dust.velocity *= 1.5f;
		}
	}

	public bool TryDeactivateLocally(ElementalHeartItem heart)
	{
		if (Player.whoAmI != Main.myPlayer)
			return false;

		string key = WorldKey(heart.ConsumptionId);
		if (!WorldHpApplied.Remove(key))
			return false;

		RecomputeBonus();
		return true;
	}

	/// <summary>
	/// Re-derives <see cref="_bonus"/> from scratch using the live HP of every
	/// current-world heart this character has been granted. Called on world enter and
	/// whenever the HP config changes, so heart bonuses always reflect current values.
	/// </summary>
	public void RecomputeBonus()
	{
		if (Player.whoAmI != Main.myPlayer)
			return;

		_bonus = 0;
		HighestTier = null;
		string prefix = WorldPrefix;
		foreach (string key in WorldHpApplied)
		{
			if (!key.StartsWith(prefix))
				continue;

			string heartId = key[prefix.Length..];
			if (ElementalHeartsServerConfig.Instance.WorldGen.SharedProgression && !HeartConsumptionWorld.IsConsumed(heartId))
				continue;

			_bonus += HeartRegistry.GetHp(heartId);
			BumpHighestTier(heartId);
		}
	}

	/// <summary>Highest Animating Potion tier active this frame, or -1 when none is.</summary>
	private int _animatingPotionTier = -1;

	/// <summary>
	/// Registers an active Animating Potion buff for this frame; called by the buff every
	/// tick it's up. If buffs of several tiers are somehow active at once the highest wins.
	/// Cleared each frame by <see cref="ResetEffects"/> before buffs re-apply it.
	/// </summary>
	public void ApplyAnimatingPotion(LifeShardTier tier)
	{
		if ((int)tier > _animatingPotionTier)
			_animatingPotionTier = (int)tier;
	}

	public override void ResetEffects()
	{
		_animatingPotionTier = -1;
	}

	/// <summary>
	/// After buffs update, keeps only the strongest Animating Potion buff by clearing every
	/// lower tier. This makes the five tiers mutually exclusive however the buff was applied
	/// — including Quick Buff, which would otherwise stack one potion buff of every tier.
	/// </summary>
	public override void PostUpdateBuffs()
	{
		for (int lower = 0; lower < _animatingPotionTier; lower++)
			Player.ClearBuff(AnimatingPotion.GetBuffType((LifeShardTier)lower));
	}

	public override void ModifyMaxStats(out StatModifier health, out StatModifier mana)
	{
		int maxCapacity = HeartCapacitySystem.GetMaxCapacity() ?? int.MaxValue;
		int appliedBonus = ElementalHeartsClientConfig.Instance.UI.EnableElementalHP ? System.Math.Min(_bonus, maxCapacity) : 0;
		// `Base` is a flat add before multipliers — exactly what life-crystal-style HP gain should do.
		health = StatModifier.Default with { Base = appliedBonus };

		// An active Animating Potion raises overall max life, and — more strongly — the
		// share of it that comes from elemental hearts (the live heart bonus, _bonus).
		if (_animatingPotionTier >= 0)
		{
			var tier = (LifeShardTier)_animatingPotionTier;
			health = health with { Base = health.Base + (appliedBonus * AnimatingPotion.GetElementalLifePercent(tier)) };
			health += AnimatingPotion.GetMaxLifePercent(tier);
		}

		mana = StatModifier.Default;
	}

	public override void OnEnterWorld()
	{
		RecomputeBonus();
		ReconcileWorldHp();
	}

	public override void SaveData(TagCompound tag)
	{
		if (WorldHpApplied.Count > 0)
			tag["worldApplied"] = WorldHpApplied.ToList();
		if (WorldUnlocked.Count > 0)
			tag["worldUnlocked"] = WorldUnlocked.ToList();
		if (ClaimedMilestones.Count > 0)
			tag["claimedMilestones"] = ClaimedMilestones.ToList();
	}

	public override void LoadData(TagCompound tag)
	{
		WorldHpApplied.Clear();
		WorldUnlocked.Clear();
		ClaimedMilestones.Clear();
		if (tag.ContainsKey("worldApplied"))
		{
			foreach (string id in tag.GetList<string>("worldApplied"))
			{
				WorldHpApplied.Add(id);
				WorldUnlocked.Add(id); // Retroactive unlock for older saves
			}
		}
		if (tag.ContainsKey("worldUnlocked"))
		{
			foreach (string id in tag.GetList<string>("worldUnlocked"))
				WorldUnlocked.Add(id);
		}
		if (tag.ContainsKey("claimedMilestones"))
		{
			foreach (string id in tag.GetList<string>("claimedMilestones"))
				ClaimedMilestones.Add(id);
		}
		// _bonus is recomputed in OnEnterWorld once the active world is known.
	}
}
