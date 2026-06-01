using ElementalHearts.Common.Configs;
using ElementalHearts.Content.Items.Placeable;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ElementalHearts.Content.Tiles.Vital;

/// <summary>
/// Worldgen-only chest placed inside Vital Canopy heart formations, replacing the vanilla
/// Ivy Chest that previously held the Impossible Heart loot.
/// </summary>
public sealed class VitalChestTile : ModTile
{
	public override bool IsLoadingEnabled(Mod mod) => ElementalHeartsServerConfig.Instance.VitalTiles.SystemEnabled;

	public override string Texture => "ElementalHearts/Content/Tiles/VitalChest";

	public override void SetStaticDefaults()
	{
		Main.tileSpelunker[Type] = true;
		Main.tileContainer[Type] = true;
		Main.tileLighted[Type] = true;
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileOreFinderPriority[Type] = 500;
		TileID.Sets.HasOutlines[Type] = true;
		TileID.Sets.BasicChest[Type] = true;
		TileID.Sets.DisableSmartCursor[Type] = true;
		TileID.Sets.AvoidedByNPCs[Type] = true;
		TileID.Sets.InteractibleByNPCs[Type] = true;
		TileID.Sets.IsAContainer[Type] = true;

		DustType = DustID.PinkCrystalShard;
		AdjTiles = new int[] { TileID.Containers };

		TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
		TileObjectData.newTile.Origin = new Point16(0, 1);
		TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
		TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(Chest.FindEmptyChest, -1, 0, true);
		TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(Chest.AfterPlacement_Hook, -1, 0, false);
		TileObjectData.newTile.AnchorInvalidTiles = new int[] {
			TileID.MagicalIceBlock, TileID.Boulder, TileID.BouncyBoulder,
			TileID.LifeCrystalBoulder, TileID.RollingCactus
		};
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.LavaDeath = false;
		TileObjectData.addTile(Type);

		AddMapEntry(new Color(180, 80, 110), Language.GetText("Mods.ElementalHearts.Tiles.VitalChestTile.MapEntry"), MapChestName);
	}

	public override LocalizedText DefaultContainerName(int frameX, int frameY)
		=> Language.GetText("Mods.ElementalHearts.Tiles.VitalChestTile.MapEntry");

	public static string MapChestName(string name, int i, int j)
	{
		int left = i;
		int top = j;
		Tile tile = Main.tile[i, j];
		if (tile.TileFrameX % 36 != 0) left--;
		if (tile.TileFrameY != 0) top--;

		int chest = Chest.FindChest(left, top);
		if (chest < 0) return Language.GetTextValue("LegacyChestType.0");

		if (Main.chest[chest].name == "") return name;
		return name + ": " + Main.chest[chest].name;
	}

	public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		// Match vanilla Life Crystal (TileID.Heart) glow exactly — values lifted from
		// Terraria's TileLightScanner for tile type 12.
		r = 0.9f;
		g = 0.4f;
		b = 0.3f;
	}

	public override bool IsLockedChest(int i, int j) => false;

	public override void NumDust(int i, int j, bool fail, ref int num) => num = 1;

	public override void KillMultiTile(int i, int j, int frameX, int frameY)
	{
		// Spills any contained loot via vanilla logic, then drops the placeable chest item
		// so the player can pick the chest back up like a wooden chest.
		Chest.DestroyChest(i, j);
		Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 32, ModContent.ItemType<VitalChestItem>());
	}

	public override bool RightClick(int i, int j)
	{
		Player player = Main.LocalPlayer;
		Tile tile = Main.tile[i, j];
		Main.mouseRightRelease = false;

		int left = i;
		int top = j;
		if (tile.TileFrameX % 36 != 0) left--;
		if (tile.TileFrameY != 0) top--;

		player.CloseSign();
		player.SetTalkNPC(-1);
		Main.npcChatCornerItem = 0;
		Main.npcChatText = "";

		if (Main.editChest)
		{
			SoundEngine.PlaySound(SoundID.MenuTick);
			Main.editChest = false;
			Main.npcChatText = string.Empty;
		}

		if (player.editedChestName)
		{
			NetMessage.SendData(MessageID.SyncPlayerChest, -1, -1, NetworkText.FromLiteral(Main.chest[player.chest].name), player.chest, 1f);
			player.editedChestName = false;
		}

		bool isLocked = IsLockedChest(left, top);
		if (Main.netMode == NetmodeID.MultiplayerClient && !isLocked)
		{
			if (left == player.chestX && top == player.chestY && player.chest != -1)
			{
				player.chest = -1;
				Recipe.FindRecipes();
				SoundEngine.PlaySound(SoundID.MenuClose);
			}
			else
			{
				NetMessage.SendData(MessageID.RequestChestOpen, -1, -1, null, left, top);
				Main.stackSplit = 600;
			}
		}
		else
		{
			if (!isLocked)
			{
				int chest = Chest.FindChest(left, top);
				if (chest != -1)
				{
					Main.stackSplit = 600;
					if (chest == player.chest)
					{
						player.chest = -1;
						SoundEngine.PlaySound(SoundID.MenuClose);
					}
					else
					{
						SoundEngine.PlaySound(player.chest < 0 ? SoundID.MenuOpen : SoundID.MenuTick);
						player.OpenChest(left, top, chest);
					}
					Recipe.FindRecipes();
				}
			}
		}

		return true;
	}

	public override void MouseOver(int i, int j)
	{
		Player player = Main.LocalPlayer;
		Tile tile = Main.tile[i, j];

		int left = i;
		int top = j;
		if (tile.TileFrameX % 36 != 0) left--;
		if (tile.TileFrameY != 0) top--;

		int chest = Chest.FindChest(left, top);
		player.cursorItemIconID = -1;
		if (chest < 0)
		{
			player.cursorItemIconText = Language.GetTextValue("LegacyChestType.0");
		}
		else
		{
			string defaultName = TileLoader.DefaultContainerName(tile.TileType, tile.TileFrameX, tile.TileFrameY);
			player.cursorItemIconText = Main.chest[chest].name.Length > 0 ? Main.chest[chest].name : defaultName;
			if (player.cursorItemIconText == defaultName)
			{
				player.cursorItemIconID = ModContent.ItemType<VitalQuartzItem>();
				player.cursorItemIconText = "";
			}
		}

		player.noThrow = 2;
		player.cursorItemIconEnabled = true;
	}

	public override void MouseOverFar(int i, int j)
	{
		MouseOver(i, j);
		Player player = Main.LocalPlayer;
		if (player.cursorItemIconText == "")
		{
			player.cursorItemIconEnabled = false;
			player.cursorItemIconID = ItemID.None;
		}
	}
}
