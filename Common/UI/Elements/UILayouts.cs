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
		
		float containerWidth = GetInnerDimensions().Width;
		if (containerWidth <= 0f)
		{
			return;
		}

		// First pass: Group children into rows
		List<List<UIElement>> rows = new();
		List<UIElement> currentRow = new();
		float currentRowWidth = 0f;
		
		foreach (var child in Elements)
		{
			float childWidth = child.GetOuterDimensions().Width;
			float spacingNeeded = currentRow.Count > 0 ? ItemSpacing : 0f;
			
			if (currentRowWidth + spacingNeeded + childWidth > containerWidth && currentRow.Count > 0)
			{
				rows.Add(currentRow);
				currentRow = new List<UIElement>();
				currentRowWidth = 0f;
				spacingNeeded = 0f;
			}
			currentRow.Add(child);
			currentRowWidth += spacingNeeded + childWidth;
		}
		if (currentRow.Count > 0)
		{
			rows.Add(currentRow);
		}

		// Calculate spacing of the first row to use as a template for the last row
		float templateSpacing = ItemSpacing;
		if (rows.Count > 0 && rows[0].Count > 1)
		{
			float totalW = 0f;
			foreach (var child in rows[0])
			{
				totalW += child.GetOuterDimensions().Width;
			}
			templateSpacing = (containerWidth - totalW) / (rows[0].Count - 1);
		}

		// Second pass: Layout elements in each row
		float currentY = 0f;
		float maxLineHeight = 0f;

		for (int r = 0; r < rows.Count; r++)
		{
			var row = rows[r];
			float totalRowElementsWidth = 0f;
			foreach (var child in row)
			{
				totalRowElementsWidth += child.GetOuterDimensions().Width;
			}

			float rowSpacing = ItemSpacing;
			bool isLastRow = (r == rows.Count - 1);
			
			if (row.Count > 1)
			{
				if (isLastRow && rows.Count > 1 && row.Count < rows[0].Count)
				{
					// Use template spacing from first row to keep grid alignment
					rowSpacing = templateSpacing;
				}
				else
				{
					// Distribute space evenly so first and last elements align to the edges
					rowSpacing = (containerWidth - totalRowElementsWidth) / (row.Count - 1);
				}
			}
			else
			{
				rowSpacing = 0f;
			}

			float currentX = 0f;
			float rowMaxHeight = 0f;

			foreach (var child in row)
			{
				child.Left.Set(currentX, 0f);
				child.Top.Set(currentY, 0f);
				child.HAlign = 0f;
				child.VAlign = 0f;
				child.Recalculate();

				currentX += child.GetOuterDimensions().Width + rowSpacing;
				
				float childHeight = child.GetOuterDimensions().Height;
				if (childHeight > rowMaxHeight)
				{
					rowMaxHeight = childHeight;
				}
			}

			currentY += rowMaxHeight;
			if (r < rows.Count - 1)
			{
				currentY += LineSpacing;
			}
			
			if (rowMaxHeight > maxLineHeight)
			{
				maxLineHeight = rowMaxHeight;
			}
		}

		if (Elements.Count > 0)
		{
			this.Height.Set(currentY, 0f);
			this.MinHeight.Set(currentY, 0f);
		}
	}

	public override void Recalculate()
	{
		base.Recalculate(); // This computes initial width and calls RecalculateChildren (which updates our Height)
		
		// If our calculated dimension doesn't match the new Height, we must recalculate one more time 
		// so the parent UIList can read the correct GetOuterDimensions().Height for the scrollbar!
		if (GetOuterDimensions().Height != Height.Pixels && Height.Pixels > 0)
		{
			base.Recalculate();
		}
	}
}
