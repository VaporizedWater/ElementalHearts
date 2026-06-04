using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Utilities;

namespace ElementalHearts.Common.Hearts;

/// <summary>
/// One heart's consumption-effect colour theme. The outer particle ring is drawn
/// in these colours; the inner ring uses the tier colour (see
/// <see cref="HeartTierExtensions.GetEffectColor"/>).
/// </summary>
public readonly struct HeartEffect
{
	public readonly Color[]? Colors;
	public readonly bool Rainbow;

	public Color Primary => Colors != null && Colors.Length > 0 ? Colors[0] : Color.White;

	public HeartEffect(params Color[] colors)
	{
		Colors = colors;
		Rainbow = false;
	}

	private HeartEffect(bool rainbow)
	{
		Colors = null;
		Rainbow = rainbow;
	}

	/// <summary>A heart whose particles cycle the full hue wheel instead of using fixed colours.</summary>
	public static HeartEffect Prismatic => new(rainbow: true);

	/// <summary>Picks a particle colour: a random hue if prismatic, otherwise one of the theme colours.</summary>
	public Color Pick(UnifiedRandom rand)
	{
		if (Rainbow)
			return Main.hslToRgb(rand.NextFloat(), 1f, 0.62f);

		if (Colors != null && Colors.Length > 0)
			return Colors[rand.Next(Colors.Length)];

		return Color.White;
	}
}

/// <summary>
/// Hand-curated per-heart colour themes for the consumption effect, sourced directly from each
/// heart's own sprite palette (the color-palette RULE — see CLAUDE.md). Every heart carries at
/// least three sprite-true colours (a monochrome material gets three shades); prismatic hearts are
/// the only exemption. Use <c>tools/Get-HeartPalette.ps1</c> to extract a sprite's hues, then
/// curate — drop the black outline / antialiasing and keep the colours that say what the heart is
/// made of. Hearts with no entry fall back to a stable hue derived from their id.
/// </summary>
public static class HeartEffectRegistry
{
	private static HeartEffect Eff(int r, int g, int b) => new(new Color(r, g, b));

	private static HeartEffect Eff(int r1, int g1, int b1, int r2, int g2, int b2) =>
		new(new Color(r1, g1, b1), new Color(r2, g2, b2));

	private static HeartEffect Eff(int r1, int g1, int b1, int r2, int g2, int b2, int r3, int g3, int b3) =>
		new(new Color(r1, g1, b1), new Color(r2, g2, b2), new Color(r3, g3, b3));

	private static readonly Dictionary<string, HeartEffect> Effects = new()
	{
		// ── Vanilla: Common ──────────────────────────────────────────────────────
		["AcornHeart"]       = Eff(112, 96, 48, 144, 160, 48, 64, 64, 32),   // acorn brown + leaf green + shadow
		["BorealWoodHeart"]  = Eff(112, 80, 64, 144, 112, 96, 80, 64, 64),
		["BubbleHeart"]      = Eff(48, 240, 192, 176, 176, 255, 224, 224, 240),
		["CactusHeart"]      = Eff(80, 128, 16, 176, 192, 48, 64, 96, 16),
		["CandyCaneHeart"]   = Eff(240, 16, 32, 255, 240, 240, 144, 16, 16),
		["CloudHeart"]       = Eff(255, 255, 255, 192, 240, 255, 176, 208, 208),
		["CoralstoneHeart"]  = Eff(144, 32, 112, 0, 80, 128, 0, 96, 80),     // pink coral + blue + teal
		["CrimsandHeart"]    = Eff(128, 48, 32, 80, 80, 64, 64, 16, 16),
		["CrimstoneHeart"]   = Eff(192, 64, 64, 96, 32, 32, 64, 16, 16),
		["DirtHeart"]        = Eff(144, 112, 80, 176, 128, 80, 112, 80, 64),
		["DynastyHeart"]     = Eff(160, 96, 48, 128, 80, 32, 80, 48, 16),
		["EbonsandHeart"]    = Eff(96, 96, 128, 64, 48, 80, 128, 112, 160),
		["EbonstoneHeart"]   = Eff(96, 80, 128, 112, 112, 144, 80, 64, 80),
		["EbonwoodHeart"]    = Eff(128, 112, 144, 96, 96, 128, 80, 80, 96),
		["FleshHeart"]       = Eff(160, 48, 80, 112, 16, 16, 224, 208, 128),
		["FossilHeart"]      = Eff(128, 80, 64, 112, 64, 32, 144, 80, 64),
		["GemcornHeart"]     = Eff(96, 64, 40, 152, 92, 222, 62, 112, 212),  // bark brown + amethyst + sapphire
		["GlassHeart"]       = Eff(192, 240, 255, 80, 176, 192, 64, 128, 144),
		["GraniteHeart"]     = Eff(48, 48, 96, 32, 16, 48, 32, 32, 64),
		["GlowingMushroomHeart"] = Eff(48, 64, 144, 64, 96, 176, 32, 32, 96),
		["HayHeart"]         = Eff(208, 192, 48, 224, 208, 112, 160, 128, 48),
		["HoneyHeart"]       = Eff(255, 176, 16, 255, 208, 32, 240, 144, 0),
		["IceHeart"]         = Eff(144, 192, 224, 224, 240, 240, 112, 176, 224),
		["LesionHeart"]      = Eff(64, 80, 48, 112, 80, 80, 48, 32, 32),
		["MarbleHeart"]      = Eff(192, 208, 224, 160, 176, 208, 144, 144, 176),
		["MudHeart"]         = Eff(96, 64, 80, 112, 80, 96, 80, 64, 64),
		["ObsidianHeart"]    = Eff(48, 32, 80, 80, 64, 128, 16, 16, 32),
		["PalmWoodHeart"]    = Eff(176, 144, 80, 192, 176, 96, 112, 96, 48),
		["PearlsandHeart"]   = Eff(208, 192, 192, 240, 224, 224, 176, 160, 192),
		["PearlstoneHeart"]  = Eff(176, 176, 192, 160, 144, 160, 128, 96, 112),
		["PearlwoodHeart"]   = Eff(192, 176, 144, 176, 160, 112, 144, 128, 96),
		["PinkIceHeart"]     = Eff(224, 176, 208, 208, 144, 192, 192, 64, 128),
		["PumpkinHeart"]     = Eff(240, 144, 16, 255, 176, 16, 176, 80, 16),
		["PurpleIceHeart"]   = Eff(144, 128, 208, 112, 48, 208, 208, 208, 224),
		["RainCloudHeart"]   = Eff(144, 144, 176, 112, 112, 160, 80, 80, 112),
		["RainbowHeart"]     = HeartEffect.Prismatic,
		["RedIceHeart"]      = Eff(192, 128, 112, 224, 160, 144, 160, 32, 32),
		["RichMahoganyHeart"]= Eff(144, 80, 80, 112, 64, 64, 80, 48, 48),
		["SandHeart"]        = Eff(208, 192, 96, 192, 160, 80, 144, 128, 64),
		["ShadewoodHeart"]   = Eff(96, 112, 112, 80, 80, 96, 64, 64, 64),
		["SlimeHeart"]       = Eff(64, 128, 255, 96, 160, 255, 16, 48, 112),
		["SnowCloudHeart"]   = Eff(144, 160, 176, 112, 144, 176, 80, 80, 112),
		["SnowHeart"]        = Eff(192, 224, 224, 240, 255, 255, 128, 176, 224),
		["SpookyHeart"]      = Eff(112, 96, 144, 96, 80, 112, 128, 112, 160),
		["StoneHeart"]       = Eff(128, 128, 128, 160, 160, 160, 80, 80, 80),
		["SunplateHeart"]    = Eff(240, 208, 32, 255, 255, 208, 224, 144, 32),
		["WoodHeart"]        = Eff(144, 112, 80, 192, 144, 112, 80, 64, 48),

		// ── Vanilla: Uncommon ────────────────────────────────────────────────────
		["AmethystHeart"]    = Eff(160, 16, 208, 192, 96, 224, 96, 16, 128),
		["AppleHeart"]       = Eff(240, 48, 64, 192, 32, 48, 32, 96, 64),
		["ApricotHeart"]     = Eff(240, 170, 90, 250, 200, 130, 200, 120, 50),   // blank sprite — fruit colours; needs art
		["BananaHeart"]      = Eff(224, 192, 32, 208, 144, 0, 160, 112, 0),
		["BlackcurrantHeart"]= Eff(110, 50, 120, 60, 30, 70, 30, 15, 40),        // blank sprite — fruit colours; needs art
		["BlinkrootHeart"]   = Eff(96, 112, 64, 128, 112, 48, 48, 64, 48),
		["BloodOrangeHeart"] = Eff(220, 70, 40, 250, 130, 60, 160, 30, 20),      // blank sprite — fruit colours; needs art
		["BlueHeart"]        = Eff(16, 96, 240, 0, 64, 176, 96, 160, 255),
		["CherryHeart"]      = Eff(200, 30, 50, 240, 80, 90, 130, 15, 30),       // blank sprite — fruit colours; needs art
		["CoconutHeart"]     = Eff(120, 80, 50, 230, 225, 210, 80, 50, 30),      // blank sprite — fruit colours; needs art
		["CopperHeart"]      = Eff(208, 128, 64, 176, 96, 32, 112, 64, 32),
		["DaybloomHeart"]    = Eff(64, 200, 96, 240, 210, 90, 112, 80, 64),
		["DeathweedHeart"]   = Eff(112, 96, 128, 64, 64, 112, 144, 112, 80),
		["DragonFruitHeart"] = Eff(225, 40, 110, 250, 110, 160, 90, 180, 90),    // blank sprite — fruit colours; needs art
		["ElderberryHeart"]  = Eff(90, 50, 110, 50, 30, 70, 25, 15, 35),         // blank sprite — fruit colours; needs art
		["EmeraldHeart"]     = Eff(0, 208, 96, 96, 224, 144, 0, 128, 64),
		["EncumberingHeart"] = Eff(64, 80, 80, 48, 48, 64, 96, 96, 112),   // heavy slate-grey weight + cold steel sheen
		["TreasureHeart"]    = Eff(192, 160, 80, 240, 192, 96, 144, 112, 64), // treasure magnet colors
		["EnchantedHeart"]   = Eff(64, 96, 240, 128, 160, 240, 32, 16, 80),
		["FireblossomHeart"] = Eff(240, 80, 0, 255, 140, 40, 192, 48, 16),
		["GoldHeart"]        = Eff(240, 208, 72, 255, 240, 160, 192, 160, 16),
		["GrapeHeart"]       = Eff(120, 60, 170, 160, 100, 210, 80, 40, 120),    // blank sprite — fruit colours; needs art
		["GrapefruitHeart"]  = Eff(240, 120, 110, 250, 170, 150, 200, 80, 70),   // blank sprite — fruit colours; needs art
		["GreenHeart"]       = Eff(0, 224, 16, 0, 176, 16, 144, 255, 160),
		["IronHeart"]        = Eff(160, 150, 140, 112, 112, 112, 80, 64, 64),
		["LeadHeart"]        = Eff(80, 96, 112, 64, 64, 64, 48, 48, 48),
		["LemonHeart"]       = Eff(245, 220, 40, 255, 240, 120, 200, 170, 20),   // blank sprite — fruit colours; needs art
		["LimeHeart"]        = Eff(150, 210, 40, 190, 235, 90, 100, 160, 20),    // blank sprite — fruit colours; needs art
		["MangoHeart"]       = Eff(245, 170, 40, 255, 205, 80, 220, 90, 40),     // blank sprite — fruit colours; needs art
		["MoonglowHeart"]    = Eff(64, 128, 32, 180, 200, 240, 32, 96, 16),
		["MushroomHeart"]    = Eff(200, 120, 90, 235, 210, 175, 140, 80, 60),    // blank sprite — mushroom colours; needs art
		["OrangeHeart"]      = Eff(240, 140, 30, 255, 180, 70, 200, 100, 10),    // blank sprite — fruit colours; needs art
		["PeachHeart"]       = Eff(240, 160, 80, 208, 96, 64, 240, 208, 96),
		["PineappleHeart"]   = Eff(235, 180, 40, 255, 215, 90, 90, 150, 40),     // blank sprite — fruit colours; needs art
		["PlatinumHeart"]    = Eff(176, 192, 224, 128, 144, 192, 80, 80, 128),
		["PlumHeart"]        = Eff(110, 50, 110, 150, 80, 150, 70, 30, 75),      // blank sprite — fruit colours; needs art
		["PomegranateHeart"] = Eff(180, 30, 50, 220, 70, 80, 120, 20, 40),       // blank sprite — fruit colours; needs art
		["RambutanHeart"]    = Eff(210, 40, 50, 240, 90, 80, 235, 225, 210),     // blank sprite — fruit colours; needs art
		["RubyHeart"]        = Eff(255, 48, 80, 224, 48, 72, 160, 32, 48),
		["SapphireHeart"]    = Eff(48, 128, 255, 96, 160, 255, 32, 80, 200),
		["ShiverthornHeart"] = Eff(32, 160, 240, 128, 176, 224, 32, 32, 160),
		["SilverHeart"]      = Eff(176, 176, 176, 240, 255, 255, 96, 112, 112),
		["StarFruitHeart"]   = Eff(220, 225, 70, 240, 240, 130, 170, 190, 40),   // blank sprite — fruit colours; needs art
		["TealHeart"]        = Eff(0, 224, 176, 0, 176, 128, 144, 255, 224),
		["TinHeart"]         = Eff(144, 144, 112, 176, 160, 128, 96, 96, 80),
		["TopazHeart"]       = Eff(232, 182, 62, 255, 208, 96, 160, 112, 16),
		["TungstenHeart"]    = Eff(160, 192, 160, 208, 240, 208, 80, 112, 80),
		["ViciousMushroomHeart"] = Eff(190, 40, 50, 230, 80, 80, 120, 20, 30),   // blank sprite — vicious mushroom; needs art
		["VileMushroomHeart"]    = Eff(120, 90, 150, 90, 140, 80, 70, 50, 90),   // blank sprite — vile mushroom; needs art
		["WaterleafHeart"]   = Eff(32, 176, 96, 16, 128, 80, 144, 255, 192),

		// ── Vanilla: Rare ────────────────────────────────────────────────────────
		["AmberHeart"]       = Eff(255, 224, 96, 222, 142, 32, 192, 112, 16),
		["BrainHeart"]       = Eff(224, 160, 160, 192, 112, 128, 112, 32, 48),
		["CobaltHeart"]      = Eff(16, 80, 144, 48, 160, 208, 0, 48, 112),
		["CogHeart"]         = Eff(176, 144, 80, 224, 176, 96, 96, 80, 48),
		["CrimtaneHeart"]    = Eff(224, 64, 64, 224, 80, 96, 128, 32, 48),
		["CursedFlameHeart"] = Eff(96, 255, 0, 48, 192, 32, 176, 255, 0),
		["DarkHeart"]        = Eff(240, 128, 240, 255, 192, 240, 200, 96, 224),  // sprite reads vivid magenta
		["DemoniteHeart"]    = Eff(96, 96, 160, 144, 128, 208, 64, 64, 112),
		["DiamondHeart"]     = Eff(192, 232, 246, 32, 208, 224, 128, 176, 208),
		["DiscordHeart"]     = Eff(240, 112, 176, 176, 16, 96, 224, 16, 112),
		["HellstoneHeart"]   = Eff(144, 64, 64, 232, 92, 42, 80, 32, 32),
		["IchorHeart"]       = Eff(255, 208, 80, 255, 160, 0, 255, 255, 160),
		["JungleSporeHeart"] = Eff(176, 208, 48, 128, 144, 32, 176, 80, 96),
		["MeteoriteHeart"]   = Eff(144, 96, 80, 96, 48, 64, 160, 112, 96),
		["MythrilHeart"]     = Eff(96, 192, 208, 96, 160, 96, 112, 208, 208),
		["OrichalcumHeart"]  = Eff(192, 0, 160, 255, 112, 224, 128, 0, 112),
		["PalladiumHeart"]   = Eff(240, 96, 64, 255, 176, 128, 192, 48, 32),
		["SoulOfFlightHeart"]= Eff(32, 128, 192, 64, 208, 224, 0, 64, 160),
		["SoulOfLightHeart"] = Eff(224, 32, 176, 240, 96, 208, 128, 0, 80),      // sprite hue
		["SoulOfNightHeart"] = Eff(128, 32, 224, 160, 96, 240, 64, 0, 128),
		["WormHeart"]        = Eff(96, 96, 128, 128, 128, 160, 80, 64, 64),

		// ── Vanilla: Epic ────────────────────────────────────────────────────────
		["AdamantiteHeart"]  = Eff(192, 32, 96, 240, 64, 120, 128, 32, 48),
		["ChlorophyteHeart"] = Eff(96, 192, 0, 32, 144, 0, 240, 255, 128),
		["CrystalHeart"]     = Eff(255, 50, 50, 255, 100, 200, 192, 64, 112),
		["HallowedHeart"]    = Eff(200, 196, 176, 246, 240, 172, 128, 112, 128),
		["LifeFruitHeart"]   = Eff(224, 160, 64, 208, 112, 64, 48, 128, 16),
		["MechanicalHeart"]  = Eff(160, 160, 168, 224, 48, 48, 80, 80, 80),
		["ShardHeart"]       = Eff(180, 50, 220, 50, 100, 255, 255, 100, 200),
		["SoulOfFrightHeart"]= Eff(208, 48, 16, 255, 96, 64, 160, 0, 0),
		["SoulOfMightHeart"] = Eff(0, 48, 224, 16, 112, 240, 80, 128, 255),
		["SoulOfSightHeart"] = Eff(0, 160, 96, 80, 224, 128, 144, 255, 144),
		["TitaniumHeart"]    = Eff(144, 128, 176, 144, 96, 144, 64, 80, 80),

		// ── Vanilla: Legendary ───────────────────────────────────────────────────
		["EctoplasmHeart"]   = Eff(32, 192, 255, 0, 112, 255, 144, 240, 255),
		["LuminiteHeart"]    = Eff(202, 182, 232, 128, 112, 176, 80, 128, 112),
		["ShroomiteHeart"]   = Eff(80, 96, 255, 112, 208, 255, 32, 16, 160),
		["SpectreHeart"]     = Eff(32, 192, 255, 224, 255, 255, 0, 144, 240),
		// Lunar bricks
		["AstraHeart"] = new HeartEffect(
			new Color( 22,  82, 212), new Color( 52, 122, 246), new Color( 32, 152, 252),
			new Color( 72,  92, 232), new Color(102, 172, 252),                            // 5 vibrant blues
			new Color( 22,  22,  28), new Color( 52,  52,  62), new Color( 82,  82,  92)), // 3 black-grays
		["CosmicEmberHeart"]   = Eff(255, 128, 96, 132, 132, 138, 255, 176, 128),  // ember + ash gray
		["CryocoreHeart"]      = Eff(162, 172, 182, 62, 152, 246, 32, 52, 122),     // gray + vibrant blue + dark blue
		["DarkCelestialHeart"] = Eff(172, 62, 232, 122, 122, 132, 62, 32, 92),      // vibrant purple + gray + dark purple
		["HeavenforgeHeart"]   = Eff(250, 250, 252, 192, 192, 202, 102, 102, 112),  // vibrant white + light gray + dark gray
		["LunarRustHeart"]     = Eff(122, 32, 42, 102, 222, 202, 242, 92, 182),     // maroon + teal + vibrant pink
		["MercuryHeart"]       = Eff(22, 22, 28, 132, 132, 142, 172, 212, 246),     // black + gray + light blue
		["StarRoyaleHeart"]    = Eff(252, 222, 52, 52, 122, 246, 255, 255, 208),    // vibrant yellow + blue + starlight

		// ── Vanilla: Exotic (boss-themed) ────────────────────────────────────────
		["BetsyHeart"]          = Eff(192, 64, 64, 255, 208, 96, 255, 144, 64),
		["BrainOfCthulhuHeart"] = Eff(144, 192, 80, 96, 128, 48, 32, 96, 112),  // sprite reads green/teal
		["DeerclopsHeart"]      = Eff(176, 224, 224, 112, 192, 224, 80, 96, 128),
		["TheDestroyerHeart"]   = Eff(96, 96, 96, 160, 160, 160, 48, 48, 48),
		["DukeFishronHeart"]    = Eff(96, 96, 224, 144, 192, 255, 48, 16, 96),
		["EaterOfWorldsHeart"]  = Eff(48, 32, 16, 48, 32, 32, 64, 48, 48),
		["ElfHeart"]            = Eff(216, 40, 40, 60, 170, 80, 224, 224, 224),  // santa red + green + white
		["ClayHeart"]           = Eff(192, 112, 80, 160, 80, 48, 128, 64, 32),
		["ChestHeart"]          = Eff(208, 128, 160, 160, 32, 64, 128, 16, 48),
		["SkeletronPrimeHeart"] = Eff(160, 160, 168, 224, 48, 48, 80, 80, 80),
		["TheTwinsHeart"]       = Eff(160, 160, 168, 255, 32, 32, 64, 255, 64),
		["EmpressOfLightHeart"] = HeartEffect.Prismatic,
		["EyeOfCthulhuHeart"]   = Eff(192, 48, 48, 64, 64, 208, 224, 224, 224),
		["FlyingDutchmanHeart"] = Eff(128, 80, 64, 192, 40, 16, 80, 80, 64),
		["GolemHeart"]          = Eff(144, 64, 0, 224, 128, 48, 96, 32, 0),
		["JackOLanternHeart"]   = Eff(235, 120, 30, 250, 205, 70, 70, 40, 20),   // pumpkin + candle flame + charred dark
		["KingSlimeHeart"]      = Eff(0, 160, 208, 96, 192, 224, 0, 80, 128),
		["LunaticCultistHeart"] = Eff(255, 208, 64, 255, 255, 80, 144, 96, 80),
		["MagnificationHeart"]  = Eff(192, 240, 255, 64, 128, 144, 80, 48, 160),  // looking-glass cyan + lens chroma
		["MartianSaucerHeart"]  = Eff(160, 160, 160, 32, 144, 160, 0, 64, 144),
		["MoonLordHeart"]       = Eff(176, 144, 128, 32, 224, 144, 96, 64, 64),
		["MourningWoodHeart"]   = Eff(240, 128, 0, 255, 160, 48, 48, 32, 80),
		["PlanteraHeart"]       = Eff(208, 80, 160, 144, 208, 32, 96, 32, 64),
		["PumpkingHeart"]       = Eff(240, 112, 0, 255, 96, 0, 32, 32, 64),
		["QueenBeeHeart"]       = Eff(255, 160, 16, 208, 144, 32, 64, 48, 32),
		["QueenSlimeHeart"]     = Eff(112, 48, 176, 0, 112, 192, 200, 96, 224),
		["RazorpineHeart"]      = Eff(32, 192, 144, 0, 64, 48, 32, 144, 96),
		["SkeletronHeart"]      = Eff(208, 208, 160, 176, 176, 144, 80, 80, 48),
		["WallOfFleshHeart"]    = Eff(112, 48, 80, 80, 32, 48, 112, 64, 128),

		// ── Vanilla: Mythic ──────────────────────────────────────────────────────
		["ZenithHeart"]      = HeartEffect.Prismatic,

		// ── Cross-mod: Calamity ──────────────────────────────────────────────────
		["AbyssGravelHeart"]         = Eff(64, 96, 112, 48, 64, 80, 16, 16, 32),
		["AerialiteHeart"]           = Eff(80, 128, 144, 144, 192, 176, 64, 96, 144),
		["AfflictedHeart"]           = Eff(240, 144, 160, 192, 112, 144, 48, 48, 80),
		["AmbergrisHeart"]           = Eff(48, 64, 64, 16, 128, 128, 48, 160, 144),
		["AquaticHeart"]             = Eff(48, 128, 112, 80, 160, 96, 48, 80, 96),
		["ArmoredHeart"]             = Eff(112, 112, 144, 96, 144, 192, 64, 64, 96),
		["AstralBossHeart"]          = Eff(64, 192, 176, 112, 240, 192, 64, 48, 80),
		["AstralClayHeart"]          = Eff(96, 80, 112, 128, 96, 128, 192, 64, 80),
		["AstralDirtHeart"]          = Eff(64, 48, 80, 96, 64, 96, 48, 144, 160),
		["AstralHeart"]              = Eff(48, 144, 160, 192, 64, 80, 128, 64, 112),
		["AstralIceHeart"]           = Eff(128, 112, 144, 96, 80, 112, 64, 192, 176),
		["AstralMonolithHeart"]      = Eff(48, 32, 64, 32, 16, 48, 64, 48, 80),
		["AstralSandHeart"]          = Eff(144, 160, 160, 192, 224, 240, 128, 96, 128),
		["AstralSandstoneHeart"]     = Eff(96, 80, 112, 80, 64, 96, 128, 96, 128),
		["AstralSnowHeart"]          = Eff(192, 208, 224, 224, 240, 255, 96, 96, 160),
		["AstralStoneHeart"]         = Eff(96, 80, 112, 64, 48, 80, 48, 144, 160),
		["AuricHeart"]               = Eff(255, 224, 128, 240, 144, 80, 128, 64, 80),
		["BlazingHeart"]             = Eff(236, 102, 42, 208, 112, 64, 128, 48, 32),
		["BloodyWormHeart"]          = Eff(112, 16, 16, 160, 32, 48, 64, 16, 32),
		["BrimstoneSlagHeart"]       = Eff(160, 32, 48, 64, 48, 80, 80, 16, 64),
		["CalamitousHeart"]          = Eff(80, 0, 192, 112, 0, 192, 160, 48, 208),
		["CelestialRemainsHeart"]    = Eff(96, 80, 112, 48, 64, 96, 255, 160, 96),
		["CinderplateHeart"]         = Eff(240, 160, 112, 160, 96, 112, 240, 224, 128),
		["CorpusHeart"]              = Eff(192, 64, 64, 144, 32, 48, 64, 16, 48),
		["CosmiliteHeart"]           = Eff(224, 80, 80, 176, 48, 64, 112, 92, 202),
		["CryogenHeart"]             = Eff(112, 144, 240, 144, 224, 255, 96, 96, 208),
		["CryonicHeart"]             = Eff(96, 112, 208, 128, 176, 255, 80, 64, 128),
		["CrystallizedToxicHeart"]   = Eff(128, 192, 64, 80, 128, 64, 16, 32, 48),
		["DarkPlasmicHeart"]         = Eff(80, 48, 128, 32, 32, 80, 16, 16, 32),
		["DraconicHeart"]            = Eff(208, 128, 64, 208, 96, 16, 96, 32, 48),
		["DynamoStemHeart"]          = Eff(255, 80, 144, 255, 160, 208, 192, 32, 80),
		["EutrophicSandHeart"]       = Eff(128, 160, 176, 144, 192, 208, 80, 80, 96),
		["ExodiumClusterHeart"]      = Eff(96, 112, 112, 128, 144, 160, 48, 48, 64),
		["FungalHeart"]              = Eff(176, 192, 160, 96, 160, 208, 128, 112, 96),
		["GehennaHeart"]             = Eff(224, 80, 80, 255, 144, 112, 64, 32, 64),
		["GravistarHeart"]           = Eff(128, 96, 128, 240, 96, 80, 48, 144, 160),
		["HardenedAstralSandHeart"]  = Eff(96, 80, 112, 128, 128, 160, 64, 48, 80),
		["MutatedHeart"]             = Eff(112, 96, 128, 96, 64, 96, 80, 64, 64),
		["NavystoneHeart"]           = Eff(48, 64, 64, 48, 96, 64, 32, 48, 48),
		["NebulousHeart"]            = Eff(96, 112, 160, 160, 144, 192, 128, 176, 176),
		["NovaeSlugHeart"]           = Eff(48, 144, 160, 255, 160, 96, 240, 96, 80),
		["OceanHeart"]               = Eff(96, 160, 160, 64, 32, 32, 128, 96, 64),
		["PerennialHeart"]           = Eff(48, 160, 64, 144, 208, 96, 32, 96, 32),
		["PlantyMushHeart"]          = Eff(48, 64, 16, 16, 48, 16, 64, 32, 32),
		["PolarizedHeart"]           = Eff(176, 16, 64, 32, 32, 80, 224, 64, 48),
		["ProfanedHeart"]            = Eff(244, 198, 80, 210, 70, 50, 192, 160, 112),
		["RottenHeart"]              = Eff(96, 80, 80, 64, 48, 64, 96, 64, 128),
		["ScoriaHeart"]              = Eff(96, 0, 32, 128, 64, 64, 80, 80, 80),
		["SeaPrismHeart"]            = Eff(112, 192, 192, 128, 255, 255, 32, 112, 144),
		["SulphurousSandHeart"]      = Eff(160, 144, 80, 144, 128, 80, 96, 64, 64),
		["SulphurousSandstoneHeart"] = Eff(144, 128, 80, 112, 96, 64, 96, 64, 64),
		["TenebrisHeart"]            = Eff(32, 80, 80, 16, 64, 64, 0, 16, 32),
		["TwistingHeart"]            = Eff(96, 128, 160, 176, 224, 208, 64, 48, 64),
		["UelibloomHeart"]           = Eff(222, 172, 82, 96, 160, 64, 176, 112, 48),
		["VoidstoneHeart"]           = Eff(64, 128, 208, 96, 208, 240, 16, 16, 16),

		// ── Cross-mod: Thorium ───────────────────────────────────────────────────
		["AbyssalHeart"]          = Eff(64, 80, 48, 224, 224, 144, 16, 32, 16),
		["AquaiteHeart"]          = Eff(64, 160, 255, 48, 32, 144, 64, 224, 255),
		["BeholderHeart"]         = Eff(80, 64, 160, 144, 128, 192, 192, 64, 0),
		["BrackishClumpHeart"]    = Eff(64, 80, 96, 80, 144, 144, 48, 48, 64),
		["ChampionHeart"]         = Eff(255, 240, 144, 192, 192, 96, 96, 80, 48),
		["DepthsRockHeart"]       = Eff(80, 112, 144, 144, 176, 224, 48, 80, 112),
		["DormantHeart"]          = Eff(160, 192, 192, 224, 176, 128, 80, 64, 48),
		["IceboundStriderHeart"]  = Eff(64, 144, 224, 192, 224, 240, 32, 64, 112),
		["IllumiteHeart"]         = Eff(160, 80, 176, 240, 32, 128, 240, 128, 176),
		["LichHeart"]             = Eff(144, 80, 176, 224, 208, 255, 80, 16, 0),
		["LifeQuartzHeart"]       = Eff(240, 32, 96, 240, 128, 176, 128, 16, 48),
		["LodestoneHeart"]        = Eff(128, 96, 80, 192, 128, 112, 80, 48, 32),
		["MagmaHeart"]            = Eff(240, 80, 48, 240, 144, 48, 192, 32, 48),
		["MossyMarineRockHeart"]  = Eff(64, 128, 112, 112, 192, 160, 48, 96, 80),
		["OmegaHeart"]            = Eff(144, 0, 224, 192, 80, 255, 0, 128, 0),
		["OnyxHeart"]             = Eff(64, 64, 64, 96, 80, 96, 32, 32, 32),
		["OpalHeart"]             = Eff(150, 210, 230, 230, 170, 210, 200, 220, 200),
		["PearlHeart"]            = Eff(112, 144, 176, 192, 224, 240, 80, 192, 112),
		["PermafrostHeart"]       = Eff(96, 144, 160, 80, 112, 144, 64, 96, 112),
		["SeaBreezeHeart"]        = Eff(255, 112, 112, 255, 160, 128, 64, 64, 128),
		["SmoothCoalHeart"]       = Eff(32, 32, 32, 64, 64, 64, 16, 16, 16),
		["StormHeart"]            = Eff(0, 192, 255, 0, 128, 208, 96, 224, 255),
		["ThoriumHeart"]          = Eff(0, 176, 176, 48, 224, 255, 176, 128, 16),
		["ValadiumHeart"]         = Eff(144, 64, 208, 192, 128, 255, 80, 0, 255),
		["VampireHeart"]          = Eff(142, 42, 52, 48, 32, 64, 112, 64, 112),
		["YewWoodHeart"]          = Eff(112, 80, 64, 80, 48, 48, 64, 48, 32),
		["ZephyrHeart"]           = Eff(255, 240, 176, 208, 192, 160, 128, 96, 80),

		// ── Cross-mod: Consolaria ────────────────────────────────────────────────
		["CornucopiaHeart"]   = Eff(208, 128, 64, 112, 64, 32, 64, 48, 0),
		["CursedHeart"]       = Eff(128, 96, 64, 96, 64, 48, 48, 32, 16),
		["EasterHeart"]       = Eff(150, 220, 150, 240, 200, 160, 200, 150, 200),
		["SoulOfBlightHeart"] = Eff(240, 208, 80, 208, 160, 64, 144, 48, 16),

		// ── Vanilla: Potion Hearts ───────────────────────────────────────────────
		// Outer-ring colour comes from each potion's liquid/buff hue (the glass-grey bottle is
		// shared across them all, so it's dropped in favour of the colour that identifies the buff).
		["AmmoReservationHeart"]  = Eff(224, 144, 0, 255, 176, 32, 160, 64, 0),
		["ArcheryHeart"]          = Eff(224, 128, 16, 255, 160, 48, 112, 64, 16),
		["BattleHeart"]           = Eff(128, 96, 192, 160, 144, 208, 80, 64, 128),
		["BiomeSightHeart"]       = Eff(240, 112, 160, 128, 96, 208, 80, 64, 160),
		["BuilderHeart"]          = Eff(176, 128, 96, 224, 176, 128, 96, 64, 48),
		["CalmingHeart"]          = Eff(128, 144, 192, 128, 192, 224, 48, 64, 160),
		["CrateHeart"]            = Eff(208, 144, 64, 240, 176, 80, 96, 64, 32),
		["DangersenseHeart"]      = Eff(255, 112, 32, 255, 144, 96, 144, 32, 16),
		["EnduranceHeart"]        = Eff(232, 122, 42, 255, 160, 80, 160, 64, 16),  // blank liquid in sprite — buff colour
		["FeatherfallHeart"]      = Eff(160, 208, 255, 144, 224, 255, 16, 80, 112),
		["FishingHeart"]          = Eff(32, 144, 80, 112, 240, 160, 16, 64, 32),
		["FlipperHeart"]          = Eff(96, 192, 255, 160, 224, 255, 16, 112, 160),
		["GillsHeart"]            = Eff(16, 96, 160, 160, 208, 255, 16, 64, 112),
		["GravitationHeart"]      = Eff(64, 16, 128, 208, 160, 240, 32, 0, 64),
		["HeartreachHeart"]       = Eff(255, 160, 160, 144, 16, 128, 80, 0, 80),
		["HunterHeart"]           = Eff(192, 80, 16, 255, 128, 48, 96, 32, 0),
		["InfernoHeart"]          = Eff(255, 128, 0, 255, 144, 112, 128, 32, 0),
		["IronskinHeart"]         = Eff(176, 176, 48, 144, 176, 192, 96, 112, 128),
		["LifeforceHeart"]        = Eff(240, 16, 0, 255, 64, 48, 128, 16, 0),
		["LoveHeart"]             = Eff(232, 32, 64, 255, 120, 160, 112, 0, 0),
		["MagicPowerHeart"]       = Eff(128, 64, 224, 192, 160, 255, 48, 0, 96),
		["ManaRegenerationHeart"] = Eff(255, 144, 208, 144, 16, 80, 96, 0, 48),
		["MiningHeart"]           = Eff(80, 144, 160, 112, 160, 176, 48, 96, 96),
		["NightOwlHeart"]         = Eff(96, 144, 16, 128, 192, 16, 48, 80, 0),
		["ObsidianSkinHeart"]     = Eff(128, 112, 192, 160, 144, 208, 64, 64, 128),
		["RageHeart"]             = Eff(192, 16, 16, 224, 160, 32, 128, 16, 16),
		["RegenerationHeart"]     = Eff(255, 64, 160, 255, 160, 208, 224, 16, 128),
		["ShineHeart"]            = Eff(224, 224, 16, 255, 255, 128, 192, 192, 16),
		["SonarHeart"]            = Eff(112, 176, 16, 80, 144, 16, 64, 112, 16),
		["SpelunkerHeart"]        = Eff(224, 192, 16, 255, 224, 64, 144, 96, 16),
		["StinkHeart"]            = Eff(96, 128, 16, 80, 96, 16, 64, 80, 16),
		["SummoningHeart"]        = Eff(192, 224, 48, 144, 176, 32, 32, 48, 16),
		["SwiftnessHeart"]        = Eff(128, 224, 16, 208, 255, 144, 80, 144, 16),
		["ThornsHeart"]           = Eff(128, 160, 16, 160, 208, 0, 112, 144, 16),
		["TitanHeart"]            = Eff(112, 208, 64, 80, 144, 48, 64, 112, 32),
		["WarmthHeart"]           = Eff(255, 192, 0, 240, 144, 0, 255, 224, 64),
		["WaterWalkingHeart"]     = Eff(16, 96, 224, 96, 160, 255, 16, 64, 128),
		["WrathHeart"]            = Eff(192, 42, 52, 240, 128, 112, 128, 32, 32),
		["InvisibilityHeart"]     = Eff(200, 150, 255, 160, 100, 200, 220, 200, 255), // invisibility potion
		["LuckHeart"]             = Eff(255, 192, 200, 255, 128, 160, 255, 224, 224), // greater luck potion

		// ── Pacified Hearts ──────────────────────────────────────────────────────
		["CommonPacifiedHeart"]    = Eff(255, 60, 60, 255, 105, 180, 144, 16, 64),
		["UncommonPacifiedHeart"]  = Eff(48, 192, 64, 128, 224, 112, 16, 144, 16),
		["RarePacifiedHeart"]      = Eff(32, 96, 224, 96, 160, 240, 0, 48, 160),
		["EpicPacifiedHeart"]      = Eff(144, 32, 208, 176, 80, 240, 96, 0, 160),
		["LegendaryPacifiedHeart"] = Eff(240, 176, 0, 255, 192, 0, 192, 96, 0),
	};

	/// <summary>
	/// Whether <paramref name="heartId"/> has a hand-authored entry (as opposed to falling
	/// back to a hash-derived hue). Used by <see cref="HeartContentValidator"/> to flag hearts
	/// that ship on the generic fallback.
	/// </summary>
	internal static bool HasExplicit(string heartId) => Effects.ContainsKey(heartId);

	/// <summary>
	/// The effect theme for a heart. Unregistered ids get a stable hue derived from
	/// the id so a new or modded heart still looks intentional.
	/// </summary>
	public static HeartEffect Get(string heartId)
	{
		if (Effects.TryGetValue(heartId, out HeartEffect effect))
			return effect;

		int hash = 17;
		foreach (char c in heartId)
			hash = (hash * 31) + c;

		var rand = new System.Random(hash);
		return new HeartEffect(Main.hslToRgb((float)rand.NextDouble(), 0.7f, 0.6f));
	}
}
