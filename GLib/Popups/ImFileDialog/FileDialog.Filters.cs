using System.Text.RegularExpressions;

using Dalamud.Utility;

namespace GLib.Popups.ImFileDialog; 

public partial class FileDialog {
	[GeneratedRegex(@"[^,{}]+(\{([^{}]*?)\})?", RegexOptions.Compiled)]
	private static partial Regex FilterRegex();
	
	private class FilterState {
		public FileFilter? Active;
		public readonly List<FileFilter> Filters = new();

		public string Search = string.Empty;
	}

	private class FileFilter {
		public readonly string Name;
		public readonly string Label;
		private readonly bool IsWildcard;
		private readonly HashSet<string> Extensions;

		public FileFilter(string name, HashSet<string> extensions) {
			var isWildcard = extensions.Count == 0 || extensions.Any(c => c == "*");
			this.Name = name;
			this.Label = isWildcard ? string.Empty : string.Join(", ", extensions);
			this.IsWildcard = isWildcard;
			this.Extensions = extensions;
		}

		public bool Match(string name) {
			if (this.IsWildcard) return true;
			var ext = Path.GetExtension(name);
			return this.Extensions.Any(tar =>  ext.Equals(tar, StringComparison.OrdinalIgnoreCase));
		}
	}
	
	// Filter parsing & setup

	private FilterState SetupFilters(FileDialogOptions options) {
		var state = new FilterState();
		var matches = FilterRegex()
			.Matches(options.Filters)
			.Select(match => {
				var value = match.Value;
				var isNamed = value.Contains('{');
				var extStr = isNamed ? match.Groups[2].Value : value;

				var name = isNamed ? value.Split('{')[0] : value;
				var extensions = extStr.Split(',')
					.Select(ext => ext.Trim())
					.ToHashSet();

				return new FileFilter(name, extensions);
			});

		state.Filters.AddRange(matches);
		
		var active = options.ActiveFilter;
		state.Active = active >= 0 && active < state.Filters.Count ? state.Filters[active] : state.Filters.FirstOrDefault();
		
		return state;
	}
	
	// Filter handling

	private static IEnumerable<Entry> GetMatchesFilter(IEnumerable<Entry> entries, FileFilter filter)
		=> entries.Where(entry => entry.IsDirectory || filter.Match(entry.Name));
	
	private static IEnumerable<Entry> GetMatchesString(IEnumerable<Entry> entries, string query)
		=> entries.Where(entry => entry.Name.Contains(query, StringComparison.OrdinalIgnoreCase));

	private IEnumerable<Entry> GetFiltered(IEnumerable<Entry> entries) {
		var filter = this.Filter.Active;
		if (filter != null)
			entries = GetMatchesFilter(entries, filter);
		
		var search = this.Filter.Search;
		if (!search.IsNullOrEmpty())
			entries = GetMatchesString(entries, search);
		
		return entries;
	}

	private void ApplyEntryFilters() {
		lock (this.Files) {
			var entries = this.GetFiltered(this.Files.AllEntries);
			this.Files.FilteredEntries.Clear();
			this.Files.FilteredEntries.AddRange(entries);
		}
	}

	private void SetFilter(FileFilter filter) {
		this.Filter.Active = filter;
		this.ApplyEntryFilters();
	}
}
