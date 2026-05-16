using System.Collections.Generic;
using System.Linq;
using ElementalHearts.Common.Systems;
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

	/// <summary>Cached sum of HP bonuses applicable in the current world. Recomputed on world enter / consume.</summary>
	private int _bonus;

	private static string WorldPrefix => $"{Main.ActiveWorldFileData.UniqueId:N}|";
	private static string WorldKey(string heartId) => WorldPrefix + heartId;

	/// <summary>
	/// Apply any hearts consumed in the current world that this character hasn't yet received HP from.
	/// </summary>
	public void ReconcileWorldHp()
	{
		if (Player.whoAmI != Main.myPlayer)
			return;

		foreach (var (id, hp) in HeartConsumptionWorld.Consumed)
		{
			if (WorldHpApplied.Add(WorldKey(id)))
			{
				_bonus += hp;
				Player.statLife += hp;
				Player.HealEffect(hp, broadcast: true);
			}
		}
	}

	private void RecomputeBonus()
	{
		_bonus = 0;
		string prefix = WorldPrefix;
		foreach (string key in WorldHpApplied)
		{
			if (!key.StartsWith(prefix))
				continue;

			string heartId = key[prefix.Length..];
			if (HeartConsumptionWorld.Consumed.TryGetValue(heartId, out int hp))
				_bonus += hp;
		}
	}

	public override void ModifyMaxStats(out StatModifier health, out StatModifier mana)
	{
		// `Base` is a flat add before multipliers — exactly what life-crystal-style HP gain should do.
		health = StatModifier.Default with { Base = _bonus };
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
		// Bonus is recomputed in OnEnterWorld once the active world is known.
	}
}
