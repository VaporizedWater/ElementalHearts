using ElementalHearts.Common.Hearts;

namespace ElementalHearts.Content.Items.Hearts;

/// <summary>
/// Boss-themed heart that cannot be crafted; dropped by its boss via
/// <see cref="Common.NPCs.BossHeartDropGlobalNPC"/>.
/// </summary>
public abstract class BossHeartItem : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Exotic;

	public sealed override void AddRecipes() { }

	protected override void PlayShockwave(Terraria.Player player)
	{
		base.PlayShockwave(player);

		Terraria.Audio.SoundStyle sound = Terraria.ID.SoundID.Roar; // Universal fallback roar

		switch (Name)
		{
			case "QueenSlimeHeart":
				sound = Terraria.ID.SoundID.Item111;
				break;
			case "EmpressHeart":
				sound = Terraria.ID.SoundID.Item161;
				break;
			case "DukeFishronHeart":
				sound = Terraria.ID.SoundID.Zombie20;
				break;
			case "DeerclopsHeart":
				sound = Terraria.ID.SoundID.DeerclopsScream;
				break;
		}

		Terraria.Audio.SoundEngine.PlaySound(sound, player.Center);
	}
}
