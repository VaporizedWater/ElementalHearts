using System.Collections.Generic;
using ElementalHearts.Content.Items.Hearts;
using Terraria.ModLoader;

namespace ElementalHearts.Common.Hearts;

/// <summary>
/// Maps a consumed potion heart's <see cref="ElementalHeartItem.ConsumptionId"/> to the
/// vanilla BuffID it should keep active on every player. Built once in
/// <see cref="ElementalHearts.PostSetupContent"/>, then read each tick by
/// <see cref="Common.Players.PotionHeartEffectsPlayer"/>.
/// </summary>
public static class PotionHeartRegistry
{
	private static readonly Dictionary<string, int> _buffByHeartId = new();

	public static void Build()
	{
		_buffByHeartId.Clear();
		foreach (PotionHeartItem heart in ModContent.GetContent<PotionHeartItem>())
			_buffByHeartId[heart.ConsumptionId] = heart.BuffType;
	}

	public static bool TryGetBuff(string heartId, out int buffType)
		=> _buffByHeartId.TryGetValue(heartId, out buffType);
}
