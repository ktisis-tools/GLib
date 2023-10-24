// ReSharper disable ConvertToConstant.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable CollectionNeverUpdated.Global

using Dalamud.Interface;
using Dalamud.Plugin.Services;

using GLib.Popups.ImFileDialog.Data;

namespace GLib.Popups.ImFileDialog;

/// <summary>
/// TODO
/// </summary>
[Flags]
public enum FileDialogFlags {
	/// <summary>
	/// Default behavior.
	/// </summary>
	None = 0,
	/// <summary>
	/// Only allows the user to select existing files.
	/// </summary>
	OpenMode = 1,
	/// <summary>
	/// Only allows the user to select folders.
	/// </summary>
	FolderMode = 2
}

/// <summary>
/// Options used to configure <see cref="FileDialog"/>.
/// </summary>
public class FileDialogOptions {
	internal readonly IPluginLog? Logger;

	/// <summary>
	/// TODO
	/// </summary>
	public FileDialogFlags Flags = FileDialogFlags.None;
	
	/// <summary>
	/// A comma-separated string denoting file types to filter for.
	/// </summary>
	/// <example>Source files{.cpp,.h,.hpp},Image files{.png,.gif,.jpg,.jpeg},.md</example>
	public string Filters = "All Files{*}";
	
	/// <summary>
	/// The index of the file extension filter to select by default.
	/// </summary>
	public int ActiveFilter = 0;

	/// <summary>
	/// The maximum number of file selections in open mode. Unlimited if set to zero or lower.
	/// </summary>
	public int MaxOpenCount = 0;

	/// <summary>
	/// The default value of the file name input.
	/// </summary>
	public string DefaultFileName = "Untitled";

	/// <summary>
	/// Extension to enforce when saving files.
	/// </summary>
	public string? Extension;

	/// <summary>
	/// A <see cref="List{T}"/> of custom <see cref="FileDialogLocation"/>s to be added to the sidebar.
	/// </summary>
	public List<FileDialogLocation> Locations = new();
	
	/// <summary>
	/// A <see cref="Dictionary{TKey,TValue}"/> of file icons where the key is the file extension (lowercase, no period) and the value is its correlating <see cref="FontAwesomeIcon"/>.
	/// </summary>
	public Dictionary<string, FontAwesomeIcon> FileIcons = new();
	
	/// <summary>
	/// The icon to display for directories in the file browser.
	/// </summary>
	public FontAwesomeIcon DirectoryIcon = FontAwesomeIcon.Folder;
	/// <summary>
	/// The default icon to display for file types not defined in <see cref="FileIcons"/>.
	/// </summary>
	public FontAwesomeIcon DefaultFileIcon = FontAwesomeIcon.File;
	
	// TODO/Consider: Localization interface?
	public string ConfirmButtonLabel = "Confirm";
	public string CancelButtonLabel = "Cancel";
	
	public string DirectoryLabel = "Folder";

	#if DEBUG
		/// <summary>
		/// DEBUG: If enabled, simulates a 5 second delay when loading directories.
		/// </summary>
		public bool _DebugSimulateLag_ = false;
	#endif

	/// <summary>
	/// Construct a new instance of the <see cref="FileDialogOptions"/> class.
	/// </summary>
	public FileDialogOptions() {}

	/// <summary>
	/// Construct a new instance of the <see cref="FileDialogOptions"/> class.
	/// </summary>
	/// <param name="logger">An instance of <see cref="IPluginLog"/> for error logging.</param>
	public FileDialogOptions(IPluginLog logger) {
		this.Logger = logger;
	}
}
