using Terraria.ModLoader;

namespace ElementalHearts.Content.Items.Hearts.CrossMod;

/// <summary>
/// Base for <em>craftable</em> hearts made from another mod's content. The per-mod
/// load gate lives on <see cref="ElementalHeartItem"/> (via <see cref="ElementalHeartItem.SourceMod"/>);
/// this class re-abstracts <see cref="SourceMod"/> so every concrete cross-mod heart
/// must declare it, and adds <see cref="RegisterModRecipe"/> for mod-gated recipes.
/// Cross-mod <em>boss</em> hearts instead use the per-mod <c>BossHeartItem</c> subclasses.
/// </summary>
public abstract class CrossModHeartItem : ElementalHeartItem
{
	public abstract override string SourceMod { get; }

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
