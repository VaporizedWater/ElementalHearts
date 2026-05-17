using System.Collections.Generic;
using ElementalHearts.Content.Items.Hearts.CrossMod.Calamity;
using ElementalHearts.Content.Items.Hearts.CrossMod.Consolaria;
using ElementalHearts.Content.Items.Hearts.CrossMod.Thorium;
using ElementalHearts.Content.Items.Hearts.Exotic;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Common.Systems;

/// <summary>
/// Maps boss NPC types to boss heart item drops. Built once in <see cref="Build"/>.
/// </summary>
public static class BossHeartDropRegistry
{
	private static readonly Dictionary<int, List<int>> DropsByNpc = new();

	public static IEnumerable<int> GetDrops(int npcType) =>
		DropsByNpc.TryGetValue(npcType, out List<int> drops) ? drops : [];

	public static void Build()
	{
		DropsByNpc.Clear();
		RegisterVanilla();
		RegisterCalamity();
		RegisterThorium();
		RegisterConsolaria();
	}

	private static void RegisterVanilla()
	{
		Add<EyeHeart>(NPCID.EyeofCthulhu);
		Add<RoyalSlimeHeart>(NPCID.KingSlime);
		Add<VolatileHeart>(NPCID.EaterofWorldsHead);
		Add<SnotHeart>(NPCID.BrainofCthulhu);
		Add<HiveHeart>(NPCID.QueenBee);
		Add<BoneHeart>(NPCID.SkeletronHead);
		Add<WallOfFleshHeart>(NPCID.WallofFlesh);
		Add<QueenSlimeHeart>(NPCID.QueenSlimeBoss);
		Add<SlaughterHeart>(NPCID.TheDestroyer);
		Add<PlantHeart>(NPCID.Plantera);
		Add<TruffleHeart>(NPCID.Plantera);
		Add<LihzahrdHeart>(NPCID.Golem);
		Add<DukeFishronHeart>(NPCID.DukeFishron);
		Add<EmpressHeart>(NPCID.HallowBoss);
		Add<AncientHeart>(NPCID.CultistBoss);
		Add<CelestialHeart>(NPCID.MoonLordCore);
		Add<MenacingHeart>(NPCID.MoonLordCore);
		Add<DeerclopsHeart>(NPCID.Deerclops);
		Add<MourningWoodHeart>(NPCID.MourningWood);
		Add<HorsemanHeart>(NPCID.Pumpking);
		Add<RazorpineHeart>(NPCID.Everscream);
		Add<ElfHeart>(NPCID.Everscream);
		Add<ElfHeart>(NPCID.SantaNK1);
		Add<BlizzardHeart>(NPCID.IceQueen);
		Add<BlizzardHeart>(NPCID.IceGolem);
		Add<FlyingDutchmanHeart>(NPCID.PirateShip);
		Add<BetsyHeart>(NPCID.DD2Betsy);
		Add<XenoHeart>(NPCID.MartianSaucer);
		Add<SoaringHeart>(NPCID.WyvernHead);
	}

	private static void RegisterCalamity()
	{
		const string mod = "CalamityMod";
		TryAddMod<AmbergrisHeart>(mod, "DesertScourge");
		TryAddMod<BloodyWormHeart>(mod, "DesertScourge");
		TryAddMod<FungalHeart>(mod, "Crabulon");
		TryAddMod<RottenHeart>(mod, "PerforatorHive");
		TryAddMod<CryogenHeart>(mod, "Cryogen");
		TryAddMod<AquaticHeart>(mod, "Anahita");
		TryAddMod<OceanHeart>(mod, "Leviathan");
		TryAddMod<AstralBossHeart>(mod, "AstrumAureus");
		TryAddMod<GravistarHeart>(mod, "AstrumDeus");
		TryAddMod<GehennaHeart>(mod, "BrimstoneElemental");
		TryAddMod<BlazingHeart>(mod, "BrimstoneElemental");
		TryAddMod<MutatedHeart>(mod, "PlaguebringerGoliath");
		TryAddMod<CrystallizedToxicHeart>(mod, "PlaguebringerGoliath");
		TryAddMod<CorpusHeart>(mod, "Ravager");
		TryAddMod<NebulousHeart>(mod, "DevourerofGods");
		TryAddMod<TwistingHeart>(mod, "DevourerofGods");
		TryAddMod<ProfanedHeart>(mod, "Providence");
		TryAddMod<DynamoStemHeart>(mod, "StormWeaverHead", "StormWeaver");
		TryAddMod<PolarizedHeart>(mod, "CeaselessVoid");
		TryAddMod<DarkPlasmicHeart>(mod, "Signus");
		TryAddMod<ArmoredHeart>(mod, "Polterghast");
		TryAddMod<CalamitousHeart>(mod, "SupremeCalamitas");
		TryAddMod<DraconicHeart>(mod, "Yharon");
		TryAddMod<AfflictedHeart>(mod, "CalamitasClone");
	}

	private static void RegisterThorium()
	{
		const string mod = "ThoriumMod";
		TryAddMod<ZephyrHeart>(mod, "GrandThunderBird");
		TryAddMod<SeaBreezeHeart>(mod, "QueenJellyfish");
		TryAddMod<VampireHeart>(mod, "Viscount");
		TryAddMod<StormHeart>(mod, "GraniteEnergyStorm");
		TryAddMod<ChampionHeart>(mod, "BuriedChampion");
		TryAddMod<OmegaHeart>(mod, "StarScouter");
		TryAddMod<IceboundStriderHeart>(mod, "BoreanStrider");
		TryAddMod<BeholderHeart>(mod, "FallenBeholder");
		TryAddMod<LichHeart>(mod, "Lich");
		TryAddMod<AbyssalHeart>(mod, "ForgottenOne");
		TryAddMod<DormantHeart>(mod, "Aquaius", "Omnicide", "SlagFury");
	}

	private static void RegisterConsolaria()
	{
		const string mod = "Consolaria";
		TryAddMod<CursedHeart>(mod, "Ocram");
		TryAddMod<SoulOfBlightHeart>(mod, "Ocram");
		TryAddMod<EasterHeart>(mod, "Lepus");
		TryAddMod<CornucopiaHeart>(mod, "Turkor");
	}

	private static void Add<THeart>(int npcType) where THeart : ModItem =>
		RegisterDrop(npcType, ModContent.ItemType<THeart>());

	private static void TryAddMod<THeart>(string modName, params string[] npcNames) where THeart : ModItem
	{
		if (!ModLoader.TryGetMod(modName, out Mod mod))
			return;

		int itemType = ModContent.ItemType<THeart>();
		foreach (string npcName in npcNames)
		{
			if (mod.TryFind<ModNPC>(npcName, out ModNPC npc))
				RegisterDrop(npc.Type, itemType);
		}
	}

	private static void RegisterDrop(int npcType, int itemType)
	{
		if (!DropsByNpc.TryGetValue(npcType, out List<int> drops))
			DropsByNpc[npcType] = drops = [];

		if (!drops.Contains(itemType))
			drops.Add(itemType);
	}
}
