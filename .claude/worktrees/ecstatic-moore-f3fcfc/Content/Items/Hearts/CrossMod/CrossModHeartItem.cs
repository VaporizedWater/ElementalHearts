using Terraria.ModLoader;

namespace ElementalHearts.Content.Items.Hearts.CrossMod;

/// <summary>
/// Base for hearts crafted from another mod's content. Hearts always load; concrete types
/// call <see cref="RegisterModRecipe"/> from <see cref="ModItem.AddRecipes"/> to add a
/// recipe only when <see cref="SourceMod"/> is present.
/// </summary>
public abstract class CrossModHeartItem : ElementalHeartItem
{
	/// <summary>Internal name of the mod this heart depends on (e.g. <c>"CalamityMod"</c>).</summary>
	protected abstract string SourceMod { get; }

	/// <summary>
	/// Registers a recipe that takes a single ingredient from <see cref="SourceMod"/>.
	/// Silently no-ops if the source mod isn't loaded or the ingredient can't be resolved.
	/// </summary>
	protected void RegisterModRecipe(string ingredientInternalName, int quantity, int tile)
	{
		if (!ModLoader.TryGetMod(SourceMod, out Mod sourceMod))
			return;
		if (!sourceMod.TryFind<ModItem>(ingredientInternalName, out ModItem ingredient))
			return;

		CreateRecipe()
			.AddIngredient(ingredient.Type, quantity)
			.AddTile(tile)
			.Register();
	}
}
