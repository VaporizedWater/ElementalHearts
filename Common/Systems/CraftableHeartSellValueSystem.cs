using System.Collections.Generic;
using ElementalHearts.Content.Items.Hearts;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Common.Systems;

/// <summary>
/// Caps the sell price of every craftable heart at 1/10 of its recipe's total material
/// sell value, so hoarding-and-selling hearts can never be more profitable than selling
/// the raw materials. Built once after recipes load and read each time a heart's
/// <see cref="Item.value"/> is initialised.
///
/// Math: a recipe ingredient of stack S worth Item.value V sells for V*S/5 (vanilla
/// rule). The heart should sell for one-tenth of the material sell total — i.e.
/// (Σ V*S / 5) / 10. Item.value is buy-price (5x sell-price), so the heart's
/// Item.value resolves to Σ(V * S) / 10.
/// </summary>
public sealed class CraftableHeartSellValueSystem : ModSystem
{
	private static readonly Dictionary<int, int> heartSellValueByType = new();

	public static bool TryGetSellValue(int itemType, out int value) =>
		heartSellValueByType.TryGetValue(itemType, out value);

	public override void PostAddRecipes()
	{
		heartSellValueByType.Clear();

		for (int i = 0; i < Recipe.numRecipes; i++)
		{
			Recipe recipe = Main.recipe[i];
			Item result = recipe.createItem;
			if (result?.ModItem is not ElementalHeartItem)
				continue;

			long total = 0;
			foreach (Item ingredient in recipe.requiredItem)
			{
				if (ingredient == null || ingredient.type <= ItemID.None || ingredient.stack <= 0)
					continue;

				// A fresh Item gives us the canonical Item.value for the type, which is
				// what vanilla uses for sell-price math. The ingredient instances inside
				// the recipe already have SetDefaults applied, so ingredient.value is
				// already that canonical value — no second lookup needed.
				total += (long)ingredient.value * ingredient.stack;
			}

			heartSellValueByType[result.type] = (int)(total / 10);
		}
	}

	public override void Unload() => heartSellValueByType.Clear();
}
