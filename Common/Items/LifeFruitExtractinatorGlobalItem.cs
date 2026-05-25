using ElementalHearts.Common.Configs;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Common.Items;

/// <summary>
/// Makes vanilla Life Fruit accepted by the Extractinator while the Vital Tiles system is
/// enabled, and rolls a chance to yield a Life Fruit Seed when one is crushed. Acceptance
/// is wired in <see cref="VitalTilesSystem.SetLifeFruitExtractable"/> so the toggle stays
/// in sync with the config.
/// </summary>
public sealed class LifeFruitExtractinatorGlobalItem : GlobalItem
{
	public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
	{
		if (item.type != ItemID.LifeFruit || !VitalTilesConfig.Instance.SystemEnabled)
			return;

		tooltips.Add(new TooltipLine(Mod, "LifeFruitExtractinator",
			"Can be crushed in the Extractinator for a chance at seeds"));
	}

	public override void ExtractinatorUse(int extractType, int extractinatorBlockType, ref int resultType, ref int resultStack)
	{
		if (extractType != ItemID.LifeFruit)
			return;

		VitalTilesConfig cfg = VitalTilesConfig.Instance;
		if (!cfg.SystemEnabled)
			return;

		// Seed roll. Life Fruit extractination has no shard primary — if the seed roll
		// misses, the player still consumes a Life Fruit and gets nothing, matching
		// vanilla silt's "sometimes nothing" feel.
		if (Main.netMode == NetmodeID.Server)
			return;
		if (Main.rand.NextFloat() >= cfg.LifeFruitSeedChance / 100f)
			return;

		resultType = ModContent.ItemType<Content.Items.Tiles.LifeFruitSeedItem>();
		resultStack = 1;
	}
}
