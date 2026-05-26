using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Tiles.Vital
{
	public class VitalVineFix : GlobalTile
	{
		public override bool TileFrame(int i, int j, int type, ref bool resetFrame, ref bool noBreak)
		{
			// Prevent Jungle Vines from breaking when they are anchored to Vital Quartz
			if (type == TileID.JungleVines)
			{
				Tile tileAbove = Main.tile[i, j - 1];
				if (tileAbove.HasTile && tileAbove.TileType == ModContent.TileType<VitalQuartzTile>() && !tileAbove.BottomSlope && !tileAbove.IsHalfBlock)
				{
					noBreak = true;
				}
			}
			
			return base.TileFrame(i, j, type, ref resetFrame, ref noBreak);
		}
	}
}
