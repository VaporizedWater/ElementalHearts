using ElementalHearts.Common.Configs;
using ElementalHearts.Content.Buffs;
using ElementalHearts.Content.Tiles.Vital;
using Terraria;
using Terraria.ModLoader;

namespace ElementalHearts.Common.Players;

/// <summary>
/// Detects when a player is standing on Vital Soil or within range of any Vital Quartz
/// block, and applies the matching buff each frame. The actual stat changes live here too
/// (regen scaling in <see cref="UpdateLifeRegen"/>, max-HP scaling in
/// <see cref="ModifyMaxStats"/>) so they compose correctly with other modifiers.
/// </summary>
public sealed class VitalTilesPlayer : ModPlayer
{
	public bool VitalSoilRegenActive;
	public bool VitalQuartzAuraActive;

	public override void ResetEffects()
	{
		VitalSoilRegenActive = false;
		VitalQuartzAuraActive = false;
	}

	public override void PreUpdate()
	{
		if (!VitalTilesConfig.Instance.SystemEnabled)
			return;

		if (IsStandingOnVitalSoil())
			Player.AddBuff(ModContent.BuffType<VitalSoilRegen>(), 10);

		if (IsNearVitalQuartz())
			Player.AddBuff(ModContent.BuffType<VitalQuartzAura>(), 10);
	}

	private bool IsStandingOnVitalSoil()
	{
		int soilType = ModContent.TileType<VitalSoilTile>();

		int leftTile = (int)(Player.position.X / 16f) - 1;
		int rightTile = (int)((Player.position.X + Player.width) / 16f) + 1;
		int bottomTile = (int)((Player.position.Y + Player.height) / 16f) + 1;

		if (bottomTile < 0 || bottomTile >= Main.maxTilesY)
			return false;

		for (int x = leftTile; x <= rightTile; x++)
		{
			if (x < 0 || x >= Main.maxTilesX)
				continue;

			Tile tile = Main.tile[x, bottomTile];
			if (tile.HasTile && tile.TileType == soilType)
				return true;
		}

		return false;
	}

	private bool IsNearVitalQuartz()
	{
		VitalTilesConfig cfg = VitalTilesConfig.Instance;
		int quartzType = ModContent.TileType<VitalQuartzTile>();

		int centerX = (int)(Player.Center.X / 16f);
		int centerY = (int)(Player.Center.Y / 16f);

		int hRange = cfg.VitalQuartzHorizontalRange;
		int vRange = cfg.VitalQuartzVerticalRange;

		int x0 = System.Math.Max(0, centerX - hRange);
		int x1 = System.Math.Min(Main.maxTilesX - 1, centerX + hRange);
		int y0 = System.Math.Max(0, centerY - vRange);
		int y1 = System.Math.Min(Main.maxTilesY - 1, centerY + vRange);

		// Early-out as soon as one Vital Quartz block is found — this scan runs every
		// frame, so the budget is tight and ranges should stay modest.
		for (int x = x0; x <= x1; x++)
		{
			for (int y = y0; y <= y1; y++)
			{
				Tile tile = Main.tile[x, y];
				if (tile.HasTile && tile.TileType == quartzType)
					return true;
			}
		}

		return false;
	}

	public override void UpdateLifeRegen()
	{
		if (!VitalSoilRegenActive)
			return;

		int percent = VitalTilesConfig.Instance.VitalSoilRegenPercent;
		if (percent <= 0)
			return;

		// `lifeRegen` is measured in half-HP-per-second units. Scale relative to the
		// existing tick so the boost stays meaningful even with other regen sources.
		// A small floor (1) prevents the buff from looking like a no-op out of combat.
		int boost = System.Math.Max(1, Player.lifeRegen * percent / 100);
		Player.lifeRegen += boost;
	}

	public override void ModifyMaxStats(out StatModifier health, out StatModifier mana)
	{
		health = StatModifier.Default;
		mana = StatModifier.Default;

		if (!VitalQuartzAuraActive)
			return;

		int percent = VitalTilesConfig.Instance.VitalQuartzMaxHpPercent;
		if (percent <= 0)
			return;

		// Adds to the additive ladder alongside other +%-HP effects (Lifeforce, the
		// Animating Potions, etc.) rather than stacking multiplicatively on top.
		health += percent / 100f;
	}
}
