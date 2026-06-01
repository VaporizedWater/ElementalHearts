using ElementalHearts.Common.Configs;
using ElementalHearts.Content.Tiles.Vital;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Items.Placeable;

/// <summary>
/// Placeable item form of <see cref="VitalQuartzTile"/>. Also accepted by the Extractinator —
/// yields shards, gems, jungle herbs, vines/stingers, and (in hardmode) rare Life Fruit.
/// </summary>
public sealed class VitalQuartzItem : ModItem
{
	public override bool IsLoadingEnabled(Mod mod) => ElementalHeartsServerConfig.Instance.VitalTiles.SystemEnabled;

	public override void SetStaticDefaults()
	{
		Item.ResearchUnlockCount = 100;
		ItemID.Sets.ExtractinatorMode[Type] = Type;
	}

	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<VitalQuartzTile>());
		Item.width = 14;
		Item.height = 14;
		Item.value = Item.sellPrice(copper: 50);
		Item.rare = ItemRarityID.White;
	}

	public override void ExtractinatorUse(int extractinatorBlockType, ref int resultType, ref int resultStack)
	{
		Common.Items.VitalExtractinatorRolls.RollVitalQuartz(ref resultType, ref resultStack);
	}
}
