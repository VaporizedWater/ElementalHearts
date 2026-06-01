using System.IO;
using ElementalHearts.Common.Hearts;
using ElementalHearts.Common.Network;
using ElementalHearts.Content.Items.Hearts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Common.Systems;

/// <summary>
/// The "a heart dropped!" moment: a rising column of tier-coloured sparks, an outward burst,
/// a bright flash of light and a soft chime at the drop point, so a boss heart doesn't get
/// lost in the post-fight confetti. Driven from <see cref="NPCs.BossHeartDropGlobalNPC"/>;
/// because boss kills resolve on the server, <see cref="Spawn"/> broadcasts to every client.
/// </summary>
public static class BossHeartDropFx
{
	/// <summary>Fire the drop moment, syncing to all clients when called on a server.</summary>
	public static void Spawn(Vector2 center, int heartItemType)
	{
		if (Main.netMode == NetmodeID.Server)
		{
			ModPacket packet = ModContent.GetInstance<ElementalHearts>().GetPacket();
			packet.Write((byte)MessageType.BossHeartDropped);
			packet.Write(heartItemType);
			packet.Write(center.X);
			packet.Write(center.Y);
			packet.Send();
			return;
		}

		Play(center, heartItemType);
	}

	/// <summary>Receiver for <see cref="MessageType.BossHeartDropped"/>.</summary>
	public static void Receive(BinaryReader reader)
	{
		int heartItemType = reader.ReadInt32();
		float x = reader.ReadSingle();
		float y = reader.ReadSingle();
		Play(new Vector2(x, y), heartItemType);
	}

	private static void Play(Vector2 center, int heartItemType)
	{
		if (Main.dedServ)
			return;

		// Colour the moment to the heart's own tier so it matches its idle glow / consume burst.
		Color color = ModContent.GetModItem(heartItemType) is ElementalHeartItem heart
			? heart.Tier.GetEffectColor()
			: Color.White;

		// Rising column — the "beam" that draws the eye to the drop.
		for (int i = 0; i < 40; i++)
		{
			Vector2 pos = center + new Vector2(Main.rand.NextFloat(-16f, 16f), Main.rand.NextFloat(-8f, 8f));
			Vector2 velocity = new(Main.rand.NextFloat(-0.7f, 0.7f), -Main.rand.NextFloat(3f, 7.5f));
			Dust spark = Dust.NewDustPerfect(pos, DustID.FireworksRGB, velocity, 0, color, 1.35f);
			spark.noGravity = true;
			spark.fadeIn = 1.2f;
		}

		// Outward burst at the base.
		for (int i = 0; i < 24; i++)
		{
			Vector2 direction = (MathHelper.TwoPi * i / 24f).ToRotationVector2();
			Dust ring = Dust.NewDustPerfect(center, DustID.FireworksRGB, direction * 4.5f, 0, color, 1.2f);
			ring.noGravity = true;
		}

		Lighting.AddLight(center, color.ToVector3() * 2.4f);
		SoundEngine.PlaySound(SoundID.Item4.WithPitchOffset(Main.rand.NextFloat(0.2f, 0.5f)).WithVolumeScale(0.7f), center);

		// Add a tactile screen shake when the boss heart drops
		Vector2 punchDir = new Vector2(0f, -1f).RotatedByRandom(MathHelper.TwoPi);
		Main.instance.CameraModifiers.Add(new Terraria.Graphics.CameraModifiers.PunchCameraModifier(
			center,
			punchDir,
			5f, // magnitude
			2f, // vibrations per second (lower is smoother)
			20, // duration in frames (slightly longer to let it settle)
			1200f, // distance falloff
			"BossHeartDrop"));
	}
}
