using ElementalHearts.Common.Configs;
using ElementalHearts.Content.Tiles.Vital;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Items.Placeable;

/// <summary>
/// Placeable item form of <see cref="VitalSoilTile"/>. Also accepted by the Extractinator —
/// yields shards, gems, herbs, and a rare Life Crystal — wired via
/// <see cref="VitalSoilExtractinatorUse"/> below.
/// </summary>
public sealed class VitalSoilItem : ModItem
{
	public override bool IsLoadingEnabled(Mod mod) => VitalTilesConfig.Instance.SystemEnabled;

	public override void SetStaticDefaults()
	{
		Item.ResearchUnlockCount = 100;
		ItemID.Sets.ExtractinatorMode[Type] = Type;
	}

	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<VitalSoilTile>());
		Item.width = 14;
		Item.height = 14;
		Item.value = Item.sellPrice(copper: 30);
		Item.rare = ItemRarityID.White;
	}

	public override void ExtractinatorUse(int extractinatorBlockType, ref int resultType, ref int resultStack)
	{
		Common.Items.VitalExtractinatorRolls.RollVitalSoil(ref resultType, ref resultStack);
	}
}
