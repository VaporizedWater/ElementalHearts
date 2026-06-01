using ElementalHearts.Common.Configs;
using ElementalHearts.Common.Players;
using ElementalHearts.Common.Systems;
using ElementalHearts.Content.Items.Vanilla.Exotic;
using Terraria;

namespace ElementalHearts.Common.Dash;

/// <summary>
/// Shared facts about the dash-burst ability unlocked by the Jack-O'-Lantern Heart: which heart
/// grants it, whether it is currently available/active for the local player, and how hard the
/// lantern hits at the current point in progression. The actual "fire a lantern behind every dash"
/// behaviour lives in <see cref="JackOLanternDashPlayer"/>; this is the small piece that player and
/// any UI both read so they agree on the rules — mirrors <see cref="Common.Camera.CursorFocus"/>.
/// </summary>
internal static class DashUpgrade
{
	/// <summary>
	/// <see cref="Content.Items.Hearts.ElementalHeartItem.ConsumptionId"/> of the heart that unlocks
	/// the ability — the Jack-O'-Lantern Heart, whose elemental power is <c>all hallows</c>.
	/// </summary>
	public const string HeartId = nameof(JackOLanternHeart);

	/// <summary>
	/// Whether the local character has unlocked the ability, honouring the world's shared-vs-local
	/// progression setting exactly the way the Heart Log does.
	/// </summary>
	public static bool IsUnlocked()
	{
		Player player = Main.LocalPlayer;
		if (player is null)
			return false;

		return ElementalHeartsServerConfig.Instance.WorldGen.SharedProgression
			? HeartConsumptionWorld.IsUnlocked(HeartId)
			: player.GetModPlayer<HeartConsumptionPlayer>().IsUnlockedLocally(HeartId);
	}

	/// <summary>
	/// Whether a dash should spawn a lantern this frame: unlocked, switched on for this character,
	/// and not globally disabled in the config.
	/// </summary>
	public static bool IsActive()
	{
		if (Main.LocalPlayer is not { active: true } player)
			return false;

		return IsUnlocked() && player.GetModPlayer<JackOLanternDashPlayer>().Enabled;
	}

	/// <summary>
	/// Damage of the lantern, climbing in steps with world progression so the dash burst stays
	/// "slightly useful" from pre-boss all the way to post-Moon-Lord without ever out-scaling the
	/// weapons the player is actually building around. Read at spawn time so it always reflects the
	/// current world state.
	/// </summary>
	public static int GetProjectileDamage()
	{
		int damage = 10; // pre-boss: chip damage on early fodder

		if (NPC.downedBoss1 || NPC.downedBoss2 || NPC.downedBoss3 || NPC.downedQueenBee)
			damage = 22;
		if (Main.hardMode)
			damage = 45;
		if (NPC.downedMechBossAny)
			damage = 70;
		if (NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3)
			damage = 90;
		if (NPC.downedPlantBoss)
			damage = 130;
		if (NPC.downedGolemBoss)
			damage = 170;
		if (NPC.downedAncientCultist)
			damage = 210;
		if (NPC.downedMoonlord)
			damage = 260;

		return damage;
	}
}
