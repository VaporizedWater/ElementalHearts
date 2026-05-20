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
	public readonly Color Primary;
	public readonly Color Secondary;
	public readonly bool Rainbow;

	public HeartEffect(Color primary)
	{
		Primary = primary;
		Secondary = primary;
		Rainbow = false;
	}

	public HeartEffect(Color primary, Color secondary)
	{
		Primary = primary;
		Secondary = secondary;
		Rainbow = false;
	}

	private HeartEffect(bool rainbow)
	{
		Primary = Color.White;
		Secondary = Color.White;
		Rainbow = rainbow;
	}

	/// <summary>A heart whose particles cycle the full hue wheel instead of using fixed colours.</summary>
	public static HeartEffect Prismatic => new(rainbow: true);

	/// <summary>Picks a particle colour: a random hue if prismatic, otherwise one of the two theme colours.</summary>
	public Color Pick(UnifiedRandom rand)
	{
		if (Rainbow)
			return Main.hslToRgb(rand.NextFloat(), 1f, 0.62f);

		return rand.NextBool() ? Primary : Secondary;
	}
}

/// <summary>
/// Hard-coded per-heart colour themes for the consumption effect. Each heart's outer
/// particle burst is tinted to its material; add or tweak an entry here to restyle a
/// heart. Hearts with no entry fall back to a stable hue derived from their id.
/// </summary>
public static class HeartEffectRegistry
{
	private static HeartEffect Eff(int r, int g, int b) => new(new Color(r, g, b));

	private static HeartEffect Eff(int r1, int g1, int b1, int r2, int g2, int b2) =>
		new(new Color(r1, g1, b1), new Color(r2, g2, b2));

	private static readonly Dictionary<string, HeartEffect> Effects = new()
	{
		// ── Vanilla: Common ──────────────────────────────────────────────────────
		["BorealWoodHeart"]  = Eff(150, 175, 195),
		["BubbleHeart"]      = Eff(130, 205, 240),
		["CactusHeart"]      = Eff(95, 160, 75),
		["CandyCaneHeart"]   = Eff(225, 55, 60, 245, 245, 245),
		["CloudHeart"]       = Eff(240, 245, 255),
		["CoralstoneHeart"]  = Eff(240, 135, 120),
		["CrimsandHeart"]    = Eff(155, 60, 55),
		["CrimstoneHeart"]   = Eff(165, 45, 50),
		["DirtHeart"]        = Eff(150, 100, 60),
		["DynastyHeart"]     = Eff(195, 65, 60),
		["EbonsandHeart"]    = Eff(85, 75, 110),
		["EbonstoneHeart"]   = Eff(80, 68, 105),
		["EbonwoodHeart"]    = Eff(92, 82, 108),
		["FleshHeart"]       = Eff(220, 120, 120),
		["FossilHeart"]      = Eff(195, 145, 90),
		["GlassHeart"]       = Eff(200, 235, 240),
		["GraniteHeart"]     = Eff(72, 82, 112),
		["HayHeart"]         = Eff(220, 190, 85),
		["HoneyHeart"]       = Eff(232, 170, 45),
		["IceHeart"]         = Eff(175, 222, 240),
		["LesionHeart"]      = Eff(82, 132, 92),
		["MarbleHeart"]      = Eff(235, 235, 240),
		["MudHeart"]         = Eff(112, 82, 60),
		["MushroomHeart"]    = Eff(115, 150, 230),
		["ObsidianHeart"]    = Eff(55, 48, 70),
		["PalmWoodHeart"]    = Eff(182, 142, 92),
		["PearlsandHeart"]   = Eff(240, 222, 230),
		["PearlstoneHeart"]  = Eff(235, 200, 222),
		["PearlwoodHeart"]   = Eff(240, 226, 236),
		["PinkIceHeart"]     = Eff(240, 172, 212),
		["PumpkinHeart"]     = Eff(232, 130, 38),
		["PurpleIceHeart"]   = Eff(190, 152, 232),
		["RainCloudHeart"]   = Eff(122, 142, 172),
		["RainbowHeart"]     = HeartEffect.Prismatic,
		["RedIceHeart"]      = Eff(222, 92, 92),
		["RichMahoganyHeart"]= Eff(162, 82, 60),
		["SandHeart"]        = Eff(226, 202, 132),
		["ShadewoodHeart"]   = Eff(122, 122, 132),
		["SlimeHeart"]       = Eff(112, 162, 232),
		["SnowCloudHeart"]   = Eff(236, 240, 250),
		["SnowHeart"]        = Eff(242, 246, 255),
		["SpookyHeart"]      = Eff(102, 112, 96),
		["StoneHeart"]       = Eff(132, 132, 138),
		["SunplateHeart"]    = Eff(242, 216, 112),
		["WoodHeart"]        = Eff(156, 112, 66),

		// ── Vanilla: Uncommon ────────────────────────────────────────────────────
		["AmethystHeart"]    = Eff(152, 92, 222),
		["CopperHeart"]      = Eff(202, 112, 62),
		["EmeraldHeart"]     = Eff(52, 192, 102),
		["EnchantedHeart"]   = Eff(112, 152, 232),
		["GoldHeart"]        = Eff(242, 202, 72),
		["IronHeart"]        = Eff(142, 132, 126),
		["LeadHeart"]        = Eff(112, 112, 122),
		["PlatinumHeart"]    = Eff(222, 226, 236),
		["RubyHeart"]        = Eff(222, 52, 72),
		["SapphireHeart"]    = Eff(62, 112, 212),
		["SilverHeart"]      = Eff(202, 206, 216),
		["TinHeart"]         = Eff(192, 186, 166),
		["TopazHeart"]       = Eff(232, 182, 62),
		["TungstenHeart"]    = Eff(172, 176, 162),

		// ── Vanilla: Rare ────────────────────────────────────────────────────────
		["AmberHeart"]       = Eff(222, 142, 32),
		["BrainHeart"]       = Eff(222, 92, 112),
		["CobaltHeart"]      = Eff(62, 122, 212),
		["CogHeart"]         = Eff(172, 132, 82),
		["CrimtaneHeart"]    = Eff(202, 56, 62),
		["CursedFlameHeart"] = Eff(122, 222, 122),
		["DarkHeart"]        = Eff(72, 62, 92),
		["DemoniteHeart"]    = Eff(112, 92, 162),
		["DiamondHeart"]     = Eff(192, 232, 246),
		["DiscordHeart"]     = Eff(150, 70, 200, 230, 90, 180),
		["HellstoneHeart"]   = Eff(232, 92, 42),
		["IchorHeart"]       = Eff(242, 212, 72),
		["MeteoriteHeart"]   = Eff(132, 112, 132),
		["MythrilHeart"]     = Eff(82, 192, 152),
		["OrichalcumHeart"]  = Eff(222, 112, 202),
		["PalladiumHeart"]   = Eff(232, 122, 72),
		["SoulOfFlightHeart"]= Eff(202, 240, 246),
		["SoulOfLightHeart"] = Eff(246, 240, 192),
		["SoulOfNightHeart"] = Eff(112, 82, 152),
		["WormHeart"]        = Eff(192, 62, 66),

		// ── Vanilla: Epic ────────────────────────────────────────────────────────
		["AdamantiteHeart"]  = Eff(228, 82, 62),
		["ChlorophyteHeart"] = Eff(122, 202, 52),
		["CrystalHeart"]     = Eff(232, 92, 132),
		["HallowedHeart"]    = Eff(246, 240, 172),
		["MechanicalHeart"]  = Eff(122, 162, 172),
		["SoulOfFrightHeart"]= Eff(212, 72, 72),
		["SoulOfMightHeart"] = Eff(232, 212, 92),
		["SoulOfSightHeart"] = Eff(92, 152, 222),
		["TitaniumHeart"]    = Eff(202, 206, 212),

		// ── Vanilla: Legendary ───────────────────────────────────────────────────
		["EctoplasmHeart"]   = Eff(152, 232, 222),
		["LuminiteHeart"]    = Eff(202, 182, 232),
		["ShroomiteHeart"]   = Eff(82, 182, 202),
		["SpectreHeart"]     = Eff(162, 222, 216),

		// ── Vanilla: Exotic (boss-themed) ────────────────────────────────────────
		["BetsyHeart"]          = Eff(152, 132, 222),
		["BrainOfCthulhuHeart"] = Eff(222, 92, 112),
		["DeerclopsHeart"]      = Eff(192, 212, 232),
		["DestroyerHeart"]      = Eff(142, 142, 152),
		["DukeFishronHeart"]    = Eff(82, 172, 182),
		["ElfHeart"]            = Eff(216, 60, 62, 60, 170, 80),
		["EmpressOfLightHeart"] = HeartEffect.Prismatic,
		["EyeOfCthulhuHeart"]   = Eff(202, 62, 62),
		["FlyingDutchmanHeart"] = Eff(122, 112, 92),
		["GolemHeart"]          = Eff(222, 142, 52),
		["KingSlimeHeart"]      = Eff(92, 142, 222),
		["LunaticCultistHeart"] = Eff(232, 216, 132),
		["MartianSaucerHeart"]  = Eff(122, 222, 162),
		["MenacingHeart"]       = Eff(152, 42, 52),
		["MoonLordHeart"]       = Eff(162, 232, 222),
		["MourningWoodHeart"]   = Eff(232, 122, 52),
		["PlanteraHeart"]       = Eff(232, 92, 132),
		["PumpkingHeart"]       = Eff(226, 122, 42),
		["QueenBeeHeart"]       = Eff(240, 200, 60, 60, 55, 50),
		["QueenSlimeHeart"]     = Eff(222, 112, 202),
		["RazorpineHeart"]      = Eff(72, 152, 92),
		["SkeletronHeart"]      = Eff(226, 226, 216),
		["WallOfFleshHeart"]    = Eff(192, 72, 62),

		// ── Vanilla: Mythic ──────────────────────────────────────────────────────
		["ZenithHeart"]      = HeartEffect.Prismatic,

		// ── Cross-mod: Calamity ──────────────────────────────────────────────────
		["AbyssGravelHeart"]                   = Eff(42, 52, 82),
		["AerialiteHeart"]                     = Eff(102, 202, 222),
		["AfflictedHeart"]                     = Eff(152, 202, 62),
		["AmbergrisHeart"]                     = Eff(202, 176, 132),
		["AquaticHeart"]                       = Eff(62, 132, 202),
		["ArmoredHeart"]                       = Eff(172, 132, 82),
		["AstralBossHeart"]                    = Eff(182, 152, 222),
		["AstralClayHeart"]                    = Eff(172, 142, 202),
		["AstralDirtHeart"]                    = Eff(162, 132, 192),
		["AstralHeart"]                        = Eff(192, 162, 232),
		["AstralIceHeart"]                     = Eff(182, 202, 232),
		["AstralMonolithHeart"]                = Eff(152, 132, 202),
		["AstralSandHeart"]                    = Eff(202, 176, 216),
		["AstralSandstoneHeart"]               = Eff(192, 166, 212),
		["AstralSnowHeart"]                    = Eff(212, 202, 236),
		["AstralStoneHeart"]                   = Eff(162, 146, 196),
		["AuricHeart"]                         = Eff(246, 216, 92),
		["BlazingHeart"]                       = Eff(236, 102, 42),
		["BloodyWormHeart"]                    = Eff(172, 42, 46),
		["BrimstoneSlagHeart"]                 = Eff(202, 62, 52),
		["CalamitousHeart"]                    = Eff(122, 52, 72),
		["CelestialRemainsHeart"]              = Eff(142, 212, 222),
		["CinderplateHeart"]                   = Eff(182, 92, 52),
		["CorpusHeart"]                        = Eff(162, 52, 56),
		["CosmiliteHeart"]                     = Eff(112, 92, 202),
		["CryogenHeart"]                       = Eff(152, 216, 236),
		["CryonicHeart"]                       = Eff(132, 202, 232),
		["CrystallizedToxicHeart"]             = Eff(142, 212, 82),
		["DarkPlasmicHeart"]                   = Eff(152, 72, 182),
		["DraconicHeart"]                      = Eff(236, 152, 52),
		["DynamoStemHeart"]                    = Eff(122, 212, 222),
		["EutrophicSandHeart"]                 = Eff(72, 132, 92),
		["ExodiumClusterHeart"]                = Eff(142, 132, 212),
		["FungalHeart"]                        = Eff(112, 162, 202),
		["GehennaHeart"]                       = Eff(202, 72, 52),
		["GravistarHeart"]                     = Eff(132, 112, 202),
		["HardenedAstralSandHeart"]            = Eff(172, 152, 202),
		["HardenedSulphurousSandstoneHeart"]   = Eff(212, 196, 92),
		["MutatedHeart"]                       = Eff(132, 202, 92),
		["NavystoneHeart"]                     = Eff(62, 102, 152),
		["NebulousHeart"]                      = Eff(172, 92, 202),
		["NovaeSlugHeart"]                     = Eff(212, 132, 182),
		["OceanHeart"]                         = Eff(42, 92, 162),
		["PerennialHeart"]                     = Eff(112, 202, 102),
		["PlantyMushHeart"]                    = Eff(122, 182, 152),
		["PolarizedHeart"]                     = Eff(132, 92, 192),
		["ProfanedHeart"]                      = Eff(244, 198, 80, 210, 70, 50),
		["RottenHeart"]                        = Eff(122, 112, 62),
		["ScoriaHeart"]                        = Eff(212, 92, 52),
		["SeaPrismHeart"]                      = Eff(92, 212, 202),
		["SulphurousSandHeart"]                = Eff(226, 206, 96),
		["SulphurousSandstoneHeart"]           = Eff(216, 196, 102),
		["TenebrisHeart"]                      = Eff(62, 52, 92),
		["TwistingHeart"]                      = Eff(152, 102, 192),
		["UelibloomHeart"]                     = Eff(222, 172, 82),
		["VoidstoneHeart"]                     = Eff(46, 52, 78),

		// ── Cross-mod: Thorium ───────────────────────────────────────────────────
		["AbyssalHeart"]          = Eff(42, 62, 92),
		["AquaiteHeart"]          = Eff(82, 172, 212),
		["BeholderHeart"]         = Eff(142, 92, 172),
		["BrackishClumpHeart"]    = Eff(92, 142, 132),
		["ChampionHeart"]         = Eff(232, 202, 102),
		["DepthsRockHeart"]       = Eff(82, 92, 112),
		["DormantHeart"]          = Eff(102, 122, 182),
		["IceboundStriderHeart"]  = Eff(162, 216, 236),
		["IllumiteHeart"]         = Eff(122, 232, 222),
		["LichHeart"]             = Eff(122, 182, 112),
		["LifeQuartzHeart"]       = Eff(232, 112, 142),
		["LodestoneHeart"]        = Eff(152, 92, 92),
		["MagmaHeart"]            = Eff(226, 102, 42),
		["MossyMarineRockHeart"]  = Eff(92, 152, 112),
		["OmegaHeart"]            = Eff(92, 172, 212),
		["OnyxHeart"]             = Eff(52, 52, 62),
		["OpalHeart"]             = Eff(150, 210, 230, 230, 170, 210),
		["PearlHeart"]            = Eff(242, 236, 230),
		["PermafrostHeart"]       = Eff(172, 222, 242),
		["SeaBreezeHeart"]        = Eff(162, 212, 232),
		["SmoothCoalHeart"]       = Eff(46, 46, 52),
		["StormHeart"]            = Eff(202, 202, 122),
		["ThoriumHeart"]          = Eff(72, 182, 162),
		["ValadiumHeart"]         = Eff(172, 202, 82),
		["VampireHeart"]          = Eff(142, 42, 52),
		["YewWoodHeart"]          = Eff(142, 102, 72),
		["ZephyrHeart"]           = Eff(182, 222, 226),

		// ── Cross-mod: Consolaria ────────────────────────────────────────────────
		["CornucopiaHeart"]   = Eff(222, 152, 62),
		["CursedHeart"]       = Eff(92, 72, 122),
		["EasterHeart"]       = Eff(240, 170, 200, 150, 220, 150),
		["SoulOfBlightHeart"] = Eff(132, 162, 92),
	};

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
