using System;
using ElementalHearts.Common.Configs;
using ElementalHearts.Content.Items.Hearts;
using Terraria;
using Terraria.ModLoader;

namespace ElementalHearts.Common.Systems;

public sealed class RecipeDifficultySystem : ModSystem
{
	public override void PostAddRecipes()
	{
		var config = ElementalHeartsConfig.Instance;
		if (config.RecipeDifficulty == 10)
			return;

		foreach (Recipe recipe in Main.recipe)
		{
			if (recipe.createItem.ModItem is not ElementalHeartItem)
				continue;

			foreach (Item ingredient in recipe.requiredItem)
			{
				ingredient.stack = Math.Max(1, (int)Math.Round(ingredient.stack * config.RecipeDifficulty / 10.0));
			}
		}
	}
}
