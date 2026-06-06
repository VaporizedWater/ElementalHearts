// Architecture scaffold only. Fill behavior in the implementation pass.
using System;

namespace ElementalHearts.Core;

/// <summary>Behavior categories for a heart. Flags let one heart be boss-themed, active, potion-like, and/or pacified.</summary>
[Flags]
public enum HeartKind : ushort
{
	None = 0,
	Craftable = 1 << 0,
	BossDrop = 1 << 1,
	Potion = 1 << 2,
	ActiveAbility = 1 << 3,
	Pacified = 1 << 4,
	CrossMod = 1 << 5,
}
