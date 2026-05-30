using System;
using ElementalHearts.Common.Hearts;
using ElementalHearts.Common.Systems;
using ElementalHearts.Content.Items.Hearts;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using Terraria.ModLoader;
using ElementalHearts.Common.Configs;
using ElementalHearts.Common.Players;

namespace ElementalHearts.Common.CrossMod.Munchies;

/// <summary>
/// Registers every loaded <see cref="ElementalHeartItem"/> with the Munchies mod's
/// checklist (see <c>Munchies.md</c>). Hearts are permanent one-shot consumables that
/// flip a bit in <see cref="HeartConsumptionWorld"/>, which maps cleanly onto Munchies'
/// "AddSingleConsumable" call shape.
///
/// The integration is intentionally data-driven: per-heart presentation is read from
/// virtual hooks on <see cref="ElementalHeartItem"/> (<see cref="ElementalHeartItem.MunchiesDifficulty"/>,
/// <see cref="ElementalHeartItem.MunchiesTextColor"/>, <see cref="ElementalHeartItem.MunchiesAvailability"/>,
/// <see cref="ElementalHeartItem.MunchiesAcquisitionText"/>, <see cref="ElementalHeartItem.MunchiesExtraTooltip"/>),
/// so new hearts get listed automatically with no edits to this file.
/// </summary>
internal static class MunchiesIntegration
{
	/// <summary>
	/// Pinned Munchies call-API version. Per the Munchies README this MUST be a string
	/// literal — do not derive it from the loaded mod's version, or backwards compat
	/// breaks the moment Munchies updates.
	/// </summary>
	private const string CallApiVersion = "1.3";

	private const string MunchiesModName = "Munchies";
	private const string CategoryPlayer = "player";
	private const string AddSingleConsumable = "AddSingleConsumable";

	private delegate void orig_DrawSelf(Terraria.UI.UIElement self, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch);
	private delegate void hook_DrawSelf(orig_DrawSelf orig, Terraria.UI.UIElement self, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch);

	private static MonoMod.RuntimeDetour.Hook _munchiesDrawHook;
	private static System.Collections.Generic.Dictionary<string, int> _animatedHeartTypes;

	public static void Register(Mod elementalHearts)
	{
		if (!ModLoader.TryGetMod(MunchiesModName, out Mod munchies))
			return;

		_animatedHeartTypes = new System.Collections.Generic.Dictionary<string, int>();

		try
		{
			// Munchies 1.3/1.4 doesn't support custom frames for animated items (it draws the entire texture).
			// We detour its internal image element to draw the proper frame if the texture belongs to one of our animated hearts.
			Type centeredUIImageType = munchies.Code.GetType("Munchies.UIElements.CenteredUIImage");
			if (centeredUIImageType != null)
			{
				System.Reflection.MethodInfo drawSelfMethod = centeredUIImageType.GetMethod("DrawSelf", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
				if (drawSelfMethod != null)
				{
					_munchiesDrawHook = new MonoMod.RuntimeDetour.Hook(drawSelfMethod, new hook_DrawSelf(CenteredUIImage_DrawSelf_Detour));
					_munchiesDrawHook.Apply();
				}
			}
		}
		catch (Exception e)
		{
			elementalHearts.Logger.Error($"Failed to hook Munchies CenteredUIImage.DrawSelf: {e.Message}\n{e.StackTrace}");
		}

		try
		{
			foreach (ElementalHeartItem heart in ModContent.GetContent<ElementalHeartItem>())
			{
				if (Terraria.Main.itemAnimations[heart.Type] != null)
					_animatedHeartTypes[heart.Texture] = heart.Type;

				RegisterHeart(elementalHearts, munchies, heart);
			}
		}
		catch (Exception e)
		{
			elementalHearts.Logger.Error($"Failed to register hearts with Munchies: {e.Message}\n{e.StackTrace}");
		}
	}

	private static void CenteredUIImage_DrawSelf_Detour(orig_DrawSelf orig, Terraria.UI.UIElement self, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
	{
		try
		{
			Type type = self.GetType();
			System.Reflection.FieldInfo field = type.GetField("_texture", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			if (field != null)
			{
				var textureAsset = field.GetValue(self) as ReLogic.Content.Asset<Microsoft.Xna.Framework.Graphics.Texture2D>;
				if (textureAsset != null && textureAsset.IsLoaded && _animatedHeartTypes != null)
				{
					if (_animatedHeartTypes.TryGetValue(textureAsset.Name, out int itemType))
					{
						Terraria.DataStructures.DrawAnimation animation = Terraria.Main.itemAnimations[itemType];
						if (animation != null)
						{
							Microsoft.Xna.Framework.Graphics.Texture2D tex = textureAsset.Value;
							Rectangle frame = animation.GetFrame(tex);

							Color color = (Color)type.GetField("Color").GetValue(self);
							float rotation = (float)type.GetField("Rotation").GetValue(self);
							float scale = (float)type.GetField("ImageScale").GetValue(self);
							Vector2 origin = new Vector2(frame.Width, frame.Height) * 0.5f;

							spriteBatch.Draw(
								tex,
								self.GetDimensions().Center(),
								frame,
								color,
								rotation,
								origin,
								scale,
								Microsoft.Xna.Framework.Graphics.SpriteEffects.None,
								0f
							);
							return; // Skip original draw call
						}
					}
				}
			}
		}
		catch
		{
			// Ignore reflection errors and fallback to original draw
		}

		orig(self, spriteBatch);
	}

	private static void RegisterHeart(Mod elementalHearts, Mod munchies, ElementalHeartItem heart)
	{
		// Capture the id so the closure stays cheap and doesn't hold the heart instance.
		string id = heart.ConsumptionId;

		object[] args =
		{
			AddSingleConsumable,
			elementalHearts,
			CallApiVersion,
			heart,
			CategoryPlayer,
			(Func<bool>)(() => ElementalHeartsWorldConfig.Instance.SharedProgression 
				? HeartConsumptionWorld.IsConsumed(id) 
				: Terraria.Main.LocalPlayer?.GetModPlayer<HeartConsumptionPlayer>().IsConsumedLocally(id) ?? false),
			(Color?)heart.MunchiesTextColor,
			heart.MunchiesDifficulty,
			heart.MunchiesExtraTooltip,
			heart.MunchiesAvailability,
			heart.MunchiesAcquisitionText,
		};

		munchies.Call(args);
	}
}
