using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria;
using ElementalHearts.Common.Players;
using ElementalHearts.Content.Items.Hearts;

namespace ElementalHearts.Content.Items.Vanilla.Uncommon;

public sealed class EncumberingHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;

	public override int HpGain => 0;

	public override bool IsActiveAbility => true;

	public override int? ActiveAbilityDailyCost => 3;

	public override bool IsAbilityEnabled => Main.LocalPlayer.GetModPlayer<EncumberingAbilityPlayer>().Enabled;

	public override void SetAbilityEnabled(bool enabled) =>
		Main.LocalPlayer.GetModPlayer<EncumberingAbilityPlayer>().Enabled = enabled;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.EncumberingStone, 1)
			.AddOptionalIngredient(ModContent.ItemType<UncommonLifeShard>(), 3)
			.AddTile(TileID.Anvils)
			.Register();
	}
}
