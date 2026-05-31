using System;
using ElementalHearts.Common.Dash;
using ElementalHearts.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ElementalHearts.Common.Players;

/// <summary>
/// The Jack-O'-Lantern Heart's "permanently upgrades your dash" ability. Holds the per-character
/// on/off switch (kept separate from the consumption ledger because flipping it never touches HP or
/// world progression — same split as <see cref="CursorFocusPlayer"/>) and, every frame a dash
/// begins, lobs a mini jack-o'-lantern out the back of the dash. Defaults to on so the upgrade is
/// felt the moment the heart is consumed; turn it off from the Heart Log.
/// </summary>
public sealed class JackOLanternDashPlayer : ModPlayer
{
	public bool Enabled = true;

	public override void PostUpdate()
	{
		// Only the owning client decides to fire; the spawned projectile syncs itself to everyone
		// else (the standard player-projectile pattern), so the server never spawns it directly.
		if (Player.whoAmI != Main.myPlayer || Player.dead)
			return;

		if (!DashUpgrade.IsActive())
			return;

		// timeSinceLastDashStarted is incremented at the top of Player.Update and reset to 0 by the
		// vanilla dash code on the exact frame any dash begins (Shield of Cthulhu, Tabi/Master Ninja
		// Gear, Solar flare — all route through the same handler), so == 0 here is a clean, dash-type
		// agnostic "a dash just started" edge. Mounts ignore the dash handler, so minecart hops are
		// correctly excluded.
		if (Player.timeSinceLastDashStarted == 0)
			FireLantern();
	}

	/// <summary>Lobs the lantern opposite the dash's heading, with a small upward arc so it reads as
	/// "thrown out the back" rather than dropped straight down.</summary>
	private void FireLantern()
	{
		// The dash sets velocity.X this frame, so its sign is the dash heading; fall back to facing.
		int dashDir = Math.Sign(Player.velocity.X);
		if (dashDir == 0)
			dashDir = Player.direction;

		Vector2 velocity = new Vector2(-dashDir * 9f, -3.25f);
		velocity = velocity.RotatedByRandom(0.12f); // tiny spread so repeated dashes don't look stamped

		// Launch flourish — owning-client only (this whole method runs solely for Main.myPlayer).
		SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot.WithVolumeScale(0.4f).WithPitchOffset(0.35f), Player.Center);
		for (int i = 0; i < 8; i++)
		{
			Dust ember = Dust.NewDustPerfect(Player.Center, DustID.Torch,
				velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.15f, 0.55f), 100, default, 1.1f);
			ember.noGravity = true;
		}

		Projectile.NewProjectile(
			Player.GetSource_Misc("JackOLanternDash"),
			Player.Center, velocity,
			ModContent.ProjectileType<JackOLanternProjectile>(),
			DashUpgrade.GetProjectileDamage(), 2f, Player.whoAmI);
	}

	public override void SaveData(TagCompound tag)
	{
		// Only the off state is worth persisting; absence means the default (on).
		if (!Enabled)
			tag["jackOLanternDashOff"] = true;
	}

	public override void LoadData(TagCompound tag)
	{
		Enabled = !tag.GetBool("jackOLanternDashOff");
	}
}
