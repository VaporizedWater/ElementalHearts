using Microsoft.Xna.Framework;
using Terraria.ID;

namespace ElementalHearts.Content.Projectiles;

public sealed class UncommonShardProjectile : SmallBossShardProjectile
{
	public override string Texture => "ElementalHearts/Content/Projectiles/UncommonSmallBossProjectile";

	protected override Vector3 LightColor => new(0.2f, 0.8f, 0.3f);

	protected override int TrailDust => DustID.GreenTorch;

	// 25% larger for better visibility.
	protected override int HitboxSize => 20;
	protected override float DrawScale => 1.25f;
}
