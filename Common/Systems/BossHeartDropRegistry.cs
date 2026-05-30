using System.Collections.Generic;
using ElementalHearts.Content.Items.CrossModHearts.Calamity;
using ElementalHearts.Content.Items.CrossModHearts.Consolaria;
using ElementalHearts.Content.Items.CrossModHearts.Thorium;
using ElementalHearts.Content.Items.Vanilla.Exotic;
using ElementalHearts.Content.Items.Vanilla.Exotic;
using ElementalHearts.Content.Items.Vanilla.Exotic;
using ElementalHearts.Content.Items.Vanilla.Exotic;
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

	/// <summary>
	/// Read-only enumeration of every (boss NPC type → heart item types) mapping.
	/// Used by BossChecklist integration to submit hearts as collectibles for each
	/// matching boss entry without duplicating the drop registration logic.
	/// </summary>
	public static IEnumerable<KeyValuePair<int, List<int>>> AllDrops => DropsByNpc;

	public static void Build()
	{
		DropsByNpc.Clear();
		RegisterVanilla();
		RegisterCalamity();
		RegisterThorium();
		RegisterConsolaria();
		RegisterAnimateBosses();
	}

	private static void RegisterVanilla()
	{
		Add<EyeOfCthulhuHeart>(NPCID.EyeofCthulhu);
		Add<KingSlimeHeart>(NPCID.KingSlime);
		Add<BrainOfCthulhuHeart>(NPCID.BrainofCthulhu);
		Add<QueenBeeHeart>(NPCID.QueenBee);
		Add<SkeletronHeart>(NPCID.SkeletronHead);
		Add<WallOfFleshHeart>(NPCID.WallofFlesh);
		Add<QueenSlimeHeart>(NPCID.QueenSlimeBoss);
		Add<DestroyerHeart>(NPCID.TheDestroyer);
		Add<PlanteraHeart>(NPCID.Plantera);
		Add<GolemHeart>(NPCID.Golem);
		Add<DukeFishronHeart>(NPCID.DukeFishron);
		Add<EmpressOfLightHeart>(NPCID.HallowBoss);
		Add<LunaticCultistHeart>(NPCID.CultistBoss);
		Add<MoonLordHeart>(NPCID.MoonLordCore);
		Add<DeerclopsHeart>(NPCID.Deerclops);
		Add<MourningWoodHeart>(NPCID.MourningWood);
		Add<PumpkingHeart>(NPCID.Pumpking);
		Add<RazorpineHeart>(NPCID.Everscream);
		Add<ElfHeart>(NPCID.Everscream);
		Add<ElfHeart>(NPCID.SantaNK1);
		Add<FlyingDutchmanHeart>(NPCID.PirateShip);
		Add<BetsyHeart>(NPCID.DD2Betsy);
		Add<MartianSaucerHeart>(NPCID.MartianSaucer);
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

	private static void RegisterAnimateBosses()
	{
		Add<CommonPacifiedHeart>(ModContent.NPCType<global::ElementalHearts.Content.NPCs.Bosses.Animate.CommonAnimate>());
		Add<UncommonPacifiedHeart>(ModContent.NPCType<global::ElementalHearts.Content.NPCs.Bosses.Animate.UncommonAnimate>());
		Add<RarePacifiedHeart>(ModContent.NPCType<global::ElementalHearts.Content.NPCs.Bosses.Animate.RareAnimate>());
		Add<EpicPacifiedHeart>(ModContent.NPCType<global::ElementalHearts.Content.NPCs.Bosses.Animate.EpicAnimate>());
		Add<LegendaryPacifiedHeart>(ModContent.NPCType<global::ElementalHearts.Content.NPCs.Bosses.Animate.LegendaryAnimate>());
	}

	private static void Add<THeart>(int npcType) where THeart : ModItem =>
		RegisterDrop(npcType, ModContent.ItemType<THeart>());

	private static void TryAddMod<THeart>(string modName, params string[] npcNames) where THeart : ModItem
	{
		if (!ModLoader.TryGetMod(modName, out Mod mod))
			return;

		// 0 here means the heart was disabled via ElementalHeartsCrossModConfig
		// and never registered as content — skip without registering a phantom drop.
		int itemType = ModContent.ItemType<THeart>();
		if (itemType <= 0)
			return;

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
