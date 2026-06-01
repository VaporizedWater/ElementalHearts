using System;
using System.Collections.Generic;
using ElementalHearts.Common.Configs;
using ElementalHearts.Common.LifeShards;
using ElementalHearts.Common.Players;
using ElementalHearts.Common.Systems;
using ElementalHearts.Content.Items.LifeShards;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using Terraria.ModLoader.Config;

namespace ElementalHearts.Common.UI;

/// <summary>
/// Draws the Life Shard panel as a column matching vanilla's Coins/Ammo columns, sitting
/// directly right of Ammo. A text label heads the column; clicking it toggles the slot
/// column open or closed. Each slot only accepts its own tier of shard, and the column
/// compacts as slots are unlocked or emptied.
/// </summary>
public sealed class LifeShardPanel : ModSystem
{
	// Context passed to ItemSlot.Handle for standard pick-up / place behaviour. The slot
	// background is drawn manually (see DrawSlot), so this affects interaction, not looks.
	private const int SlotContext = ItemSlot.Context.InventoryItem;

	/// <summary>
	/// The vanilla main inventory grid's scale, used for column/row layout. It can't be
	/// read from <see cref="Main.inventoryScale"/> here — other UI panels overwrite that
	/// before this layer runs — so the grid's own constant is used instead.
	/// </summary>
	private const float MainInventoryScale = .755f;

	/// <summary>Scale of the Coins/Ammo slots — smaller than a main inventory slot.</summary>
	private const float SlotScale = 0.6f;

	/// <summary>
	/// Upgrade-button size as a fraction of a shard slot — kept small and compact so the
	/// row of buttons sits unobtrusively beside the slot, in keeping with vanilla's UI.
	/// </summary>
	private const float UpgradeButtonScale = 0.31f;

	/// <summary>Pixel gap between the slot and its buttons, and between adjacent buttons.</summary>
	private const float UpgradeButtonGap = 3f;

	// Column position in main-grid pitches from the grid's top-left origin. The 10-wide
	// grid is columns 0-9; vanilla's Coins/Ammo columns follow, so the Shards column sits
	// one pitch right of Ammo. Tune these two indices if the column doesn't line up.
	private const float OriginX = 20f;
	private const float OriginY = 20f;
	private const float ColumnIndex = 13f;
	private const float FirstSlotRow = 2f;

	/// <summary>Whether the slot column is expanded. Session state, deliberately not saved.</summary>
	private bool _expanded = true;

	private static bool _dragging;
	private static Vector2 _dragOffset;

	/// <summary>One-shot flag — the Lite chat sequence only fires on the first tip click each
	/// world load, reset by <see cref="OnWorldLoad"/> / <see cref="OnWorldUnload"/>.</summary>
	private static bool _liteChatFiredThisLoad;

	/// <summary>Pending Lite chat lines, paired with the <see cref="Main.GameUpdateCount"/> tick
	/// they should print on. Drained by <see cref="PostUpdateEverything"/> so the staggered
	/// delivery keeps working even when the inventory (and this panel) is closed.</summary>
	private static readonly Queue<(uint TriggerTick, string Text)> _liteChatQueue = new();

	public static bool IsHoveringShardSlot { get; internal set; }

	public override void OnWorldLoad()
	{
		_liteChatFiredThisLoad = false;
		_liteChatQueue.Clear();
	}

	public override void OnWorldUnload()
	{
		_liteChatFiredThisLoad = false;
		_liteChatQueue.Clear();
	}

	public override void PostUpdateEverything()
	{
		while (_liteChatQueue.Count > 0 && _liteChatQueue.Peek().TriggerTick <= Main.GameUpdateCount)
			Main.NewText(_liteChatQueue.Dequeue().Text);
	}

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
		IsHoveringShardSlot = false;

		if (!Main.playerInventory || Main.LocalPlayer == null)
			return;
		if (!ElementalHeartsServerConfig.Instance.LifeShards.SystemEnabled)
			return;

		LifeShardPlayer shardPlayer = Main.LocalPlayer.GetModPlayer<LifeShardPlayer>();

		// Tiers worth drawing: any unlocked (non-empty) slot, plus the tier of a shard
		// held on the cursor — so a removed shard can always be put back, re-unlocking it.
		List<int> visible = GetVisibleTiers(shardPlayer);
		if (visible.Count == 0)
			return;

		// Pin Main.inventoryScale to this column's slot scale for the layer, then restore it,
		// so scale-dependent vanilla slot code stays consistent with the smaller slots here.
		float savedScale = Main.inventoryScale;
		Main.inventoryScale = SlotScale;

		float gridPitch = 56f * MainInventoryScale;
		float slotPitch = 56f * SlotScale;
		float slotSize = TextureAssets.InventoryBack.Value.Width * SlotScale;
		float columnX = OriginX + (ColumnIndex * gridPitch);
		float firstSlotY = OriginY + (FirstSlotRow * gridPitch);
		Vector2 mouse = UiMouse();

		if (ElementalHeartsClientConfig.Instance.UI.DraggableUI && ElementalHeartsClientConfig.Instance.UI.UIPosition != Vector2.Zero)
		{
			columnX = ElementalHeartsClientConfig.Instance.UI.UIPosition.X;
			firstSlotY = ElementalHeartsClientConfig.Instance.UI.UIPosition.Y;

			// Enforce safe zone on load to prevent spawning in the top left
			if (columnX < 300f && firstSlotY < 300f)
			{
				if (300f - columnX < 300f - firstSlotY)
					columnX = 300f;
				else
					firstSlotY = 300f;
			}
			columnX = Math.Clamp(columnX, 0f, Main.screenWidth - slotSize);
			firstSlotY = Math.Clamp(firstSlotY, 0f, Main.screenHeight - slotSize);
		}

		if (_dragging)
		{
			if (Main.mouseLeft)
			{
				columnX = mouse.X - _dragOffset.X;
				firstSlotY = mouse.Y - _dragOffset.Y;

				if (columnX < 300f && firstSlotY < 300f)
				{
					if (300f - columnX < 300f - firstSlotY)
						columnX = 300f;
					else
						firstSlotY = 300f;
				}
				columnX = Math.Clamp(columnX, 0f, Main.screenWidth - slotSize);
				firstSlotY = Math.Clamp(firstSlotY, 0f, Main.screenHeight - slotSize);

				ElementalHeartsClientConfig.Instance.UI.UIPosition = new Vector2(columnX, firstSlotY);
			}
			else
			{
				_dragging = false;
				var saveMethod = typeof(Terraria.ModLoader.Config.ConfigManager).GetMethod("Save", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
				if (saveMethod != null)
				{
					saveMethod.Invoke(null, new object[] { ElementalHeartsClientConfig.Instance.Visuals });
				}
			}
		}

		DrawToggleLabel(spriteBatch, columnX, firstSlotY, slotSize, mouse);

		if (_expanded)
		{
			float slotY = firstSlotY;
			foreach (int tier in visible)
			{
				Vector2 slotPos = new Vector2(columnX, slotY);
				DrawSlot(spriteBatch, shardPlayer, tier, slotPos, mouse);
				DrawUpgradeButtons(spriteBatch, shardPlayer, tier, slotPos, slotSize, mouse);
				slotY += slotPitch;
			}

			// Tip badge sits in place of the upgrade button on the highest tier whose upgrade is
			// still gated by an undefeated Animate. Single badge, only when affordable.
			DrawAnimateTipBadge(spriteBatch, shardPlayer, columnX, firstSlotY, slotSize,
				slotPitch, visible, mouse);
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

	private void DrawToggleLabel(SpriteBatch spriteBatch, float columnX, float firstSlotY,
		float slotSize, Vector2 mouse)
	{
		string text = Language.GetTextValue("Mods.ElementalHearts.UI.LifeShardPanel");
		Vector2 textSize = FontAssets.MouseText.Value.MeasureString(text) * .6f;

		// Centred horizontally over the slot column, hugging the first slot. The drawn width
		// uses the real draw scale so the label lines up with the slot below it.
		float drawnWidth = FontAssets.MouseText.Value.MeasureString(text).X * MainInventoryScale;
		Vector2 pos = new Vector2(columnX + ((slotSize - drawnWidth) / 2f), firstSlotY - textSize.Y - 3);

		// Pulsing color, no border — consistent with the vanilla "Coins"/"Ammo" labels.
		Color pulsingColor = new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor);
		spriteBatch.DrawString(FontAssets.MouseText.Value, text, pos, pulsingColor,
			0f, Vector2.Zero, MainInventoryScale, SpriteEffects.None, 0f);

		var rect = new Rectangle((int)pos.X, (int)pos.Y, (int)drawnWidth, (int)textSize.Y);
		if (!rect.Contains((int)mouse.X, (int)mouse.Y) || PlayerInput.IgnoreMouseInterface)
			return;

		Main.LocalPlayer.mouseInterface = true;
		
		if (ElementalHeartsClientConfig.Instance.UI.DraggableUI)
		{
			if (Main.mouseLeft && Main.mouseLeftRelease && !_dragging)
			{
				_dragging = true;
				_dragOffset = new Vector2(mouse.X - columnX, mouse.Y - firstSlotY);
			}
			if (Main.mouseRight && Main.mouseRightRelease)
			{
				_expanded = !_expanded;
				Main.mouseRightRelease = false;
				SoundEngine.PlaySound(SoundID.MenuTick);
			}
		}
		else
		{
			if (Main.mouseLeft && Main.mouseLeftRelease)
			{
				_expanded = !_expanded;
				Main.mouseLeftRelease = false;
				SoundEngine.PlaySound(SoundID.MenuTick);
			}
		}
	}

	private static void DrawSlot(SpriteBatch spriteBatch, LifeShardPlayer shardPlayer, int tier,
		Vector2 position, Vector2 mouse)
	{
		float slotSize = TextureAssets.InventoryBack.Value.Width * SlotScale;
		var rect = new Rectangle((int)position.X, (int)position.Y, (int)slotSize, (int)slotSize);

		if (rect.Contains((int)mouse.X, (int)mouse.Y) && !PlayerInput.IgnoreMouseInterface)
		{
			IsHoveringShardSlot = true;
			Main.LocalPlayer.mouseInterface = true;

			// The slot only accepts its own tier of shard; an empty cursor (taking shards
			// out) is always allowed.
			bool cursorAllowed = Main.mouseItem.IsAir
				|| (Main.mouseItem.ModItem is LifeShardItem held && (int)held.Tier == tier);
			if (cursorAllowed)
				ItemSlot.Handle(ref shardPlayer.Shards[tier], SlotContext);
		}

		// Drawn manually rather than via ItemSlot.Draw: that helper treats a ref-item slot
		// as hotbar slot 0 and stamps a "1" hotbar number onto it. This isn't a hotbar slot.
		Texture2D back = TextureAssets.InventoryBack.Value;
		spriteBatch.Draw(back, position, null, Main.inventoryBack, 0f, Vector2.Zero,
			SlotScale, SpriteEffects.None, 0f);

		Item item = shardPlayer.Shards[tier];
		if (item.IsAir)
			return;

		// Item icon, centred in the slot. Drawn at the slot's own scale — only shrunk
		// further if the sprite is oversized — exactly how vanilla renders items in their
		// slots, so small items stay small instead of being blown up to fill the slot.
		Main.instance.LoadItem(item.type);
		Texture2D icon = TextureAssets.Item[item.type].Value;
		float iconScale = SlotScale;
		float maxDim = Math.Max(icon.Width, icon.Height);
		if (maxDim > 32f)
			iconScale *= 32f / maxDim;
		spriteBatch.Draw(icon, position + new Vector2(slotSize / 2f), null, Color.White, 0f,
			new Vector2(icon.Width, icon.Height) / 2f, iconScale, SpriteEffects.None, 0f);

		// Stack count, bottom-left like a vanilla slot.
		if (item.stack > 1)
		{
			Vector2 stackPos = position + new Vector2(slotSize * 0.16f, slotSize * 0.56f);
			ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value,
				item.stack.ToString(), stackPos, Color.White, 0f, Vector2.Zero,
				new Vector2(SlotScale), -1f, SlotScale);
		}
	}

	/// <summary>
	/// Draws the upgrade buttons in a row right of a slot — one per higher tier the slot can
	/// currently afford to craft directly. Each button is that target tier's upgrade-label
	/// sprite; clicking it consumes the shards and produces one shard of that tier, skipping
	/// any tiers in between. A higher-tier button only appears once you can afford it, so a
	/// big enough stockpile lets you jump straight past the tiers you don't want.
	/// </summary>
	private static void DrawUpgradeButtons(SpriteBatch spriteBatch, LifeShardPlayer shardPlayer,
		int tier, Vector2 slotPos, float slotSize, Vector2 mouse)
	{
		// Small, compact buttons in a row, vertically centred on the slot.
		float buttonSize = slotSize * UpgradeButtonScale;
		float buttonY = slotPos.Y + ((slotSize - buttonSize) / 2f);
		float x = slotPos.X + slotSize + UpgradeButtonGap;

		// Passive "breathing" pulse — the label gently scales ±5% on a slow sine. The
		// hit-test rect stays fixed to buttonSize, so only the visuals move.
		float pulse = 1f + (0.05f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f));

		// One button per higher tier — Uncommon through Legendary — that the slot can
		// currently afford. The loop runs to the top tier, so the Legendary button is
		// wired up exactly like the rest once you hold enough shards for it.
		for (int target = tier + 1; target < LifeShardPlayer.SlotCount; target++)
		{
			if (!shardPlayer.CanUpgrade(tier, target))
				continue;

			var targetTier = (LifeShardTier)target;
			Texture2D label = UpgradeLabel(targetTier);

			var rect = new Rectangle((int)x, (int)buttonY, (int)buttonSize, (int)buttonSize);
			bool hover = rect.Contains((int)mouse.X, (int)mouse.Y) && !PlayerInput.IgnoreMouseInterface;
			Vector2 center = new Vector2(x + (buttonSize / 2f), buttonY + (buttonSize / 2f));

			// Hovering nudges the label itself a touch larger.

			float scale = buttonSize / label.Width * pulse * (hover ? 1.15f : 1f);
			spriteBatch.Draw(label, center, null, Color.White, 0f,
				new Vector2(label.Width, label.Height) / 2f, scale, SpriteEffects.None, 0f);

			if (hover)
			{
				Main.LocalPlayer.mouseInterface = true;
				var lowerTier = (LifeShardTier)tier;
				int cost = lowerTier.GetUpgradeCost(targetTier);
				// "Life Shards" / "Life Shard" tinted with each tier's own colour, so the tier
				// reads from the colour alone instead of repeating the word ("Uncommon", etc.).
				string lowerText = Tinted("Life Shards", lowerTier.GetTextColor());
				string targetText = Tinted("Life Shard", targetTier.GetTextColor());
				Main.instance.MouseText(Language.GetTextValue(
					"Mods.ElementalHearts.UI.Fuse", cost, lowerText, targetText));

				// Left-click performs the upgrade: consume the shards, craft the result,
				// and play the smith + crystal cue.
				if (Clicked())
				{
					shardPlayer.TryUpgrade(tier, target);
					PlayUpgradeSound(targetTier);
				}
			}

			x += buttonSize + UpgradeButtonGap;
		}
	}

	/// <summary>
	/// The upgrade-label sprite for a craftable target tier (Uncommon…Legendary), loaded
	/// from <c>Common/UI/</c> next to this panel. tModLoader caches the asset internally, so
	/// requesting it per frame is cheap.
	/// </summary>
	private static Texture2D UpgradeLabel(LifeShardTier tier)
		=> ModContent.Request<Texture2D>($"ElementalHearts/Common/UI/{tier}UpgradeLabel",
			AssetRequestMode.ImmediateLoad).Value;

	/// <summary>True on the frame the left mouse button is freshly pressed; consumes the press.</summary>
	private static bool Clicked()
	{
		if (Main.mouseLeft && Main.mouseLeftRelease)
		{
			Main.mouseLeftRelease = false;
			return true;
		}

		return false;
	}

	/// <summary>
	/// Plays the feedback for a successful combine: the vanilla reforge "smith" tink layered
	/// with the resulting tier's own crystal cue.
	/// </summary>
	private static void PlayUpgradeSound(LifeShardTier resultTier)
	{
		SoundEngine.PlaySound(SoundID.Item37);
		SoundEngine.PlaySound(resultTier.GetPickupSound());
	}

	/// <summary>
	/// Draws the Animate progression hint — a pulsing badge sitting in place of the upgrade
	/// button on the highest shard slot whose upgrade is still gated by an undefeated Animate.
	/// Only renders when the Tips config is on, there is a higher tier to unlock, the gated
	/// slot has shards equal to or above the fuse cost, and that slot is visible. Hovering
	/// shows the short tip; clicking broadcasts an expanded version to chat.
	/// </summary>
	private static void DrawAnimateTipBadge(SpriteBatch spriteBatch, LifeShardPlayer shardPlayer,
		float columnX, float firstSlotY, float slotSize, float slotPitch, List<int> visible,
		Vector2 mouse)
	{
		if (!ElementalHeartsClientConfig.Instance.Tips.EnableTips)
			return;

		// Slot whose upgrade is currently locked by Animate progression — the lowest target
		// CanUpgrade would still reject is UnlockedTier + 1, so the gated source slot is
		// UnlockedTier itself. Bail when there's no higher tier left to gate.
		int gatedTier = AnimateProgressionSystem.UnlockedTier;
		int nextTier = gatedTier + 1;
		if (nextTier >= LifeShardPlayer.SlotCount)
			return;

		int rowIndex = visible.IndexOf(gatedTier);
		if (rowIndex < 0)
			return;

		// Hide the tip if the player couldn't fuse one shard up even if the gate lifted —
		// keeps the badge from nagging when there isn't enough material for an upgrade anyway.
		int cost = ((LifeShardTier)gatedTier).GetUpgradeCost((LifeShardTier)nextTier);
		if (cost <= 0 || shardPlayer.Shards[gatedTier].stack < cost)
			return;

		float slotY = firstSlotY + (rowIndex * slotPitch);
		float buttonSize = slotSize * UpgradeButtonScale;
		Vector2 center = new Vector2(
			columnX + slotSize + UpgradeButtonGap + (buttonSize / 2f),
			slotY + (slotSize / 2f));

		// Soft "breathing" pulse — matches the passive animation on the upgrade-label sprites
		// it sits in line with.
		float pulse = 1f + (0.05f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f));

		Texture2D label = ModContent.Request<Texture2D>("ElementalHearts/Assets/UI/TipLabel",
			AssetRequestMode.ImmediateLoad).Value;

		Rectangle rect = new Rectangle((int)(center.X - (buttonSize / 2f)),
			(int)(center.Y - (buttonSize / 2f)), (int)buttonSize, (int)buttonSize);
		bool hover = rect.Contains((int)mouse.X, (int)mouse.Y) && !PlayerInput.IgnoreMouseInterface;

		float scale = buttonSize / label.Width * pulse * (hover ? 1.15f : 1f);
		spriteBatch.Draw(label, center, null, Color.White, 0f,
			new Vector2(label.Width, label.Height) / 2f, scale, SpriteEffects.None, 0f);

		if (!hover)
			return;

		Main.LocalPlayer.mouseInterface = true;

		// "Animate" takes the gated tier's colour, "next tier" takes the unlock target's —
		// so the wording mirrors the slots the tip references without naming the tiers.
		Color currentColor = ((LifeShardTier)gatedTier).GetTextColor();
		Color nextColor = ((LifeShardTier)nextTier).GetTextColor();
		string animate = Tinted("Animate", currentColor);
		string nextText = Tinted("next tier", nextColor);

		// The very first tip (no Animate ever defeated) bundles the "Spawn him with a Menacing
		// Heart!" call to action; higher-tier tips are deliberately terser — by then the player
		// already knows the loop and just needs the colour cue for the next step.
		bool firstTip = gatedTier == 0;
		if (firstTip)
		{
			string menacing = Tinted("Menacing Heart", currentColor);
			Main.instance.MouseText(string.Format(
				Language.GetTextValue("Mods.ElementalHearts.UI.AnimateTip"),
				animate, nextText, menacing));
		}
		else
		{
			Main.instance.MouseText(string.Format(
				Language.GetTextValue("Mods.ElementalHearts.UI.AnimateTipNext"),
				animate, nextText));
		}

		if (Main.mouseLeft && Main.mouseLeftRelease)
		{
			Main.mouseLeftRelease = false;
			SoundEngine.PlaySound(SoundID.MenuTick);
			QueueLiteChat();
		}
	}

	/// <summary>Wraps a string in Terraria's chat colour code so it renders in the given colour
	/// inside any text widget that runs through <c>ChatManager</c> (MouseText, chat messages).</summary>
	private static string Tinted(string text, Color color)
		=> $"[c/{color.R:X2}{color.G:X2}{color.B:X2}:{text}]";

	/// <summary>
	/// Schedules the four-line "Lite" chat sequence into <see cref="_liteChatQueue"/>, one line
	/// per second. Guarded by <see cref="_liteChatFiredThisLoad"/> so the sequence only plays on
	/// the very first tip click each world load — replaying would dilute the "natural chat" feel.
	/// </summary>
	private static void QueueLiteChat()
	{
		if (_liteChatFiredThisLoad)
			return;
		_liteChatFiredThisLoad = true;

		uint now = Main.GameUpdateCount;
		for (int i = 1; i <= 4; i++)
		{
			string line = Language.GetTextValue($"Mods.ElementalHearts.UI.LiteChat{i}");
			// 60 ticks ≈ 1 second at Terraria's fixed 60 FPS. First line fires after a 1s beat
			// too, so it doesn't collide with the tick sound and reads as a fresh chat ping.
			_liteChatQueue.Enqueue(((uint)(now + (i * 60)), $"<Lite> {line}"));
		}
	}

	/// <summary>Mouse position (already scaled by tModLoader in 1.4).</summary>
	private static Vector2 UiMouse()
		=> new Vector2(Main.mouseX, Main.mouseY);
}
