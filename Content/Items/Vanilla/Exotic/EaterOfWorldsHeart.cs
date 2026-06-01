using ElementalHearts.Content.Items.Hearts;
using Terraria;
using Terraria.DataStructures;

namespace ElementalHearts.Content.Items.Vanilla.Exotic;

public sealed class EaterOfWorldsHeart : BossHeartItem
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();
		Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(20, 9));
	}
}

