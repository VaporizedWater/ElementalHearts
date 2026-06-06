// Architecture scaffold only. Fill behavior in the implementation pass.
using Terraria;
using Terraria.ModLoader;

namespace ElementalHearts.Core;

/// <summary>Recipe-building extensions for HeartId. Item classes call this instead of duplicating recipe logic.</summary>
public static class HeartIdRecipeExtensions
{
	public static void AddHeartRecipe(this HeartId id, ModItem item)
	{
		// Build and register the recipe for this heart from enum-owned recipe intent.
	}

	public static int GetRecipeCost(this HeartId id, int baseAmount)
	{
		// Apply recipe difficulty scaling and nice-number rounding.
		return baseAmount;
	}

	public static bool HasRecipe(this HeartId id)
	{
		// Boss-only and special hearts can opt out of normal recipe generation.
		return id.GetKind().HasFlag(HeartKind.Craftable);
	}
}
