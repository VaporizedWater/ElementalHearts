// Architecture scaffold only. Fill behavior in the implementation pass.
namespace ElementalHearts.Core;

/// <summary>Semantic extensions for HeartId: tier, kind, source mod, asset path, and localization identity.</summary>
public static class HeartIdExtensions
{
	public static HeartTier GetTier(this HeartId id)
	{
		// Return the progression tier for this heart. Implementation will be exhaustive over HeartId.
		return HeartTier.Common;
	}

	public static HeartKind GetKind(this HeartId id)
	{
		// Return behavior flags for this heart, such as craftable, boss drop, potion, active ability, or cross-mod.
		return HeartKind.Craftable;
	}

	public static ModSource GetSource(this HeartId id)
	{
		// Return the source mod used for load gates and UI filters.
		return ModSource.Vanilla;
	}

	public static string GetSourceModName(this HeartId id)
	{
		// Convert typed source identity to tModLoader's internal mod name only at API boundaries.
		return id.GetSource() switch
		{
			ModSource.Calamity => "CalamityMod",
			ModSource.Thorium => "ThoriumMod",
			ModSource.Consolaria => "Consolaria",
			_ => string.Empty,
		};
	}

	public static string GetPowerKey(this HeartId id)
	{
		// Return the localization/power token for activated-power tooltip text.
		return id.ToString();
	}

	public static string GetAssetPath(this HeartId id)
	{
		// Return the asset path for this heart once assets are moved under Assets/.
		return $"ElementalHearts/Assets/Items/Hearts/{id}";
	}
}
