using System;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ElementalHearts.Common.CrossMod.MusicDisplay;

/// <summary>
/// Registers the mod's Animate boss themes with the MusicDisplay mod
/// (see <c>MusicDisplay.md</c>). Each track is shown with a title, author,
/// and the mod name as the subtitle while it is playing.
/// </summary>
internal static class MusicDisplayIntegration
{
	private const string MusicDisplayModName = "MusicDisplay";
	private const string AddMusic = "AddMusic";

	public static void Register(Mod elementalHearts)
	{
		// Music autoloading is disabled on dedicated servers / no-audio launches;
		// without it GetMusicSlot returns 0, which would register a useless entry.
		if (!elementalHearts.MusicAutoloadingEnabled)
			return;

		if (!ModLoader.TryGetMod(MusicDisplayModName, out Mod musicDisplay))
			return;

		try
		{
			AddTrack(elementalHearts, musicDisplay, "Music/CommonAnimateTheme", "CommonAnimate");
			AddTrack(elementalHearts, musicDisplay, "Music/UncommonAnimateTheme", "UncommonAnimate");
			AddTrack(elementalHearts, musicDisplay, "Music/RareAnimateTheme", "RareAnimate");
			AddTrack(elementalHearts, musicDisplay, "Music/EpicAnimateTheme", "EpicAnimate");
			AddTrack(elementalHearts, musicDisplay, "Music/LegendaryAnimateTheme", "LegendaryAnimate");
		}
		catch (Exception e)
		{
			elementalHearts.Logger.Error($"Failed to register tracks with MusicDisplay: {e.Message}\n{e.StackTrace}");
		}
	}

	private static void AddTrack(Mod elementalHearts, Mod musicDisplay, string musicPath, string trackKey)
	{
		short slot = (short)MusicLoader.GetMusicSlot(elementalHearts, musicPath);
		if (slot <= 0)
			return;

		LocalizedText title = Language.GetText($"Mods.ElementalHearts.TrackNames.{trackKey}.Name");
		LocalizedText author = Language.GetText($"Mods.ElementalHearts.TrackNames.{trackKey}.Author");
		LocalizedText subtitle = Language.GetText("Mods.ElementalHearts.TrackNames.ModName");

		musicDisplay.Call(AddMusic, slot, title, author, subtitle, (Func<bool>)(() => true));
	}
}
