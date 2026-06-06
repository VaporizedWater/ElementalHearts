// Architecture scaffold only. Fill behavior in the implementation pass.
using Microsoft.Xna.Framework;

namespace ElementalHearts.Core;

/// <summary>Visual extensions for HeartId: consumption colors, glow intent, light, dust, and future effect descriptors.</summary>
public static class HeartIdVisualExtensions
{
	public static Color GetPrimaryColor(this HeartId id)
	{
		// Return the main material color for consumption dust and item glow.
		return Color.White;
	}

	public static Color GetSecondaryColor(this HeartId id)
	{
		// Return alternate material color; default mirrors primary color.
		return id.GetPrimaryColor();
	}

	public static bool UsesPrismaticEffect(this HeartId id)
	{
		// Return true for rainbow/zenith/prismatic hearts that cycle hue instead of fixed colors.
		return false;
	}
}
