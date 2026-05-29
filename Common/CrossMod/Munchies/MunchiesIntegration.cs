using System;
using ElementalHearts.Common.Hearts;
using ElementalHearts.Common.Systems;
using ElementalHearts.Content.Items.Hearts;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ElementalHearts.Common.CrossMod.Munchies;

/// <summary>
/// Registers every loaded <see cref="ElementalHeartItem"/> with the Munchies mod's
/// checklist (see <c>Munchies.md</c>). Hearts are permanent one-shot consumables that
/// flip a bit in <see cref="HeartConsumptionWorld"/>, which maps cleanly onto Munchies'
/// "AddSingleConsumable" call shape.
///
/// The integration is intentionally data-driven: per-heart presentation is read from
/// virtual hooks on <see cref="ElementalHeartItem"/> (<see cref="ElementalHeartItem.MunchiesDifficulty"/>,
/// <see cref="ElementalHeartItem.MunchiesTextColor"/>, <see cref="ElementalHeartItem.MunchiesAvailability"/>,
/// <see cref="ElementalHeartItem.MunchiesAcquisitionText"/>, <see cref="ElementalHeartItem.MunchiesExtraTooltip"/>),
/// so new hearts get listed automatically with no edits to this file.
/// </summary>
internal static class MunchiesIntegration
{
	/// <summary>
	/// Pinned Munchies call-API version. Per the Munchies README this MUST be a string
	/// literal — do not derive it from the loaded mod's version, or backwards compat
	/// breaks the moment Munchies updates.
	/// </summary>
	private const string CallApiVersion = "1.3";

	private const string MunchiesModName = "Munchies";
	private const string CategoryPlayer = "player";
	private const string AddSingleConsumable = "AddSingleConsumable";

	public static void Register(Mod elementalHearts)
	{
		if (!ModLoader.TryGetMod(MunchiesModName, out Mod munchies))
			return;

		try
		{
			foreach (ElementalHeartItem heart in ModContent.GetContent<ElementalHeartItem>())
				RegisterHeart(elementalHearts, munchies, heart);
		}
		catch (Exception e)
		{
			elementalHearts.Logger.Error($"Failed to register hearts with Munchies: {e.Message}\n{e.StackTrace}");
		}
	}

	private static void RegisterHeart(Mod elementalHearts, Mod munchies, ElementalHeartItem heart)
	{
		// Capture the id so the closure stays cheap and doesn't hold the heart instance.
		string id = heart.ConsumptionId;

		object[] args =
		{
			AddSingleConsumable,
			elementalHearts,
			CallApiVersion,
			heart,
			CategoryPlayer,
			(Func<bool>)(() => HeartConsumptionWorld.IsConsumed(id)),
			(Color?)heart.MunchiesTextColor,
			heart.MunchiesDifficulty,
			heart.MunchiesExtraTooltip,
			heart.MunchiesAvailability,
			heart.MunchiesAcquisitionText,
		};

		munchies.Call(args);
	}
}
