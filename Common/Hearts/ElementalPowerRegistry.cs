using System.Collections.Generic;

namespace ElementalHearts.Common.Hearts;

/// <summary>
/// Curated elemental-power names shown in the "the elemental power of … has been activated" tooltip.
/// Every entry is a hand-written, lighthearted pun that fits the heart's material/theme — never the
/// lowercased-class-name fallback (that path exists only so the game never crashes, and a DEBUG
/// <see cref="HeartContentValidator"/> warning fires for any heart that hits it).
/// </summary>
internal static class ElementalPowerRegistry
{
	private static readonly Dictionary<string, string> Powers = new()
	{
		// ── Common: blocks & terrain ──────────────────────────────────────────────
		["AcornHeart"] = "mighty oak",          // from tiny acorns…
		["BubbleHeart"] = "bubble trouble",
		["BorealWoodHeart"] = "up north",
		["CactusHeart"] = "stay sharp",
		["CandyCaneHeart"] = "sugar rush",
		["CloudHeart"] = "cloud nine",
		["CoralstoneHeart"] = "reef madness",   // Reefer Madness
		["CrimsandHeart"] = "blood in the sand",
		["CrimstoneHeart"] = "crimson tide",
		["DirtHeart"] = "dirt cheap",
		["DynastyHeart"] = "dynasty warriors",
		["EbonsandHeart"] = "the sandman",
		["EbonstoneHeart"] = "rotten to the core",
		["EbonwoodHeart"] = "knock on deadwood",
		["FleshHeart"] = "fresh meat",
		["FossilHeart"] = "blast from the past",
		["GemcornHeart"] = "diamond in the rough",
		["GlassHeart"] = "glass cannon",
		["GraniteHeart"] = "taken for granite",
		["HayHeart"] = "hay day",
		["HoneyHeart"] = "the bee's knees",
		["IceHeart"] = "ice ice baby",
		["MarbleHeart"] = "lost your marbles",
		["MudHeart"] = "stick in the mud",
		["MushroomHeart"] = "1-up",              // Mario's mushroom
		["ObsidianHeart"] = "cutting edge",      // obsidian = sharpest natural blade
		["PalmWoodHeart"] = "palm reading",
		["PearlsandHeart"] = "pearly sands",
		["PearlstoneHeart"] = "holier than thou",
		["PearlwoodHeart"] = "pearly whites",
		["PinkIceHeart"] = "tickled pink",
		["PumpkinHeart"] = "pumpkin spice",
		["PurpleIceHeart"] = "purple reign",
		["RainbowHeart"] = "taste the rainbow",
		["RainCloudHeart"] = "under the weather",
		["RedIceHeart"] = "cherry slush",
		["RichMahoganyHeart"] = "stay classy",   // Anchorman's "rich mahogany"
		["SandHeart"] = "sands of time",
		["ShadewoodHeart"] = "shady business",
		["SlimeHeart"] = "slime time",
		["SnowCloudHeart"] = "let it snow",
		["SnowHeart"] = "cold shoulder",
		["SpookyHeart"] = "spooktacular",
		["StoneHeart"] = "stone cold",
		["SunplateHeart"] = "sunny side up",
		["WoodHeart"] = "knock on wood",

		// ── Common: colours & gels ────────────────────────────────────────────────
		["BlueHeart"] = "feeling blue",
		["GreenHeart"] = "green with envy",
		["TealHeart"] = "teal we meet again",

		// ── Common: herbs ─────────────────────────────────────────────────────────
		["BlinkrootHeart"] = "blink and you'll miss it",
		["DaybloomHeart"] = "rise and shine",
		["DeathweedHeart"] = "pushing daisies",
		["FireblossomHeart"] = "trial by fire",
		["MoonglowHeart"] = "glow in the dark",
		["ShiverthornHeart"] = "the shivers",
		["WaterleafHeart"] = "make it rain",

		// ── Common: mushrooms & spores ────────────────────────────────────────────
		["GlowingMushroomHeart"] = "spore loser",
		["JungleSporeHeart"] = "spore wars",
		["ViciousMushroomHeart"] = "bad trip",
		["VileMushroomHeart"] = "spore-adic",
		["ShardHeart"] = "shard to say",

		// ── Common: fruit (the orchard) ───────────────────────────────────────────
		["AppleHeart"] = "doctor away",          // an apple a day…
		["ApricotHeart"] = "pit stop",
		["BananaHeart"] = "bananza",
		["BlackcurrantHeart"] = "current events",
		["BloodOrangeHeart"] = "pulp fiction",
		["CherryHeart"] = "cherry on top",
		["CoconutHeart"] = "coco loco",
		["DragonFruitHeart"] = "dragon's breath",
		["ElderberryHeart"] = "smelt of elderberries",   // Monty Python
		["GrapeHeart"] = "sour grapes",
		["GrapefruitHeart"] = "sour face",
		["LemonHeart"] = "life gives you lemons",
		["LimeHeart"] = "limelight",
		["MangoHeart"] = "it takes two to mango",
		["OrangeHeart"] = "orange you glad",
		["PeachHeart"] = "peachy keen",
		["PineappleHeart"] = "pizza topping",
		["PlumHeart"] = "plum tuckered",
		["PomegranateHeart"] = "seedy character",
		["RambutanHeart"] = "hairy situation",
		["StarFruitHeart"] = "star of the show",

		// ── Uncommon: ores & gems ─────────────────────────────────────────────────
		["AmethystHeart"] = "purple haze",
		["CopperHeart"] = "tarnished",
		["EmeraldHeart"] = "emerald city",
		["EncumberingHeart"] = "heavy burden",
		["TreasureHeart"] = "finders keepers",
		["GoldHeart"] = "gold rush",
		["IronHeart"] = "pump iron",
		["LeadHeart"] = "heavy metal",
		["PlatinumHeart"] = "going platinum",
		["RubyHeart"] = "ruby tuesday",
		["SapphireHeart"] = "blue blood",
		["SilverHeart"] = "hi-ho silver",
		["TinHeart"] = "tin man",                // the Tin Man wanted a heart
		["TopazHeart"] = "golden hour",
		["TungstenHeart"] = "heavy duty",

		// ── Rare ──────────────────────────────────────────────────────────────────
		["AmberHeart"] = "stuck in time",
		["BrainHeart"] = "brainiac",
		["CobaltHeart"] = "cobalt blues",
		["CogHeart"] = "cog in the machine",
		["CrimtaneHeart"] = "blood rush",
		["CrystalHeart"] = "crystal clear",
		["LifeFruitHeart"] = "forbidden fruit",
		["CursedFlameHeart"] = "burn notice",
		["DarkHeart"] = "dark side",
		["DemoniteHeart"] = "demon time",
		["DiamondHeart"] = "unbreakable",
		["DiscordHeart"] = "now you see me",     // Rod of Discord teleport
		["EnchantedHeart"] = "spellbound",
		["HellstoneHeart"] = "hot to the touch",
		["IchorHeart"] = "golden blood",         // ichor = blood of the gods
		["LesionHeart"] = "get that checked",
		["MeteoriteHeart"] = "shooting star",
		["MythrilHeart"] = "myth busted",
		["OrichalcumHeart"] = "pretty in pink",
		["PalladiumHeart"] = "catalytic converter",
		["SoulOfFlightHeart"] = "fly away",
		["SoulOfFrightHeart"] = "scared stiff",
		["SoulOfLightHeart"] = "lighten up",
		["SoulOfMightHeart"] = "feeling mighty",
		["SoulOfNightHeart"] = "nightlife",
		["SoulOfSightHeart"] = "eagle eye",
		["WormHeart"] = "early bird",            // …gets the worm

		// ── Epic ──────────────────────────────────────────────────────────────────
		["AdamantiteHeart"] = "won't budge",
		["ChlorophyteHeart"] = "photosynthesis",
		["HallowedHeart"] = "holy moly",
		["MechanicalHeart"] = "well-oiled",
		["TitaniumHeart"] = "unsinkable",        // …like a certain ship

		// ── Legendary ─────────────────────────────────────────────────────────────
		["EctoplasmHeart"] = "who you gonna call",
		["LuminiteHeart"] = "moonstruck",
		["ShroomiteHeart"] = "fungus among us",
		["SpectreHeart"] = "ghost mode",
		// Lunar bricks — each is a moon-phase joke (work, comedy timing, royalty, decay)
		["AstraHeart"] = "moonlighting",         // Third Quarter — late shift / second job by moonlight
		["CosmicEmberHeart"] = "slow burn",      // Waxing Gibbous — slow growth + ember combustion
		["CryocoreHeart"] = "cold open",         // First Quarter — the opening; comedy term + literally cold
		["DarkCelestialHeart"] = "lights out",   // Waning Crescent — almost-gone moon = bedtime
		["HeavenforgeHeart"] = "overtime",       // Full Moon — peak shift at the celestial forge
		["LunarRustHeart"] = "moonrot",          // Waning Gibbous — wane = decay; rust pun
		["MercuryHeart"] = "mood swing",         // New Moon — mercurial + dark new-moon mood
		["StarRoyaleHeart"] = "moonarch",        // Waxing Crescent — moon + monarch; crescent = crown

		// ── Exotic (boss-themed) ──────────────────────────────────────────────────
		["BetsyHeart"] = "here be dragons",
		["BrainOfCthulhuHeart"] = "brainstorm",
		["DeerclopsHeart"] = "oh deer",
		["TheDestroyerHeart"] = "demolition",
		["DukeFishronHeart"] = "something's fishy",
		["EaterOfWorldsHeart"] = "worm food",
		["ElfHeart"] = "on the shelf",
		["ClayHeart"] = "clay pigeon",
		["SkeletronPrimeHeart"] = "prime time",
		["TheTwinsHeart"] = "seeing double",
		["EmpressOfLightHeart"] = "blinding",
		["EyeOfCthulhuHeart"] = "eye see you",
		["FlyingDutchmanHeart"] = "yo ho ho",
		["GolemHeart"] = "rock solid",
		["JackOLanternHeart"] = "this is halloween",
		["KingSlimeHeart"] = "long live the king",
		["LunaticCultistHeart"] = "cult classic",
		["MagnificationHeart"] = "enhance",      // the CSI "zoom… enhance" — it literally magnifies
		["MartianSaucerHeart"] = "phone home",
		["MoonLordHeart"] = "over the moon",
		["MourningWoodHeart"] = "good mourning",
		["PlanteraHeart"] = "plant food",
		["PumpkingHeart"] = "smashing pumpkins",
		["QueenBeeHeart"] = "royal jelly",
		["QueenSlimeHeart"] = "drama queen",
		["RazorpineHeart"] = "pins and needles",
		["SkeletronHeart"] = "bad to the bone",
		["WallOfFleshHeart"] = "off the wall",

		// ── Mythic ────────────────────────────────────────────────────────────────
		["ZenithHeart"] = "kitchen sink",        // …everything but the. The Zenith is every sword at once

		// ── Boss spawners: Menacing (an escalating-rage arc — the foil to Pacified) ─
		["CommonMenacingHeart"]    = "spoiling for a fight",
		["UncommonMenacingHeart"]  = "picking a fight",
		["RareMenacingHeart"]      = "throwing hands",
		["EpicMenacingHeart"]      = "blind rage",
		["LegendaryMenacingHeart"] = "absolutely feral",

		// ── Pacified Hearts (the Animate bosses, talked down from a rampage) ───────
		// An anger-management arc that levels up with the tier: from first therapy session
		// to full lotus-position enlightenment.
		["CommonPacifiedHeart"]    = "deep breaths",
		["UncommonPacifiedHeart"]  = "anger management",
		["RarePacifiedHeart"]      = "inner peace",
		["EpicPacifiedHeart"]      = "zen mode",
		["LegendaryPacifiedHeart"] = "enlightenment",

		// ── Cross-mod: Calamity ───────────────────────────────────────────────────
		["AbyssGravelHeart"] = "rock bottom",
		["AerialiteHeart"] = "head in the clouds",
		["AfflictedHeart"] = "patient zero",
		["AmbergrisHeart"] = "whale of a time",
		["AquaticHeart"] = "release the kraken",
		["ArmoredHeart"] = "knight life",
		["AstralBossHeart"] = "star god",        // Astrum Deus / Aureus
		["AstralClayHeart"] = "cosmic clay",
		["AstralDirtHeart"] = "cosmic dirt",
		["AstralHeart"] = "astral projection",
		["AstralIceHeart"] = "cosmic cold",
		["AstralMonolithHeart"] = "space odyssey",   // 2001's monolith
		["AstralSandHeart"] = "cosmic sands",
		["AstralSandstoneHeart"] = "stellar sandstone",
		["AstralSnowHeart"] = "cosmic flurry",
		["AstralStoneHeart"] = "space rock",
		["AuricHeart"] = "midas touch",          // auric = gold; also Auric Goldfinger
		["BlazingHeart"] = "blaze of glory",
		["BloodyWormHeart"] = "blood and guts",
		["BrimstoneSlagHeart"] = "fire and brimstone",
		["CalamitousHeart"] = "what a disaster",
		["CelestialRemainsHeart"] = "celestial leftovers",
		["CinderplateHeart"] = "burnt to a cinder",
		["CorpusHeart"] = "blood bank",
		["CosmiliteHeart"] = "cosmic horror",
		["CryogenHeart"] = "deep freeze",
		["CryonicHeart"] = "brain freeze",
		["CrystallizedToxicHeart"] = "toxic relationship",
		["DarkPlasmicHeart"] = "dark matter",
		["DraconicHeart"] = "dragon's breath",
		["DynamoStemHeart"] = "fully charged",
		["EutrophicSandHeart"] = "deep blue",
		["ExodiumClusterHeart"] = "exit strategy",
		["FungalHeart"] = "mushroom cloud",
		["GehennaHeart"] = "hellscape",
		["GravistarHeart"] = "what goes up",
		["HardenedAstralSandHeart"] = "set in stars",
		["MutatedHeart"] = "x-gene",
		["NavystoneHeart"] = "deep sea",
		["NebulousHeart"] = "spaced out",
		["NovaeSlugHeart"] = "supernova",
		["OceanHeart"] = "the deep end",
		["PerennialHeart"] = "evergreen",
		["PlantyMushHeart"] = "mulch ado",
		["PolarizedHeart"] = "opposites attract",
		["ProfanedHeart"] = "unholy",
		["RottenHeart"] = "rotten luck",
		["ScoriaHeart"] = "deep fried",
		["SeaPrismHeart"] = "ocean view",
		["SulphurousSandHeart"] = "rotten eggs",
		["SulphurousSandstoneHeart"] = "stink bomb",
		["TenebrisHeart"] = "into darkness",
		["TwistingHeart"] = "plot twist",
		["UelibloomHeart"] = "late bloomer",
		["VoidstoneHeart"] = "into the void",

		// ── Cross-mod: Consolaria ─────────────────────────────────────────────────
		["CornucopiaHeart"] = "horn of plenty",
		["CursedHeart"] = "hex appeal",
		["EasterHeart"] = "egg hunt",
		["SoulOfBlightHeart"] = "blight night",

		// ── Vanilla: Potion Hearts (active buffs) ─────────────────────────────────
		["AmmoReservationHeart"]  = "ammo to spare",
		["ArcheryHeart"]          = "bullseye",
		["BattleHeart"]           = "pick a fight",
		["BiomeSightHeart"]       = "x-ray vision",
		["BuilderHeart"]          = "bob the builder",
		["CalmingHeart"]          = "chill pill",
		["CrateHeart"]            = "loot box",
		["DangersenseHeart"]      = "spidey sense",
		["EnduranceHeart"]        = "tough it out",
		["FeatherfallHeart"]      = "float on",
		["FishingHeart"]          = "gone fishin'",
		["FlipperHeart"]          = "just keep swimming",
		["GillsHeart"]            = "fishy breath",
		["GravitationHeart"]      = "upside down",
		["HeartreachHeart"]       = "reach out",
		["HunterHeart"]           = "thrill of the hunt",
		["InfernoHeart"]          = "ring of fire",
		["IronskinHeart"]         = "thick skinned",
		["LifeforceHeart"]        = "feel alive",
		["LoveHeart"]             = "lovestruck",
		["MagicPowerHeart"]       = "abracadabra",
		["ManaRegenerationHeart"] = "mana from heaven",
		["MiningHeart"]           = "heigh-ho",
		["NightOwlHeart"]         = "night owl",
		["ObsidianSkinHeart"]     = "fireproof",
		["RageHeart"]             = "see red",
		["RegenerationHeart"]     = "walk it off",
		["ShineHeart"]            = "glow stick",
		["SonarHeart"]            = "ping!",
		["SpelunkerHeart"]        = "all that glitters",
		["StinkHeart"]            = "pee-yew",
		["SummoningHeart"]        = "minion mode",
		["SwiftnessHeart"]        = "gotta go fast",
		["ThornsHeart"]           = "prickly",
		["TitanHeart"]            = "clash of titans",
		["WarmthHeart"]           = "warm and fuzzy",
		["WaterWalkingHeart"]     = "walk on water",
		["WrathHeart"]            = "grapes of wrath",
		["InvisibilityHeart"]     = "now you don't",
		["LuckHeart"]             = "lucky break",

		// ── Cross-mod: Thorium ────────────────────────────────────────────────────
		["AbyssalHeart"] = "abyssal gaze",
		["AquaiteHeart"] = "go with the flow",
		["BeholderHeart"] = "eye on you",        // beholder = giant eye
		["BrackishClumpHeart"] = "swamp thing",
		["ChampionHeart"] = "we are the champions",
		["DepthsRockHeart"] = "down under",
		["DormantHeart"] = "wake up call",
		["IceboundStriderHeart"] = "ice walker",
		["IllumiteHeart"] = "enlightened",
		["LifeQuartzHeart"] = "crystal healing",
		["LichHeart"] = "lich king",
		["LodestoneHeart"] = "magnetic personality",
		["MagmaHeart"] = "the floor is lava",
		["MossyMarineRockHeart"] = "gathers no moss",   // a rolling stone…
		["OmegaHeart"] = "the final word",       // omega = the last letter
		["OnyxHeart"] = "pitch black",
		["OpalHeart"] = "play of color",
		["PearlHeart"] = "pearls of wisdom",
		["PermafrostHeart"] = "ice age",
		["SeaBreezeHeart"] = "breath of fresh air",
		["SmoothCoalHeart"] = "naughty list",    // coal in the stocking
		["StormHeart"] = "eye of the storm",
		["ThoriumHeart"] = "going nuclear",      // thorium = nuclear fuel
		["ValadiumHeart"] = "valedictorian",
		["VampireHeart"] = "bite me",
		["YewWoodHeart"] = "yew again",          // you / yew
		["ZephyrHeart"] = "gone with the wind",
	};

	public static string Get(string heartName) =>
		Powers.TryGetValue(heartName, out string power) ? power : heartName.ToLowerInvariant();

	/// <summary>
	/// Whether <paramref name="heartName"/> has a curated power word (as opposed to falling
	/// back to its lower-cased class name). Used by <see cref="HeartContentValidator"/>.
	/// </summary>
	internal static bool HasExplicit(string heartName) => Powers.ContainsKey(heartName);
}
