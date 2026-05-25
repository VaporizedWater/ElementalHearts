using Terraria;
using Terraria.ModLoader;

namespace Terraria.ModLoader;

public static class RecipeExtensions
{
	/// <summary>
	/// Safely adds an ingredient only if its ID is valid (> 0).
	/// This handles gracefully omitting disabled mod items (like Life Shards) without breaking the recipe.
	/// </summary>
	public static Recipe AddOptionalIngredient(this Recipe recipe, int itemID, int stack = 1)
	{
		if (itemID > 0)
		{
			recipe.AddIngredient(itemID, stack);
		}
		return recipe;
	}
}
