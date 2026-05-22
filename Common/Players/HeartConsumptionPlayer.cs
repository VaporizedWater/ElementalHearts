using System.Collections.Generic;
using System.Linq;
using ElementalHearts.Common.Hearts;
using ElementalHearts.Common.LifeShards;
using ElementalHearts.Common.Systems;
using ElementalHearts.Content.Items.Potions;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

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

	/// <summary>Cached sum of HP bonuses applicable in the current world.</summary>
	private int _bonus;

	private static string WorldPrefix => $"{Main.ActiveWorldFileData.UniqueId:N}|";
	private static string WorldKey(string heartId) => WorldPrefix + heartId;

	/// <summary>
	/// Apply any hearts consumed in the current world that this character hasn't yet
	/// received HP from. Coalesces the heal into a single <see cref="Player.HealEffect"/>
	/// call so late-joining players don't get a flurry of stacked popups.
	/// </summary>
	public void ReconcileWorldHp()
	{
		if (Player.whoAmI != Main.myPlayer)
			return;

		int gained = 0;
		foreach (string id in HeartConsumptionWorld.Consumed)
		{
			if (WorldHpApplied.Add(WorldKey(id)))
			{
				int hp = HeartRegistry.GetHp(id);
				_bonus += hp;
				gained += hp;
			}
		}

		if (gained <= 0)
			return;

		Player.statLife += gained;
		// broadcast: false — every client independently reconciles, so a broadcast
		// from each one would N²-multiply the popup across players.
		Player.HealEffect(gained, broadcast: false);
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
		_bonus = 0;
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
		string prefix = WorldPrefix;
		foreach (string key in WorldHpApplied)
		{
			if (!key.StartsWith(prefix))
				continue;

			string heartId = key[prefix.Length..];
			if (HeartConsumptionWorld.IsConsumed(heartId))
				_bonus += HeartRegistry.GetHp(heartId);
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
		// `Base` is a flat add before multipliers — exactly what life-crystal-style HP gain should do.
		health = StatModifier.Default with { Base = _bonus };

		// An active Animating Potion raises overall max life, and — more strongly — the
		// share of it that comes from elemental hearts (the live heart bonus, _bonus).
		if (_animatingPotionTier >= 0)
		{
			var tier = (LifeShardTier)_animatingPotionTier;
			health = health with { Base = health.Base + (_bonus * AnimatingPotion.GetElementalLifePercent(tier)) };
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
	}

	public override void LoadData(TagCompound tag)
	{
		WorldHpApplied.Clear();
		if (tag.ContainsKey("worldApplied"))
		{
			foreach (string id in tag.GetList<string>("worldApplied"))
				WorldHpApplied.Add(id);
		}
		// _bonus is recomputed in OnEnterWorld once the active world is known.
	}
}
