namespace GLib.Utility; 

/// <summary>
/// Provides helper functions for string formatting.
/// </summary>
public static class Format {
	private static readonly string[] Suffix = { "B", "KB", "MB", "GB", "TB" };
	
	/// <summary>
	/// Formats file sizes into human-readable text.
	/// </summary>
	/// <param name="bytes">The size in bytes to format into text.</param>
	/// <returns>The formatted string.</returns>
	public static string FileSize(long bytes) {
		switch (bytes) {
			case 0:
				return $"0 {Suffix[0]}";
			case < 0:
				return string.Empty;
			default:
				var oom = Math.Min(
					(int)Math.Floor(Math.Log(bytes, 1024)),
					Suffix.Length - 1
				);
				var fmtBytes = Math.Round(bytes / Math.Pow(1024, oom), 1);
				return $"{fmtBytes} {Suffix[oom]}";
		}
	}
}
