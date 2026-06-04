#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using ElementalHearts.Content.Items.Hearts;
using Terraria.ModLoader;

namespace ElementalHearts.Common.Hearts;

/// <summary>
/// DEBUG-only consistency check run once from the mod's <c>PostSetupContent</c>, after every
/// registry is built. Both <see cref="HeartEffectRegistry"/> and
/// <see cref="ElementalPowerRegistry"/> fall back silently to a generic value, so a heart that
/// forgot its entry still loads and ships — it just looks/labels wrong with no error anywhere.
/// This validator turns those silent fallbacks into log warnings so the gap is caught the first
/// time the mod is run in a dev build, exactly as the "checklist for every new heart" in
/// CLAUDE.md promises. Compiled out of release entirely (this whole file is under <c>#if DEBUG</c>).
/// This is the *content* half of the heart validator; the *texture* half (every heart has a
/// <c>.png</c> beside its <c>.cs</c>) is enforced before compile by <c>build.ps1</c>'s
/// <c>Ensure-HeartTextures</c> — it can't run here because a missing texture hard-fails tML's
/// content load before <c>PostSetupContent</c> is ever reached.
/// </summary>
internal static class HeartContentValidator
{
	public static void Validate(Mod mod)
	{
		var missingEffect = new List<string>();
		var thinPalette = new List<string>();
		var missingPower = new List<string>();
		var hpOnActiveAbility = new List<string>();
		var tierFolderMismatch = new List<string>();
		int total = 0;

		foreach (ElementalHeartItem heart in HeartRegistry.All)
		{
			total++;

			// Effect is keyed by ConsumptionId; power by the class Name — mirror the live lookups
			// in ElementalHeartItem.PlayConsumeEffect / ElementalPowerMaterial exactly.
			if (!HeartEffectRegistry.HasExplicit(heart.ConsumptionId))
			{
				missingEffect.Add(heart.Name);
			}
			else
			{
				// Color-palette rule: a curated entry must carry at least three sprite-derived
				// colours (or be intentionally prismatic). Fewer than three = not finished.
				HeartEffect effect = HeartEffectRegistry.Get(heart.ConsumptionId);
				if (!effect.Rainbow && (effect.Colors == null || effect.Colors.Length < 3))
					thinPalette.Add(heart.Name);
			}

			if (!ElementalPowerRegistry.HasExplicit(heart.Name))
				missingPower.Add(heart.Name);

			// CLAUDE.md core rule #9: only passive hearts grant HP. An active-ability heart that
			// forgot to override HpGain => 0 would silently hand out free max-life on top of its
			// ability — a real balance bug, caught here.
			if (heart.IsActiveAbility && heart.HpGain != 0)
				hpOnActiveAbility.Add(heart.Name);

			if (HasTierFolderMismatch(heart))
				tierFolderMismatch.Add($"{heart.Name} ({heart.Tier}, namespace {heart.GetType().Namespace})");
		}

		Report(mod, "no HeartEffectRegistry entry (using hash-derived fallback colour)", missingEffect);
		Report(mod, "fewer than 3 curated palette colours (color-palette rule)", thinPalette);
		Report(mod, "no ElementalPowerRegistry entry (power tooltip will show its class name)", missingPower);
		Report(mod, "is an active-ability heart but grants HP (must override HpGain => 0)", hpOnActiveAbility);
		Report(mod, "has a tier/folder namespace mismatch", tierFolderMismatch);

		if (missingEffect.Count == 0 && thinPalette.Count == 0 && missingPower.Count == 0 && hpOnActiveAbility.Count == 0 && tierFolderMismatch.Count == 0)
			mod.Logger.Info($"HeartContentValidator: all {total} hearts validated cleanly.");
	}

	private static bool HasTierFolderMismatch(ElementalHeartItem heart)
	{
		string[] namespaceParts = (heart.GetType().Namespace ?? string.Empty).Split('.');
		string declaredTier = heart.Tier.ToString();
		string[] tierNames = Enum.GetNames(typeof(HeartTier));

		foreach (string part in namespaceParts)
		{
			if (tierNames.Contains(part) && part != declaredTier)
				return true;
		}

		int vanillaIndex = Array.IndexOf(namespaceParts, "Vanilla");
		if (vanillaIndex >= 0 && vanillaIndex + 1 < namespaceParts.Length && tierNames.Contains(namespaceParts[vanillaIndex + 1]))
			return namespaceParts[vanillaIndex + 1] != declaredTier;

		return false;
	}

	private static void Report(Mod mod, string problem, List<string> hearts)
	{
		if (hearts.Count == 0)
			return;

		mod.Logger.Warn($"HeartContentValidator: {hearts.Count} heart(s) {problem}: {string.Join(", ", hearts)}");
	}
}
#endif
