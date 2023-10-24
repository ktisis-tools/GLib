using Dalamud.Interface;
using Dalamud.Utility;

using GLib.Utility;
using GLib.Popups.ImFileDialog.Data;

namespace GLib.Popups.ImFileDialog; 

public partial class FileDialog {
	private bool IsLoading {
		get {
			lock (this.Files) {
				return this.Files.TokenSource != null;
			}
		}
	}
	
	private class FileState {
		public string? ActiveDirectory;
		public int SelectedCount;
		
		public CancellationTokenSource? TokenSource;
		public List<Entry> AllEntries = new();
		public readonly List<Entry> FilteredEntries = new();

		public void Clear() {
			this.SelectedCount = 0;

			this.TokenSource = null;
			this.AllEntries.Clear();
			this.FilteredEntries.Clear();
		}
	}
	
	// Helpers

	private int MaxCount => this.IsOpenMode ? this.Options.MaxOpenCount : 1;

	private IEnumerable<T> TakeMax<T>(IEnumerable<T> entries) {
		var maxCount = this.MaxCount;
		return maxCount <= 0 ? entries : entries.Take(maxCount);
	}
	
	// History

	private bool CanGoPrevious => !this.PathHistory.IsHeadPrevious;
	private bool CanGoNext => this.PathHistory.CanRedo;

	private void GoPrevious() {
		if (!this.CanGoPrevious) return;
		this.PathHistory.Previous();
		if (this.PathHistory.Current != null)
			this.OpenDirectory(this.PathHistory.Current, false);
	}

	private void GoNext() {
		if (!this.CanGoNext) return;
		this.PathHistory.Next();
		if (this.PathHistory.Current != null)
			this.OpenDirectory(this.PathHistory.Current, false);
	}
	
	// Set current directory

	private Task OpenDirectory(string path, bool logHistory = true) {
		var tokenSrc = new CancellationTokenSource();
		lock (this.Files) {
			this.Files.TokenSource?.Cancel();
			this.Files.TokenSource = tokenSrc;
		}

		var token = tokenSrc.Token;
		tokenSrc.CancelAfter(TimeSpan.FromSeconds(60));
		
		var task = this.OpenDirectoryTask(path, token)
			.ContinueWith(result => {
				lock (this.Files) {
					this.Files.TokenSource?.Dispose();
					this.Files.TokenSource = null;
				}
				
				var failed = token.IsCancellationRequested;
				if (result.Exception != null) {
					failed = true;
					if (result.Exception.InnerException is Exception error)
						this.NotifyError(error);
					this.Options.Logger?.Error(result.Exception.ToString());
				}

				if (failed) {
					this.Ui.PathInput = this.ActiveDirectory ?? string.Empty;
					return;
				}
				
				lock (this.Files) {
					this.ActiveDirectory = path;
					this.Files.AllEntries = result.Result;
					if (logHistory)
						this.PathHistory.AddUnique(path);
					this.ApplyEntryFilters();
				}
			}, token);
		
		return task;
	}

	private async Task<List<Entry>> OpenDirectoryTask(string path, CancellationToken token) {
		await Task.Yield();
		
		#if DEBUG
			if (this.Options._DebugSimulateLag_)
				Thread.Sleep(TimeSpan.FromSeconds(5));
		#endif
		
		var entries = this.BuildFilesForPath(path)
			.TakeWhile(_ => !token.IsCancellationRequested)
			.Select(file => new Entry(file, this.Options))
			.ToList();
		
		token.ThrowIfCancellationRequested();
		
		return entries;
	}
	
	// Directory loading & opening handlers
	
	private IEnumerable<File> BuildFilesForPath(string path) {
		var entries = this.GetFilesInDirectory(path);
		
		var dir = new DirectoryInfo(path);
		if (dir.Parent == null) return entries;
		
		var parent = new File(dir.Parent.FullName) { Name = ".." };
		return entries.Prepend(parent);
	}
	
	private IEnumerable<File> GetFilesInDirectory(string path) {
		// Read subdirectory paths
		IEnumerable<string> paths = Directory.GetDirectories(path);
		
		// Read file paths if in file select mode
		if (!this.IsFolderMode)
			paths = paths.Concat(Directory.GetFiles(path));
		
		// Instantiate paths into File instances
		return paths.Select(file => new File(file));
	}
	
	// Selection

	private void DeselectAll() {
		lock (this.Files) {
			this.Files.AllEntries.ForEach(item => item.IsSelected = false);
		}
	}

	private bool SelectEntry(Entry target, bool isCtrl = false, bool isShift = false) {
		lock (this.Files) {
			var entries = this.Files.AllEntries;
			var prev = this.Ui.LastSelected;

			var maxCount = this.MaxCount;
			var hasMax = maxCount > 0;

			var index = this.Files.FilteredEntries.IndexOf(target);
			var prevIndex = prev == null ? -1 : this.Files.FilteredEntries.IndexOf(prev);
			if (index < 0) return false;

			isShift &= prevIndex >= 0;
			if (isShift && hasMax)
				index = Math.Clamp(index, prevIndex - maxCount + 1, prevIndex + maxCount - 1);
			var min = isShift ? Math.Min(index, prevIndex) : -1;
			var max = isShift ? Math.Max(index, prevIndex) : -1;

			var prevSelectedCount = entries.Count(x => x.IsSelected);
			var prevWasMul = prevSelectedCount > 1;
			var tarWasSelected = target.IsSelected;
			var selectedCount = isCtrl ? prevSelectedCount : 0;
			if (!isCtrl) this.DeselectAll();

			for (var i = 0; i < entries.Count; i++) {
				var entry = entries[i];
				var lastValue = entry.IsSelected;
				var isSelect = lastValue;

				if (isCtrl)
					// Expected behavior: Toggle select state of target (0 -> 1 -> 0), all others remain unchanged.
					isSelect ^= entry == target;
				else
					isSelect = isShift ? i >= min && i <= max : entry == target && !(this.IsOpenMode && tarWasSelected && !prevWasMul);
				
				if (hasMax && isSelect && selectedCount >= maxCount)
					continue;

				entry.IsSelected = isSelect;
				if (isSelect != lastValue)
					selectedCount += isSelect ? 1 : -1;
			}
			
			this.Files.SelectedCount = selectedCount;
			this.UpdateUiFileInput(true);

			var selected = target.IsSelected;
			if (selected && !isShift)
				this.Ui.LastSelected = target;
			return selected;
		}
	}
	
	// Setup sidebar locations
	
	private static readonly Dictionary<Environment.SpecialFolder, FontAwesomeIcon> DefaultLocations = new() {
		{ Environment.SpecialFolder.Desktop, FontAwesomeIcon.Desktop },
		{ Environment.SpecialFolder.MyDocuments, FontAwesomeIcon.File },
		{ Environment.SpecialFolder.Favorites, FontAwesomeIcon.Star },
		{ Environment.SpecialFolder.MyPictures, FontAwesomeIcon.Image },
		{ Environment.SpecialFolder.MyVideos, FontAwesomeIcon.Video },
		{ Environment.SpecialFolder.MyMusic, FontAwesomeIcon.Music }
	};
	
	private List<FileDialogLocation> SetupLocations(FileDialogOptions options) {
		var results = new List<FileDialogLocation>();

		try {
			var i = 0;
			foreach (var (folder, icon) in DefaultLocations) {
				if (FileDialogLocation.TryGet(out var result, folder, icon, i++))
					results.Add(result);
			}
			
			var personal = Path.GetDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
			results.Add(new FileDialogLocation(Path.Join(personal, "Downloads"), FontAwesomeIcon.Download, 1));

			results.AddRange(options.Locations);
			results.Sort((a, b) => a.Position - b.Position);
			results = this.SetupDrives().Concat(results).ToList();
		} catch (Exception err) {
			options.Logger?.Error(err.ToString());
		}
		
		return results;
	}

	private IEnumerable<FileDialogLocation> SetupDrives() => DriveInfo.GetDrives()
		.Where(drive => drive.IsReady)
		.Select(drive => {
			var letter = drive.Name.TrimEnd('\\');
			var label = drive.VolumeLabel.IsNullOrEmpty() ? drive.DriveFormat : drive.VolumeLabel;
			return new FileDialogLocation($"{label} ({letter})", drive.RootDirectory.FullName, FontAwesomeIcon.Hdd);
		});
	
	/// <summary>
	/// Wrapper around a <see cref="File"/> to be displayed.
	/// </summary>
	private class Entry {
		public readonly File File;
		
		public readonly string Name;
		public readonly string Type;
		public readonly string Size;
		public readonly string Modified;

		public readonly FontAwesomeIcon Icon;

		public bool IsSelected;

		public bool IsHidden => this.File.IsHidden; // TODO
		public bool IsDirectory => this.File.IsDirectory;
		
		public Entry(File file, FileDialogOptions options) {
			this.File = file;

			this.Name = file.Name;
			this.Type = file.Type;
			this.Size = Format.FileSize(this.File.Size);
			this.Modified = this.BuildModifiedString();

			if (file.IsDirectory)
				this.Icon = options.DirectoryIcon;
			else
				this.Icon = options.FileIcons.TryGetValue(file.Type, out var icon) ? icon : options.DefaultFileIcon;
		}
		
		public void Open(FileDialog sender) {
			if (this.File.IsDirectory)
				sender.OpenDirectory(this.File.FullPath);
			else if (this.IsSelected)
				sender.Confirm();
		}
		
		private string BuildModifiedString() {
			if (this.File.LastModified == null)
				return string.Empty;

			var value = this.File.LastModified.Value;
			var date = value.ToShortDateString();
			var time = value.ToShortTimeString();
			return $"{date} {time}";
		}
	}
	
	private class File {
		public string Name;
		public string FullPath;
		
		public readonly string Type;
		
		public readonly long Size;
		public readonly DateTime? LastModified;

		private readonly FileAttributes Attributes;

		public bool IsHidden => this.Attributes.HasFlag(FileAttributes.Hidden);
		public bool IsDirectory => this.Attributes.HasFlag(FileAttributes.Directory);

		public File(string path) {
			this.Attributes = System.IO.File.GetAttributes(path);

			FileSystemInfo info = this.IsDirectory ? new DirectoryInfo(path) : new FileInfo(path);
			
			this.Name = info.Name;
			this.FullPath = info.FullName;
			
			this.Type = this.IsDirectory ? "Folder" : ((FileInfo)info).Extension.TrimStart('.');
			
			this.Size = this.IsDirectory ? -1 : ((FileInfo)info).Length;
			this.LastModified = info.LastWriteTime;
		}
	}
}
