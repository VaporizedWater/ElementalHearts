using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;
using ElementalHearts.Common.Players;
using ElementalHearts.Common.Items;

namespace ElementalHearts.Common.UI;

/// <summary>
/// Handles drawing the extra 10 inventory slots and shifting the trash slot and crafting menu
/// down to prevent UI overlap when the Chest Heart is enabled.
/// </summary>
public sealed class ChestHeartInventorySystem : ModSystem
{
	public override void Load()
	{
		IL_Main.DrawInventory += DrawInventory_IL;
		IL_Main.DrawTrashItemSlot += DrawTrashItemSlot_IL;
		IL_ChestUI.Draw += ChestUI_Draw_IL;
		IL_ChestUI.DrawName += ChestUI_DrawName_IL;
		IL_ChestUI.DrawButtons += ChestUI_DrawButtons_IL;
		IL_ChestUI.DrawSlots += ChestUI_DrawSlots_IL;
		IL_Player.ItemSpace += PlayerItemSpace_IL;
	}

	public override void Unload()
	{
		IL_Main.DrawInventory -= DrawInventory_IL;
		IL_Main.DrawTrashItemSlot -= DrawTrashItemSlot_IL;
		IL_ChestUI.Draw -= ChestUI_Draw_IL;
		IL_ChestUI.DrawName -= ChestUI_DrawName_IL;
		IL_ChestUI.DrawButtons -= ChestUI_DrawButtons_IL;
		IL_ChestUI.DrawSlots -= ChestUI_DrawSlots_IL;
		IL_Player.ItemSpace -= PlayerItemSpace_IL;
	}

	public static float GetMainInventoryScale()
	{
		return 0.85f;
	}

	private void DrawInventory_IL(ILContext il)
	{
		var c = new ILCursor(il);

		// 1. Shift availableRecipeY base (410f)
		if (c.TryGotoNext(MoveType.After, i => i.MatchLdcR4(410f)))
		{
			c.EmitDelegate<Func<float, float>>(origY =>
			{
				if (Main.LocalPlayer != null && Main.LocalPlayer.active && Main.LocalPlayer.GetModPlayer<ChestHeartPlayer>().Enabled)
				{
					return origY + 56f * GetMainInventoryScale();
				}
				return origY;
			});
		}

		// Reset cursor to start
		c.Index = 0;

		// 2. Shift V_139 base (450)
		if (c.TryGotoNext(MoveType.After, i => i.MatchLdcI4(450)))
		{
			c.EmitDelegate<Func<int, int>>(origY =>
			{
				if (Main.LocalPlayer != null && Main.LocalPlayer.active && Main.LocalPlayer.GetModPlayer<ChestHeartPlayer>().Enabled)
				{
					return origY + (int)(56f * GetMainInventoryScale());
				}
				return origY;
			});
		}

		// Reset cursor to start
		c.Index = 0;

		// 3. Shift V_143 base (340)
		if (c.TryGotoNext(MoveType.After, i => i.MatchLdcI4(340)))
		{
			c.EmitDelegate<Func<int, int>>(origY =>
			{
				if (Main.LocalPlayer != null && Main.LocalPlayer.active && Main.LocalPlayer.GetModPlayer<ChestHeartPlayer>().Enabled)
				{
					return origY + (int)(56f * GetMainInventoryScale());
				}
				return origY;
			});
		}

		// Reset cursor to start
		c.Index = 0;

		// 4. Shift "Crafting" text Y base (414)
		if (c.TryGotoNext(MoveType.After, i => i.MatchLdcI4(414)))
		{
			c.EmitDelegate<Func<int, int>>(origY =>
			{
				if (Main.LocalPlayer != null && Main.LocalPlayer.active && Main.LocalPlayer.GetModPlayer<ChestHeartPlayer>().Enabled)
				{
					return origY + (int)(56f * GetMainInventoryScale());
				}
				return origY;
			});
		}

		// Reset cursor to start
		c.Index = 0;

		// 5. Shift all reads of invBottom (chest/shop panel Y coordinate base)
		while (c.TryGotoNext(MoveType.After, i => i.OpCode == OpCodes.Ldfld && i.Operand is FieldReference fr && fr.Name == "invBottom"))
		{
			c.EmitDelegate<Func<int, int>>(origInvBottom =>
			{
				if (Main.LocalPlayer != null && Main.LocalPlayer.active && Main.LocalPlayer.GetModPlayer<ChestHeartPlayer>().Enabled)
				{
					return origInvBottom + (int)(56f * GetMainInventoryScale());
				}
				return origInvBottom;
			});
		}

		// Reset cursor to start
		c.Index = 0;

		// 6. Shift chest buttons Y base (244)
		if (c.TryGotoNext(MoveType.After, i => i.MatchLdcI4(244)))
		{
			c.EmitDelegate<Func<int, int>>(origY =>
			{
				if (Main.LocalPlayer != null && Main.LocalPlayer.active && Main.LocalPlayer.GetModPlayer<ChestHeartPlayer>().Enabled)
				{
					return origY + (int)(56f * GetMainInventoryScale());
				}
				return origY;
			});
		}

		// 7. Shift shop buttons Y base (244)
		if (c.TryGotoNext(MoveType.After, i => i.MatchLdcI4(244)))
		{
			c.EmitDelegate<Func<int, int>>(origY =>
			{
				if (Main.LocalPlayer != null && Main.LocalPlayer.active && Main.LocalPlayer.GetModPlayer<ChestHeartPlayer>().Enabled)
				{
					return origY + (int)(56f * GetMainInventoryScale());
				}
				return origY;
			});
		}

		// Reset cursor to start
		c.Index = 0;

		// 8. Draw the extra slots in line right after the main inventory loops finish
		if (c.TryGotoNext(MoveType.Before, i => i.MatchCall(typeof(BuilderToggleLoader), "ActiveBuilderToggles")))
		{
			c.Emit(OpCodes.Ldsfld, typeof(Main).GetField(nameof(Main.spriteBatch))!);
			c.EmitDelegate<Action<SpriteBatch>>(sb =>
			{
				DrawSlots(sb);
			});
		}
	}

	/// <summary>
	/// IL-patches <c>Player.ItemSpace(Item)</c> so that the returned bool is OR-combined
	/// with our extra-slot space check. When this returns <see langword="true"/>, the engine
	/// attracts the item toward the player and eventually calls <c>OnPickup</c>, where
	/// <see cref="ChestHeartPickupGlobalItem"/> absorbs it into the extra slots.
	///
	/// We patch at every <c>ret</c> instruction: if vanilla returned false we check whether
	/// our extra slots have room and return true instead.
	/// </summary>
	private static void PlayerItemSpace_IL(ILContext il)
	{
		var c = new ILCursor(il);

		// Player.ItemSpace(Item item) — `this` is arg 0, `item` is arg 1.
		// Strategy: after every ret, intercept the bool on the stack.
		// Simpler: find the single return point and emit our OR before it.
		// We'll wrap the whole body: at every point that would return false,
		// also check our extra slots.
		while (c.TryGotoNext(MoveType.Before, i => i.MatchRet()))
		{
			// Stack at this point has the bool return value on top.
			// We want:  result = result || ExtraHasSpace(this, item)
			// Emit:  ldarg.0 (this / Player), ldarg.1 (item), call helper, or
			c.Emit(OpCodes.Ldarg_0);   // Player (this)
			c.Emit(OpCodes.Ldarg_1);   // Item
			c.EmitDelegate<Func<bool, Player, Item, bool>>((vanillaResult, player, item) =>
			{
				if (vanillaResult) return true; // vanilla already has space — nothing to do
				if (player.whoAmI != Main.myPlayer) return false;

				var cp = player.GetModPlayer<ChestHeartPlayer>();
				if (!cp.Enabled) return false;

				return ChestHeartPickupGlobalItem.ExtraInventoryHasSpaceStatic(cp, item);
			});
		}
	}

	private void DrawTrashItemSlot_IL(ILContext il)
	{
		var c = new ILCursor(il);

		// Shift trash Y base (258)
		if (c.TryGotoNext(MoveType.After, i => i.MatchLdcI4(258)))
		{
			c.EmitDelegate<Func<int, int>>(origY =>
			{
				if (Main.LocalPlayer != null && Main.LocalPlayer.active && Main.LocalPlayer.GetModPlayer<ChestHeartPlayer>().Enabled)
				{
					return origY + (int)(56f * GetMainInventoryScale());
				}
				return origY;
			});
		}
	}

	private void ChestUI_Draw_IL(ILContext il) => ShiftInvBottomReads(il);
	private void ChestUI_DrawName_IL(ILContext il) => ShiftInvBottomReads(il);
	private void ChestUI_DrawButtons_IL(ILContext il) => ShiftInvBottomReads(il);
	private void ChestUI_DrawSlots_IL(ILContext il) => ShiftInvBottomReads(il);

	private void ShiftInvBottomReads(ILContext il)
	{
		var c = new ILCursor(il);
		while (c.TryGotoNext(MoveType.After, i => i.OpCode == OpCodes.Ldfld && i.Operand is FieldReference fr && fr.Name == "invBottom"))
		{
			c.EmitDelegate<Func<int, int>>(origInvBottom =>
			{
				if (Main.LocalPlayer != null && Main.LocalPlayer.active && Main.LocalPlayer.GetModPlayer<ChestHeartPlayer>().Enabled)
				{
					return origInvBottom + (int)(56f * GetMainInventoryScale());
				}
				return origInvBottom;
			});
		}
	}

	private static Effect? _cachedAuroraEffect;

	public static void DrawAuroraRect(SpriteBatch spriteBatch, Rectangle drawRect, Color c1, Color c2, Color c3, float borderRadius, float hoverIntensity = 0f)
	{
		if (_cachedAuroraEffect == null)
		{
			try
			{
				byte[] shaderBytes = ModContent.GetInstance<ElementalHearts>().GetFileBytes("Assets/Effects/AuroraGradient.fxc");
				if (shaderBytes != null && shaderBytes.Length > 0)
				{
					_cachedAuroraEffect = new Effect(Main.graphics.GraphicsDevice, shaderBytes);
				}
			}
			catch
			{
				// Ignore or fallback
			}
		}

		if (_cachedAuroraEffect != null)
		{
			c1 *= 0.6f;
			c2 *= 0.6f;
			c3 *= 0.6f;

			_cachedAuroraEffect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
			_cachedAuroraEffect.Parameters["uResolution"]?.SetValue(new Vector2(drawRect.Width, drawRect.Height));
			_cachedAuroraEffect.Parameters["uHoverGlow"]?.SetValue(hoverIntensity);
			_cachedAuroraEffect.Parameters["uBorderRadius"]?.SetValue(borderRadius);
			_cachedAuroraEffect.Parameters["uColor1"]?.SetValue(c1.ToVector4());
			_cachedAuroraEffect.Parameters["uColor2"]?.SetValue(c2.ToVector4());
			_cachedAuroraEffect.Parameters["uColor3"]?.SetValue(c3.ToVector4());

			RasterizerState rasterizer = spriteBatch.GraphicsDevice.RasterizerState;

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, rasterizer, _cachedAuroraEffect, Main.UIScaleMatrix);

			spriteBatch.Draw(Terraria.GameContent.TextureAssets.MagicPixel.Value, drawRect, Color.White * 1.0f);

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, rasterizer, null, Main.UIScaleMatrix);
		}
	}

	public static void DrawSlots(SpriteBatch spriteBatch)
	{
		if (!Main.playerInventory || Main.LocalPlayer == null || !Main.LocalPlayer.active)
			return;

		var modPlayer = Main.LocalPlayer.GetModPlayer<ChestHeartPlayer>();
		if (!modPlayer.Enabled)
			return;

		float scale = GetMainInventoryScale();

		// Chest Heart's signature 3-color palette
		Color c1 = new Color(208, 128, 160); // light vital pink
		Color c2 = new Color(160, 32, 64);   // crimson
		Color c3 = new Color(128, 16, 48);   // dark burgundy

		// Temporarily tint Main.inventoryBack to make the slots glassy and transparent
		Color savedBack = Main.inventoryBack;
		Main.inventoryBack = Main.inventoryBack * 0.35f;

		for (int i = 0; i < 10; i++)
		{
			float slotX = 20f + i * 56f * scale;
			float slotY = 20f + 5 * 56f * scale;
			Vector2 slotPos = new Vector2(slotX, slotY);
			Rectangle slotRect = new Rectangle((int)slotX, (int)slotY, (int)(52f * scale), (int)(52f * scale));

			// Check if the user is hovering over this slot to add a hover glow effect
			bool isHovered = slotRect.Contains(Main.mouseX, Main.mouseY) && !PlayerInput.IgnoreMouseInterface;

			// Draw individual slot's aurora shader background
			DrawAuroraRect(spriteBatch, slotRect, c1, c2, c3, 10f * scale, isHovered ? 1f : 0f);

			// Draw slot using indices 10 to 19 to look exactly like vanilla slots and avoid hotbar labels
			ItemSlot.Draw(spriteBatch, modPlayer.ExtraInventory, ItemSlot.Context.InventoryItem, 10 + i, slotPos);

			// Handle interaction using indices 10 to 19
			if (isHovered)
			{
				Main.LocalPlayer.mouseInterface = true;
				ItemSlot.Handle(modPlayer.ExtraInventory, ItemSlot.Context.InventoryItem, 10 + i);
			}
		}

		Main.inventoryBack = savedBack;
	}
}

