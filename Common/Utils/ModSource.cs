// Architecture scaffold only. Fill behavior in the implementation pass.
namespace ElementalHearts.Core;

/// <summary>Typed source-mod identity for load gates and filters. String mod names are produced by extension methods only.</summary>
public enum ModSource : byte
{
	Vanilla = 0,
	Calamity,
	Thorium,
	Consolaria,
}
