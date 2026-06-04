using Microsoft.Xna.Framework;
using Terraria.UI;

namespace ElementalHearts.Common.UI.Elements;

/// <summary>
/// A vertical layout container that automatically stacks its children from top to bottom.
/// The last child (or a designated child) can have Height.Set(0, 1f) and it will automatically
/// expand to fill the remaining available height of this container.
/// </summary>
public class UIFlexVertical : UIElement
{
	public float ListPadding = 5f;

	public override void Recalculate()
	{
		// First pass: let base recalculate so we know our own inner dimensions
		base.Recalculate();

		CalculatedStyle innerDimensions = GetInnerDimensions();
		float currentY = 0f;
		UIElement fillElement = null;

		// Calculate total fixed height and position non-fill elements
		foreach (UIElement child in Elements)
		{
			// If an element asks for 100% height (or >0%), treat it as the fill element
			if (child.Height.Percent > 0f)
			{
				fillElement = child;
				continue;
			}

			// Force standard elements to stay horizontally aligned if they have 100% width
			if (child.Width.Percent == 1f)
			{
				child.Width.Set(innerDimensions.Width, 0f);
			}

			child.Top.Set(currentY, 0f);
			child.Recalculate(); // Recalculate child so its outer dimensions update

			currentY += child.GetOuterDimensions().Height + ListPadding;
		}

		// Second pass: give the remaining height to the fill element
		if (fillElement != null)
		{
			float remainingHeight = innerDimensions.Height - currentY;
			if (remainingHeight < 0) remainingHeight = 0;

			fillElement.Top.Set(currentY, 0f);
			fillElement.Height.Set(remainingHeight, 0f);
			
			if (fillElement.Width.Percent == 1f)
			{
				fillElement.Width.Set(innerDimensions.Width, 0f);
			}

			fillElement.Recalculate();
		}
	}
}
