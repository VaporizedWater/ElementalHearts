using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace ElementalHearts.Common.UI.Elements;

public class UISmoothScrollbar : UIScrollbar
{
	private float _targetViewPosition;
	private float _lastFrameViewPosition;

	public UISmoothScrollbar() : base()
	{
		_targetViewPosition = ViewPosition;
		_lastFrameViewPosition = ViewPosition;
	}

	public override void ScrollWheel(UIScrollWheelEvent evt)
	{
		float visualPos = ViewPosition;
		
		// Temporarily snap to the target so the setter can calculate clamping accurately based on where we are *going*, not where we *are* visually.
		ViewPosition = _targetViewPosition;
		
		// Instead of base.ScrollWheel(evt) which applies 100% of the scroll delta, we apply 10% sensitivity.
		// The ViewPosition property setter automatically clamps to the valid min/max bounds.
		ViewPosition -= evt.ScrollWheelValue * 0.10f; 
		
		// Bubble the event up to the parent since we skipped calling base.ScrollWheel
		if (Parent != null) Parent.ScrollWheel(evt);
		
		// Save the newly clamped target
		_targetViewPosition = ViewPosition;
		
		// Restore the visual position so we can gracefully lerp from it
		ViewPosition = visualPos;
	}

	public override void Update(GameTime gameTime)
	{
		// Detect if something else (like setting the view boundaries) forced ViewPosition to change between frames
		if (ViewPosition != _lastFrameViewPosition)
		{
			_targetViewPosition = ViewPosition;
		}

		float visualPos = ViewPosition;
		base.Update(gameTime);
		
		if (ViewPosition != visualPos)
		{
			// Dragging logic or base class forced a change
			_targetViewPosition = ViewPosition;
		}
		else
		{
			// Smoothly interpolate towards the target
			if (System.Math.Abs(ViewPosition - _targetViewPosition) > 0.01f)
			{
				ViewPosition = MathHelper.Lerp(ViewPosition, _targetViewPosition, 0.08f);
			}
			else
			{
				ViewPosition = _targetViewPosition;
			}
		}
		
		_lastFrameViewPosition = ViewPosition;
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		float visualPos = ViewPosition;
		base.DrawSelf(spriteBatch);
		
		// Terraria handles scrollbar dragging in DrawSelf in some versions
		if (ViewPosition != visualPos)
		{
			_targetViewPosition = ViewPosition;
			_lastFrameViewPosition = ViewPosition;
		}
	}
}
