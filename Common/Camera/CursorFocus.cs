using ElementalHearts.Common.Configs;
using ElementalHearts.Common.Players;
using ElementalHearts.Common.Systems;
using ElementalHearts.Content.Items.Hearts;

using Terraria;
using ElementalHearts.Content.Items.Vanilla.Exotic;
using ElementalHearts.Content.Items.Vanilla.Exotic;
using Terraria.ModLoader;

namespace ElementalHearts.Common.Camera;

/// <summary>
/// Shared facts about the Cursor Focus camera ability: which heart grants it and whether it is
/// currently available/active for the local player. The actual panning lives in
/// <see cref="CursorFocusSystem"/> / <see cref="CursorFocusModifier"/>; this is the small piece
/// both that system and the Heart Log UI need to agree on.
/// </summary>
internal static class CursorFocus
{
	/// <summary>
	/// <see cref="ElementalHeartItem.ConsumptionId"/> of the heart that unlocks the ability — the
	/// Magnification ("Lens") Heart, whose elemental power is <c>focus</c>.
	/// </summary>
	public const string HeartId = nameof(MagnificationHeart);

	/// <summary>True for the one heart that grants the Cursor Focus ability.</summary>
	public static bool IsFocusHeart(ElementalHeartItem heart) => heart is MagnificationHeart;

	/// <summary>
	/// Whether the local character has unlocked the ability, honouring the world's shared-vs-local
	/// progression setting exactly the way the Heart Log does.
	/// </summary>
	public static bool IsUnlocked()
	{
		Player player = Main.LocalPlayer;
		if (player is null)
			return false;

		return ElementalHeartsWorldConfig.Instance.SharedProgression
			? HeartConsumptionWorld.IsUnlocked(HeartId)
			: player.GetModPlayer<HeartConsumptionPlayer>().IsUnlockedLocally(HeartId);
	}

	/// <summary>
	/// Whether the camera should be panning this frame: unlocked, switched on for this character,
	/// and not globally disabled in the config.
	/// </summary>
	public static bool IsActive()
	{
		if (!ElementalHeartsCameraConfig.Instance.EnableCursorFocus)
			return false;

		if (Main.LocalPlayer is not { active: true } player)
			return false;

		return IsUnlocked() && player.GetModPlayer<CursorFocusPlayer>().Enabled;
	}
}
