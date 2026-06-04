using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Buffs;

/// <summary>
/// Cooldown debuff shown after the Deerclops Heart's parry is used. Purely cosmetic — the real
/// countdown lives in <see cref="Common.Players.ParryAbilityPlayer"/>, which keeps this icon's
/// timer in lockstep so the player can read exactly when they can parry again (the way Chaos State
/// gates the Rod of Discord). Borrows Frostburn's icon so it reads as a wintry, beast-flavoured lock.
/// </summary>
public sealed class ParryCooldown : ModBuff
{
	// Lean on a vanilla icon (same trick as DestroyerTargetDebuff) so no bespoke sprite is needed.
	public override string Texture => "Terraria/Images/Buff_" + BuffID.Frostburn;

	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = true;
		Main.buffNoSave[Type] = true;  // the cooldown is rederived every frame, never persisted
	}
}
