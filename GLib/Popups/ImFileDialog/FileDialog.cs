using Dalamud.Utility;

using GLib.State;
using GLib.Popups.ImFileDialog.Data;

namespace GLib.Popups.ImFileDialog;

/// <summary>
/// Represents the method that will handle the <see cref="FileDialog.OnConfirmed"/> event.
/// </summary>
public delegate void FileDialogConfirmHandler(FileDialog sender, IEnumerable<string> paths);

/// <summary>
/// A file dialog.
/// </summary>
public partial class FileDialog {
	/// <summary>
	/// The window title of this file dialog.
	/// </summary>
	public readonly string Title;
	private readonly FileDialogOptions Options;
	
	private readonly UiState Ui = new();
	private readonly FileState Files = new();
	
	private readonly FilterState Filter;
	private readonly List<FileDialogLocation> Locations;

	private readonly HistoryState<string> PathHistory = new();
	
	// Events
	
	private event FileDialogConfirmHandler OnConfirmed;
	
	// State helpers
	
	private readonly bool IsOpenMode;
	private readonly bool IsFolderMode;
	
	/// <summary>
	/// Constructs a new instance of the <see cref="FileDialog"/> class with default options.
	/// </summary>
	/// <param name="title">The title of the ImGui window.</param>
	/// <param name="onConfirm">Event fired when a selection is confirmed by the user.</param>
	public FileDialog(
		string title,
		FileDialogConfirmHandler onConfirm
	) {
		var options = new FileDialogOptions();
		this.Title = title;
		this.Options = options;
		this.Locations = this.SetupLocations(options);
		this.Filter = this.SetupFilters(options);
		
		this.OnConfirmed = onConfirm;
	}
	
	/// <summary>
	/// Constructs a new instance of the <see cref="FileDialog"/> class.
	/// </summary>
	/// <param name="title">The title of the ImGui window.</param>
	/// <param name="onConfirm">Event fired when a selection is confirmed by the user.</param>
	/// <param name="options">Options used to configure the file dialog.</param>
	public FileDialog(
		string title,
		FileDialogConfirmHandler onConfirm,
		FileDialogOptions options
	) {
		this.Title = title;
		this.Options = options;
		this.Locations = this.SetupLocations(options);
		this.Filter = this.SetupFilters(options);

		this.OnConfirmed = onConfirm;

		this.IsOpenMode = options.Flags.HasFlag(FileDialogFlags.OpenMode);
		this.IsFolderMode = options.Flags.HasFlag(FileDialogFlags.FolderMode);
	}
	
	/// <summary>
	/// Gets or sets a value indicating whether this window is open or not.
	/// </summary>
	public bool IsOpen {
		get => this.Ui.IsOpen;
		set {
			if (value)
				this.Open();
			else
				this.Close();
		}
	}
	
	/// <summary>
	/// The current active directory in the file browser.
	/// </summary>
	public string? ActiveDirectory {
		get => this.Files.ActiveDirectory;
		private set {
			this.Files.ActiveDirectory = value;
			this.Files.SelectedCount = 0;
			this.Ui.PathInput = value ?? string.Empty;
		}
	}
	
	// Window state

	/// <summary>
	/// Opens the file dialog window.
	/// </summary>
	public void Open()
		=> this.Open(this.ActiveDirectory ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
	
	/// <summary>
	/// Opens the file dialog window.
	/// </summary>
	/// <param name="path">The directory path to open this file dialog in.</param>
	public void Open(string path) {
		this.OpenDirectory(path);
		this.Ui.IsOpen = true;

		var fileInput = this.Options.DefaultFileName;
		if (this.Options.Extension != null)
			fileInput = Path.ChangeExtension(fileInput, this.Options.Extension);
		this.Ui.FileInput = fileInput;
	}

	/// <summary>
	/// Close the file dialog window.
	/// </summary>
	public void Close() {
		this.Ui.IsOpen = false;
		lock (this.Files) {
			this.Files.Clear();
		}
	}

	/// <summary>
	/// Toggles the file dialog window on or off.
	/// </summary>
	public void Toggle() => this.IsOpen = !this.IsOpen;
	
	// Confirm

	private bool CanConfirm => this.Files.SelectedCount > 0 || !this.Ui.FileInput.IsNullOrEmpty();
	
	private void Confirm(bool overwrite = false) {
		if (!this.CanConfirm) return;

		var selected = this.Files.AllEntries
			.Where(item => item.IsSelected)
			.Select(item => item.File.FullPath);

		if (!this.ActiveDirectory.IsNullOrEmpty() && !this.Ui.FileInput.IsNullOrEmpty()) {
			var fullPath = Path.Join(this.ActiveDirectory, this.Ui.FileInput);
			selected = selected.Prepend(fullPath);
		}

		var paths = this.TakeMax(selected.Distinct()).ToList();
		if (!this.IsOpenMode && !overwrite) {
			var exists = paths.Find(Path.Exists);
			if (exists != null) {
				this.NotifyOverwrite(Path.GetFileName(exists));
				return;
			}
		}
		
		this.OnConfirmed?.Invoke(this, paths);
		
		this.Options.Logger?.Debug($"Confirmed {paths.Count} path(s):\n{string.Join('\n', paths)}");
		
		this.Close();
	}
}
