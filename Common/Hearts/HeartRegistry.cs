using System.Collections.Generic;
using ElementalHearts.Content.Items.Hearts;
using Terraria.ModLoader;

namespace ElementalHearts.Common.Hearts;

/// <summary>
/// Resolves a heart's stable <see cref="ElementalHeartItem.ConsumptionId"/> to its
/// definition. Consumed hearts are only stored as ids, so HP is always read live from
/// here — meaning a change to the HP config retroactively updates every character.
/// Built once in <see cref="ElementalHearts.PostSetupContent"/>.
/// </summary>
public static class HeartRegistry
{
	private static readonly Dictionary<string, ElementalHeartItem> _byId = new();

	public static void Build()
	{
		_byId.Clear();
		foreach (ElementalHeartItem heart in ModContent.GetContent<ElementalHeartItem>())
			_byId[heart.ConsumptionId] = heart;
	}

	/// <summary>
	/// HP a consumed heart currently grants, read live from the HP config. Returns 0 for
	/// an unknown id (e.g. a heart whose source mod is disabled) so it simply contributes
	/// nothing until it can be resolved again.
	/// </summary>
	public static int GetHp(string heartId) =>
		_byId.TryGetValue(heartId, out ElementalHeartItem heart) ? heart.HpGain : 0;

	/// <summary>
	/// Tier of a consumed heart, or null for an unknown id (source mod disabled, etc.).
	/// Used to drive the player-UI heart colour off the highest tier consumed.
	/// </summary>
	public static HeartTier? GetTier(string heartId) =>
		_byId.TryGetValue(heartId, out ElementalHeartItem heart) ? heart.Tier : null;
}
