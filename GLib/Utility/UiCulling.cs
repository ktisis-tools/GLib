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
}
