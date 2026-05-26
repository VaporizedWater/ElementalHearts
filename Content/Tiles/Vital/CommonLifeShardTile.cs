using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ElementalHearts.Content.Tiles.Vital
{
	public class CommonLifeShardTile : ModTile
	{
		public override string Texture => "ElementalHearts/Content/Tiles/Vital/CommonLifeShardTile";

		public override void SetStaticDefaults()
		{
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileWaterDeath[Type] = false;
			Main.tileLavaDeath[Type] = false;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.RandomStyleRange = 3;
			TileObjectData.newTile.LavaDeath = false;
			
			// Allow growing on any side
			TileObjectData.newTile.AnchorBottom = new Terraria.DataStructures.AnchorData(Terraria.Enums.AnchorType.SolidTile, 2, 0);
			
			for (int style = 0; style < 3; style++)
			{
				TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
				TileObjectData.newAlternate.AnchorBottom = Terraria.DataStructures.AnchorData.Empty;
				TileObjectData.newAlternate.AnchorTop = new Terraria.DataStructures.AnchorData(Terraria.Enums.AnchorType.SolidTile, 2, 0);
				TileObjectData.addAlternate(style);
				
				TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
				TileObjectData.newAlternate.AnchorBottom = Terraria.DataStructures.AnchorData.Empty;
				TileObjectData.newAlternate.AnchorLeft = new Terraria.DataStructures.AnchorData(Terraria.Enums.AnchorType.SolidTile, 2, 0);
				TileObjectData.addAlternate(style);
				
				TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
				TileObjectData.newAlternate.AnchorBottom = Terraria.DataStructures.AnchorData.Empty;
				TileObjectData.newAlternate.AnchorRight = new Terraria.DataStructures.AnchorData(Terraria.Enums.AnchorType.SolidTile, 2, 0);
				TileObjectData.addAlternate(style);
			}

			TileObjectData.addTile(Type);

			HitSound = SoundID.Shatter;
			DustType = DustID.PinkCrystalShard;

			AddMapEntry(new Color(255, 150, 200), CreateMapEntryName());
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = 0.5f;
			g = 0.1f;
			b = 0.3f;
		}

		public override void NumDust(int i, int j, bool fail, ref int num)
		{
			num = fail ? 0 : 1; // Make particle effects extremely subtle
		}

		// Vanilla's multitile detection uses TileObjectData frame ranges. Style2x2 only
		// covers frameY 0–18, but worldgen places ceiling/wall variants at frameY 36/72/108,
		// so the game treats each block as 1x1 — KillMultiTile never fires. We cascade the
		// 2x2 manually here and roll the drop ourselves.
		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			if (fail || effectOnly) return;

			Tile t = Main.tile[i, j];
			int partX = (t.TileFrameX % 36) / 18;
			int partY = (t.TileFrameY % 36) / 18;
			int left = i - partX;
			int top = j - partY;

			for (int dx = 0; dx < 2; dx++)
			{
				for (int dy = 0; dy < 2; dy++)
				{
					int x = left + dx;
					int y = top + dy;
					if (x == i && y == j) continue;
					if (x < 0 || y < 0 || x >= Main.maxTilesX || y >= Main.maxTilesY) continue;
					Tile other = Main.tile[x, y];
					if (other.HasTile && other.TileType == Type)
					{
						other.HasTile = false;
						other.TileFrameX = 0;
						other.TileFrameY = 0;
						if (Main.netMode == Terraria.ID.NetmodeID.Server)
							NetMessage.SendTileSquare(-1, x, y);
					}
				}
			}

			if (Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient) return;

			int itemToDrop = ModContent.ItemType<global::ElementalHearts.Content.Items.LifeShards.CommonLifeShard>();
			if (itemToDrop == 0) itemToDrop = Terraria.ID.ItemID.DirtBlock; // FALLBACK

			float rand = Main.rand.NextFloat();

			if (rand < 0.0001f) // 0.01%
			{
				int legendary = ModContent.ItemType<global::ElementalHearts.Content.Items.LifeShards.LegendaryLifeShard>();
				if (legendary != 0) itemToDrop = legendary;
			}
			else if (rand < 0.0011f) // 0.1% + 0.01%
			{
				int epic = ModContent.ItemType<global::ElementalHearts.Content.Items.LifeShards.EpicLifeShard>();
				if (epic != 0) itemToDrop = epic;
			}
			else if (rand < 0.0111f) // 1% + 0.11%
			{
				int rare = ModContent.ItemType<global::ElementalHearts.Content.Items.LifeShards.RareLifeShard>();
				if (rare != 0) itemToDrop = rare;
			}
			else if (rand < 0.1111f) // 10% + 1.11%
			{
				int uncommon = ModContent.ItemType<global::ElementalHearts.Content.Items.LifeShards.UncommonLifeShard>();
				if (uncommon != 0) itemToDrop = uncommon;
			}

			Item.NewItem(new Terraria.DataStructures.EntitySource_TileBreak(left, top), left * 16, top * 16, 32, 32, itemToDrop, 1);
		}

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
		{
			Tile t = Main.tile[i, j];

			// For a 2x2 tile, horizontal styles are separated by 36 pixels (18*2).
			// Find the top-left block of the 2x2 structure
			int partX = (t.TileFrameX % 36) / 18;
			int partY = (t.TileFrameY % 36) / 18;
			int left = i - partX;
			int top = j - partY;

			// Decode orientation from the current frameY. The crystal is bound to the
			// specific anchor it was placed on — losing that block must destroy the
			// crystal, even if another side happens to be solid.
			int baseFrameY = t.TileFrameY - (partY * 18);

			bool anchored = baseFrameY switch
			{
				0   => WorldGen.SolidTile(left, top + 2) && WorldGen.SolidTile(left + 1, top + 2),   // Floor — anchored below
				36  => WorldGen.SolidTile(left, top - 1) && WorldGen.SolidTile(left + 1, top - 1),   // Ceiling — anchored above
				72  => WorldGen.SolidTile(left + 2, top) && WorldGen.SolidTile(left + 2, top + 1),   // Right wall — anchored right
				108 => WorldGen.SolidTile(left - 1, top) && WorldGen.SolidTile(left - 1, top + 1),   // Left wall — anchored left
				_   => false,
			};

			if (!anchored && !noBreak)
			{
				// Routes through our KillTile override, which cascades the 2x2 and drops a shard.
				WorldGen.KillTile(i, j);
			}

			return false; // Frame is set at placement; never re-orient on the fly.
		}
	}
}
