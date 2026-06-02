using Microsoft.Xna.Framework;
using Terraria.UI;
using System.Collections.Generic;

namespace ElementalHearts.Common.UI.Elements;

public class UIHorizontalList : UIElement
{
	public float ListPadding = 5f;
	public bool RightAligned = false;
	
	public override void RecalculateChildren()
	{
		base.RecalculateChildren();
		
		float currentOffset = 0f;
		var children = Elements;
		
		if (RightAligned)
		{
			for (int i = children.Count - 1; i >= 0; i--)
			{
				var child = children[i];
				child.HAlign = 1f;
				child.Left.Set(-currentOffset, 0f);
				child.Recalculate();
				currentOffset += child.GetOuterDimensions().Width + ListPadding;
			}
		}
		else
		{
			for (int i = 0; i < children.Count; i++)
			{
				var child = children[i];
				child.HAlign = 0f;
				child.Left.Set(currentOffset, 0f);
				child.Recalculate();
				currentOffset += child.GetOuterDimensions().Width + ListPadding;
			}
		}
	}
}

public class UIWrapList : UIElement
{
	public float ItemSpacing = 10f;
	public float LineSpacing = 10f;

	public override void RecalculateChildren()
	{
		base.RecalculateChildren();
		
		float currentX = 0f;
		float currentY = 0f;
		float maxLineHeight = 0f;
		float containerWidth = GetInnerDimensions().Width;
		
		foreach (var child in Elements)
		{
			float childWidth = child.GetOuterDimensions().Width;
			float childHeight = child.GetOuterDimensions().Height;
			
			if (currentX + childWidth > containerWidth && currentX > 0)
			{
				currentX = 0f;
				currentY += maxLineHeight + LineSpacing;
				maxLineHeight = 0f;
			}
			
			child.Left.Set(currentX, 0f);
			child.Top.Set(currentY, 0f);
			child.HAlign = 0f;
			child.VAlign = 0f;
			child.Recalculate();
			
			currentX += childWidth + ItemSpacing;
			if (childHeight > maxLineHeight)
			{
				maxLineHeight = childHeight;
			}
		}
		
		// Optionally adjust our own height to encapsulate children
		if (Elements.Count > 0)
		{
			this.Height.Set(currentY + maxLineHeight, 0f);
		}
	}
}
