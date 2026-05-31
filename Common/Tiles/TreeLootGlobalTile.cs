using ElementalHearts.Common.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Common.Tiles;

public sealed class TreeLootGlobalTile : GlobalTile
{
	public override void Drop(int i, int j, int type)
	{
		// Identify if this tile is a tree block.
		bool isTree = type == TileID.Trees || type == TileID.PalmTree || type == TileID.MushroomTrees;

		if (!isTree)
			return;

		// Since GlobalTile.Drop runs where the tile broke, we try to find the player who likely broke it.
		// Usually this runs on the client that broke it, but we can just find the closest player.
		int playerIndex = Player.FindClosest(new Vector2(i * 16, j * 16), 16, 16);
		if (playerIndex < 0 || playerIndex >= Main.maxPlayers)
			return;

		Player player = Main.player[playerIndex];
		if (!player.active || player.dead)
			return;

		var heartPlayer = player.GetModPlayer<HeartConsumptionPlayer>();

		// Acorn Heart: increases tree loot by 25% (represented here as a 25% chance to drop extra Acorns per tile)
		// We drop Acorns rather than guessing the wood type because it's a safe generic tree drop.
		// Also adds a chance to drop fruit.
		if (heartPlayer.IsConsumedLocally("AcornHeart") && player.GetModPlayer<AcornHeartPlayer>().Enabled)
		{
			if (Main.rand.NextBool(4))
			{
				Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), new Vector2(i * 16, j * 16), 16, 16, ItemID.Acorn);
			}
			
			// Small chance to drop fruit
			if (Main.rand.NextBool(25))
			{
				int[] fruits = new int[] { ItemID.Apple, ItemID.Apricot, ItemID.Grapefruit, ItemID.Lemon, ItemID.Peach, ItemID.Cherry, ItemID.Plum, ItemID.Mango, ItemID.Pineapple, ItemID.Coconut, ItemID.Banana };
				int fruit = fruits[Main.rand.Next(fruits.Length)];
				Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), new Vector2(i * 16, j * 16), 16, 16, fruit);
			}
		}

		// Gemcorn Heart: allows harvesting gems from vanilla trees
		if (heartPlayer.IsConsumedLocally("GemcornHeart") && player.GetModPlayer<GemcornHeartPlayer>().Enabled)
		{
			// Moderate chance so that chopping a whole tree yields roughly 1-2 gems.
			// A tree has about 10-20 blocks, so 1 in 15 is reasonable.
			if (Main.rand.NextBool(15))
			{
				int[] gems = new int[] { ItemID.Amethyst, ItemID.Topaz, ItemID.Sapphire, ItemID.Emerald, ItemID.Ruby, ItemID.Diamond, ItemID.Amber };
				int gem = gems[Main.rand.Next(gems.Length)];
				Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), new Vector2(i * 16, j * 16), 16, 16, gem);
			}
		}
	}
}
