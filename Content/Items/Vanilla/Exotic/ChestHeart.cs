using ElementalHearts.Common.Hearts;
using ElementalHearts.Common.Players;
using ElementalHearts.Content.Items.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using ElementalHearts.Content.Items.Placeable;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Items.Vanilla.Exotic;

/// <summary>
/// Exotic Active heart: consuming it permanently unlocks an extra 10 inventory slots.
/// Grants no HP — the extra slots are the payoff. Toggled via the Heart Log checklist.
/// Costs 10 life shards / day to keep active. Toggling off is blocked if items are still
/// present in the extra inventory slots.
/// </summary>
public sealed class ChestHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Exotic;

	public override int HpGain => 0;

	public override bool IsActiveAbility => true;

	public override int ActiveAbilityDailyCost => 10;

	public override bool IsAbilityEnabled => Main.LocalPlayer.GetModPlayer<ChestHeartPlayer>().Enabled;

	public override void SetAbilityEnabled(bool enabled)
	{
		var modPlayer = Main.LocalPlayer.GetModPlayer<ChestHeartPlayer>();
		
		if (!enabled)
		{
			// Verify if there are items inside the extra inventory slots
			bool hasItems = false;
			for (int i = 0; i < modPlayer.ExtraInventory.Length; i++)
			{
				if (!modPlayer.ExtraInventory[i].IsAir)
				{
					hasItems = true;
					break;
				}
			}

			if (hasItems)
			{
				// Block turning the ability off and play a refusal cue
				SoundEngine.PlaySound(SoundID.MenuTick);
				if (Main.myPlayer == Main.LocalPlayer.whoAmI)
				{
					Main.NewText("Cannot disable Chest Heart while there are items in the extra slots!", Color.Red);
				}
				return;
			}
		}

		modPlayer.Enabled = enabled;
	}

	protected override void PlayConsumeSound(Vector2 center)
	{
		base.PlayConsumeSound(center);
		SoundEngine.PlaySound(SoundID.Unlock, center);
	}

	public override void AddRecipes() =>
		CreateRecipe()
			.AddIngredient(ModContent.ItemType<RareLifeShard>(), RecipeCost(3))
			.AddIngredient(ModContent.ItemType<VitalChestItem>(), RecipeCost(10))
			.AddTile(TileID.MythrilAnvil)
			.Register();
}
