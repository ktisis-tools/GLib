namespace GLib.Utility;

public static class UiCulling {
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

	public static IEnumerable<T> Scroll<T>(IEnumerable<T> enumerator, int count, float itemHeight, float frameHeight, ref float scroll)
		=> Scroll(enumerator, count, itemHeight, frameHeight, ref scroll, out _, out _);
	
	public static IEnumerable<T> Scroll<T>(IEnumerable<T> enumerator, int count, float itemHeight, float frameHeight, ref float scroll, out float maxScroll)
		=> Scroll(enumerator, count, itemHeight, frameHeight, ref scroll, out maxScroll, out _);
	
	public static IEnumerable<T> Scroll<T>(IEnumerable<T> enumerator, int count, float itemHeight, float frameHeight, ref float scroll, out int start)
		=> Scroll(enumerator, count, itemHeight, frameHeight, ref scroll, out _, out start);
}
