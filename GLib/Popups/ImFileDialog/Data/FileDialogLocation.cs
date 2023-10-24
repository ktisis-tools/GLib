using Dalamud.Interface;
using Dalamud.Utility;

namespace GLib.Popups.ImFileDialog.Data; 

/// <summary>
/// Class representing a location in the user's file system.
/// </summary>
public class FileDialogLocation {
	private const FontAwesomeIcon DefaultIcon = FontAwesomeIcon.Folder;
	
	/// <summary>
	/// The display name of this location.
	/// </summary>
	/// 
	public string Name;
	
	/// <summary>
	/// The pull path of this location on the system.
	/// </summary>
	public string FullPath;
	
	/// <summary>
	/// The display position of this location.
	/// </summary>
	/// <remarks>Logical drives always take precedence over this value.</remarks>
	public int Position;
	
	/// <summary>
	/// The icon displayed alongside this location.
	/// </summary>
	public FontAwesomeIcon Icon;

	/// <summary>
	/// Creates a new instance of the <see cref="FileDialogLocation"/> class.
	/// </summary>
	/// <param name="path">The full path of this location's directory. Its name will be resolved automatically.</param>
	/// <param name="icon">The icon to display alongside this location.</param>
	/// <param name="position">This location's position in the <see cref="FileDialog"/> sidebar.</param>
	/// <remarks>Logical drives always take precedence over <paramref name="position"/> values.</remarks>
	public FileDialogLocation(string path, FontAwesomeIcon icon = DefaultIcon, int position = default) {
		this.FullPath = path;
		this.Name = new DirectoryInfo(path).Name;
		this.Icon = icon;
		this.Position = position;
	}

	/// <summary>
	/// Creates a new instance of the <see cref="FileDialogLocation"/> class.
	/// </summary>
	/// <param name="name">The name to display for this location.</param>
	/// <param name="path">The full path of this location's directory.</param>
	/// <param name="icon">The icon to display alongside this location.</param>
	/// <param name="position">This location's position in the <see cref="FileDialog"/> sidebar.</param>
	/// <remarks>Logical drives always take precedence over <paramref name="position"/> values.</remarks>
	public FileDialogLocation(string name, string path, FontAwesomeIcon icon = DefaultIcon, int position = default) {
		this.Name = name;
		this.FullPath = path;
		this.Icon = icon;
		this.Position = position;
	}

	/// <summary>
	/// Attempts to resolve a <see cref="FileDialogLocation"/> associated with a given <see cref="Environment.SpecialFolder"/> value.
	/// </summary>
	/// <param name="result">The resolved <see cref="FileDialogLocation"/> if successful. Uninitialized if unsuccessful.</param>
	/// <param name="target">The special folder folder to resolve.</param>
	/// <param name="icon">The icon to display alongside this location.</param>
	/// <param name="position">This location's position in the <see cref="FileDialog"/> sidebar.</param>
	/// <returns>A boolean indicating whether this location was resolved successfully.</returns>
	/// <remarks>Logical drives always take precedence over <paramref name="position"/> values.</remarks>
	public static bool TryGet(out FileDialogLocation result, Environment.SpecialFolder target, FontAwesomeIcon icon = DefaultIcon, int position = default) {
		try {
			var path = Environment.GetFolderPath(target);
			var valid = !path.IsNullOrEmpty();
			result = valid ? new FileDialogLocation(path, icon, position) : default!;
			return valid;
		} catch {
			result = default!;
			return false;
		}
	}
}
