// Architecture scaffold only. Fill behavior in the implementation pass.
namespace ElementalHearts.Core;

/// <summary>Stable typed identity for bosses whose first-kill and heart-drop state is tracked by the world state.</summary>
public enum BossId : ushort
{
	None = 0,
	KingSlime, EyeOfCthulhu, BrainOfCthulhu, QueenBee, Skeletron, WallOfFlesh, QueenSlime, Destroyer, Plantera, Golem, DukeFishron, EmpressOfLight, LunaticCultist, MoonLord, Deerclops, MourningWood, Pumpking, Everscream, SantaNK1, FlyingDutchman, Betsy, MartianSaucer,
	CommonAnimate, UncommonAnimate, RareAnimate, EpicAnimate, LegendaryAnimate,
	Length
}
