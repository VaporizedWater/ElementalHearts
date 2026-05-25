namespace ElementalHearts.Common.Worldgen;

/// <summary>
/// Generates and caches heart-shaped boolean masks used to lay out the Jungle Life
/// Mini-Biome. The mask is computed from the standard implicit heart curve
/// <c>(x² + y² - 1)³ - x²·y³ = 0</c> with the point oriented downward, matching the
/// reading direction of the player-visible heart icon.
/// </summary>
public static class HeartShape
{
	private static bool[,] _cached;
	private static int _cachedWidth;
	private static int _cachedHeight;

	/// <summary>
	/// Returns a <paramref name="width"/>×<paramref name="height"/> mask where
	/// <c>true</c> cells lie inside the heart. The most recent request is cached;
	/// repeated calls at the same dimensions reuse the same array.
	/// </summary>
	public static bool[,] Get(int width, int height)
	{
		if (_cached != null && _cachedWidth == width && _cachedHeight == height)
			return _cached;

		_cached = Build(width, height);
		_cachedWidth = width;
		_cachedHeight = height;
		return _cached;
	}

	private static bool[,] Build(int width, int height)
	{
		bool[,] mask = new bool[width, height];

		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				// Map (x, y) into the heart equation's [-1.2, 1.2] working box. Y is
				// flipped so the heart's point lands at the bottom of the array — the
				// expected reading orientation when this lays out into tile space.
				float nx = (x / (float)(width - 1) - 0.5f) * 2.4f;
				float ny = -(y / (float)(height - 1) - 0.5f) * 2.4f;

				float a = nx * nx + ny * ny - 1f;
				float lhs = a * a * a - nx * nx * ny * ny * ny;

				mask[x, y] = lhs <= 0f;
			}
		}

		return mask;
	}
}
