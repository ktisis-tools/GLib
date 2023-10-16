namespace GLib.Utility;

/// <summary>
/// Provides helper functions for improved performance through UI culling.
/// </summary>
public static class UiCulling {
	/// <summary>
	/// Helper function to improve performance of scrollable lists.
	/// </summary>
	/// <param name="enumerator">The input enumerable to reduce into its visible contents.</param>
	/// <param name="count">The total number of items in the enumerator.</param>
	/// <param name="itemHeight">The height of each item to draw.</param>
	/// <param name="frameHeight">The height of the frame to draw items in, determining the number of items to draw.</param>
	/// <param name="scroll">The current scroll height.</param>
	/// <param name="maxScroll">The maximum scrollable height.</param>
	/// <param name="start">The starting index of items to draw for the current scroll height.</param>
	/// <typeparam name="T">The type representing each item in the enumerator.</typeparam>
	/// <returns>An <see cref="IEnumerable{T}"/> containing only the visible contents of <c>enumerator</c>.</returns>
	public static IEnumerable<T> Scroll<T>(
		IEnumerable<T> enumerator,
		int count,
		float itemHeight,
		float frameHeight,
		ref float scroll,
		out float maxScroll,
		out int start
	) {
		maxScroll = itemHeight * count;
		scroll = Math.Min(scroll, maxScroll);

		start = (int)Math.Floor(scroll / itemHeight);
		var displayCt = (int)Math.Floor(frameHeight / itemHeight);
		displayCt = Math.Min(displayCt, count - start);

		return enumerator.Skip(start).Take(displayCt + 1);
	}

	/// <summary>
	/// See <see cref="o:Scroll(IEnumerable{T}, int, float, float, ref float, out float, out int)"/>.
	/// </summary>
	public static IEnumerable<T> Scroll<T>(IEnumerable<T> enumerator, int count, float itemHeight, float frameHeight, ref float scroll)
		=> Scroll(enumerator, count, itemHeight, frameHeight, ref scroll, out _, out _);
	
	/// <summary>
	/// See <see cref="o:Scroll(IEnumerable{T}, int, float, float, ref float, out float, out int)"/>.
	/// </summary>
	public static IEnumerable<T> Scroll<T>(IEnumerable<T> enumerator, int count, float itemHeight, float frameHeight, ref float scroll, out float maxScroll)
		=> Scroll(enumerator, count, itemHeight, frameHeight, ref scroll, out maxScroll, out _);
	
	/// <summary>
	/// See <see cref="o:Scroll(IEnumerable{T}, int, float, float, ref float, out float, out int)"/>.
	/// </summary>
	public static IEnumerable<T> Scroll<T>(IEnumerable<T> enumerator, int count, float itemHeight, float frameHeight, ref float scroll, out int start)
		=> Scroll(enumerator, count, itemHeight, frameHeight, ref scroll, out _, out start);
}
