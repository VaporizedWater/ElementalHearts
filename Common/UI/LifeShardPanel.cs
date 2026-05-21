using System;
using System.Collections.Generic;
using ElementalHearts.Common.Configs;
using ElementalHearts.Common.LifeShards;
using ElementalHearts.Common.Players;
using ElementalHearts.Content.Items.LifeShards;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace ElementalHearts.Common.UI;

/// <summary>
/// Draws the Life Shard panel as a column matching vanilla's Coins/Ammo columns, sitting
/// directly right of Ammo. A "Shards" text label heads the column; clicking it toggles the
/// slot column open or closed. Each slot only accepts its own tier of shard, and the column
/// compacts as slots are unlocked or emptied.
/// </summary>
public sealed class LifeShardPanel : ModSystem
{
	// InventoryItem gives the plain blue inventory slot background; the storage contexts
	// (ChestItem, BankItem) tint the slot maroon or pink.
	private const int SlotContext = ItemSlot.Context.InventoryItem;

	/// <summary>
	/// The vanilla main inventory grid's scale, used for column/row layout. It can't be
	/// read from <see cref="Main.inventoryScale"/> here — other UI panels overwrite that
	/// before this layer runs — so the grid's own constant is used instead.
	/// </summary>
	private const float MainInventoryScale = 0.755f;

	/// <summary>Scale of the Coins/Ammo slots — smaller than a main inventory slot.</summary>
	private const float SlotScale = 0.66f;

	// Column position in main-grid pitches from the grid's top-left origin. The 10-wide
	// grid is columns 0-9; vanilla's Coins/Ammo columns follow, so the Shards column sits
	// one pitch right of Ammo. Tune these two indices if the column doesn't line up.
	private const float OriginX = 20f;
	private const float OriginY = 20f;
	private const float ColumnIndex = 13f;
	private const float FirstSlotRow = 2f;

	/// <summary>Whether the slot column is expanded. Session state, deliberately not saved.</summary>
	private bool _expanded = true;

	public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
	{
		int inventoryLayer = layers.FindIndex(layer => layer.Name == "Vanilla: Inventory");
		if (inventoryLayer < 0)
			return;

		layers.Insert(inventoryLayer + 1, new LegacyGameInterfaceLayer(
			"ElementalHearts: Life Shard Panel",
			() =>
			{
				Draw(Main.spriteBatch);
				return true;
			},
			InterfaceScaleType.UI));
	}

	private void Draw(SpriteBatch spriteBatch)
	{
		if (!Main.playerInventory || Main.LocalPlayer == null)
			return;
		if (!LifeShardConfig.Instance.SystemEnabled)
			return;

		LifeShardPlayer shardPlayer = Main.LocalPlayer.GetModPlayer<LifeShardPlayer>();

		// Tiers worth drawing: any unlocked (non-empty) slot, plus the tier of a shard
		// held on the cursor — so a removed shard can always be put back, re-unlocking it.
		List<int> visible = GetVisibleTiers(shardPlayer);
		if (visible.Count == 0)
			return;

		// ItemSlot.Draw sizes slots from Main.inventoryScale, so force it to the Coins/Ammo
		// slot scale for this layer (it's reset by vanilla every frame anyway).
		float savedScale = Main.inventoryScale;
		Main.inventoryScale = SlotScale;

		float gridPitch = 56f * MainInventoryScale;
		float slotPitch = 56f * SlotScale;
		float slotSize = TextureAssets.InventoryBack.Value.Width * SlotScale;
		float columnX = OriginX + (ColumnIndex * gridPitch);
		float firstSlotY = OriginY + (FirstSlotRow * gridPitch);
		Vector2 mouse = UiMouse();

		DrawToggleLabel(spriteBatch, columnX, firstSlotY, mouse);

		if (_expanded)
		{
			float slotY = firstSlotY;
			foreach (int tier in visible)
			{
				Vector2 slotPos = new Vector2(columnX, slotY);
				DrawSlot(spriteBatch, shardPlayer, tier, slotPos, mouse);
				DrawCombineButton(spriteBatch, shardPlayer, tier, slotPos, slotSize, mouse);
				slotY += slotPitch;
			}
		}

		Main.inventoryScale = savedScale;
	}

	private static List<int> GetVisibleTiers(LifeShardPlayer shardPlayer)
	{
		int cursorTier = Main.mouseItem.ModItem is LifeShardItem held ? (int)held.Tier : -1;

		var visible = new List<int>();
		for (int tier = 0; tier < LifeShardPlayer.SlotCount; tier++)
		{
			if (!shardPlayer.Shards[tier].IsAir || tier == cursorTier)
				visible.Add(tier);
		}

		return visible;
	}

	private void DrawToggleLabel(SpriteBatch spriteBatch, float columnX, float firstSlotY, Vector2 mouse)
	{
		string text = Language.GetTextValue("Mods.ElementalHearts.UI.LifeShardPanel");
		Vector2 textSize = FontAssets.MouseText.Value.MeasureString(text) * MainInventoryScale;

		// Left-aligned to the column and hugging the first slot, like vanilla's labels.
		Vector2 pos = new Vector2(columnX, firstSlotY - textSize.Y - 2f);

		// Plain white, no border — consistent with the vanilla "Coins"/"Ammo" labels.
		spriteBatch.DrawString(FontAssets.MouseText.Value, text, pos, Color.White,
			0f, Vector2.Zero, MainInventoryScale, SpriteEffects.None, 0f);

		var rect = new Rectangle((int)pos.X, (int)pos.Y, (int)textSize.X, (int)textSize.Y);
		if (!rect.Contains((int)mouse.X, (int)mouse.Y) || PlayerInput.IgnoreMouseInterface)
			return;

		Main.LocalPlayer.mouseInterface = true;
		if (Main.mouseLeft && Main.mouseLeftRelease)
		{
			_expanded = !_expanded;
			Main.mouseLeftRelease = false;
			SoundEngine.PlaySound(SoundID.MenuTick);
		}
	}

	private static void DrawSlot(SpriteBatch spriteBatch, LifeShardPlayer shardPlayer, int tier,
		Vector2 position, Vector2 mouse)
	{
		float slotSize = TextureAssets.InventoryBack.Value.Width * SlotScale;
		var rect = new Rectangle((int)position.X, (int)position.Y, (int)slotSize, (int)slotSize);

		if (rect.Contains((int)mouse.X, (int)mouse.Y) && !PlayerInput.IgnoreMouseInterface)
		{
			Main.LocalPlayer.mouseInterface = true;

			// The slot only accepts its own tier of shard; an empty cursor (taking shards
			// out) is always allowed.
			bool cursorAllowed = Main.mouseItem.IsAir
				|| (Main.mouseItem.ModItem is LifeShardItem held && (int)held.Tier == tier);
			if (cursorAllowed)
				ItemSlot.Handle(ref shardPlayer.Shards[tier], SlotContext);
		}

		ItemSlot.Draw(spriteBatch, ref shardPlayer.Shards[tier], SlotContext, position);
	}

	/// <summary>
	/// Draws a small "combine" button right of a slot when it holds enough shards to merge
	/// up a tier. The button shows the resulting tier's shard icon; clicking it combines.
	/// </summary>
	private static void DrawCombineButton(SpriteBatch spriteBatch, LifeShardPlayer shardPlayer,
		int tier, Vector2 slotPos, float slotSize, Vector2 mouse)
	{
		if (!shardPlayer.CanCombine(tier))
			return;

		var resultTier = (LifeShardTier)(tier + 1);
		float size = slotSize * 0.62f;
		Vector2 pos = new Vector2(slotPos.X + slotSize + 4f, slotPos.Y + ((slotSize - size) / 2f));

		Texture2D back = TextureAssets.InventoryBack.Value;
		spriteBatch.Draw(back, pos, null, Color.White, 0f, Vector2.Zero,
			size / back.Width, SpriteEffects.None, 0f);

		Texture2D icon = TextureAssets.Item[resultTier.GetItemType()].Value;
		float iconScale = (size * 0.7f) / Math.Max(icon.Width, icon.Height);
		spriteBatch.Draw(icon, pos + new Vector2(size / 2f), null, Color.White, 0f,
			new Vector2(icon.Width, icon.Height) / 2f, iconScale, SpriteEffects.None, 0f);

		var rect = new Rectangle((int)pos.X, (int)pos.Y, (int)size, (int)size);
		if (!rect.Contains((int)mouse.X, (int)mouse.Y) || PlayerInput.IgnoreMouseInterface)
			return;

		Main.LocalPlayer.mouseInterface = true;
		Main.instance.MouseText(Language.GetTextValue(
			"Mods.ElementalHearts.UI.Combine", resultTier.GetUpgradeCost()));

		if (Main.mouseLeft && Main.mouseLeftRelease)
		{
			shardPlayer.TryCombine(tier);
			Main.mouseLeftRelease = false;
			SoundEngine.PlaySound(SoundID.Grab);
		}
	}

	/// <summary>Mouse position transformed into the UI-scaled space the panel draws in.</summary>
	private static Vector2 UiMouse()
		=> Vector2.Transform(new Vector2(Main.mouseX, Main.mouseY), Matrix.Invert(Main.UIScaleMatrix));
}
