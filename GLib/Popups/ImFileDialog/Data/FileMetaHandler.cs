using Dalamud.Interface;

namespace GLib.Popups.ImFileDialog.Data; 

/// <summary>
/// Represents the handler method passed to <see cref="FileMetaHandler.AddFileType"/>.
/// </summary>
public delegate FileMeta? FileMetaBuildDelegate(string path);

/// <summary>
/// Interface for providing file metadata.
/// </summary>
public sealed class FileMetaHandler {
	private readonly UiBuilder _uiBuilder;
	
	private readonly Dictionary<string, FileMetaBuildDelegate> Handlers = new();

	/// <summary>
	/// Constructs a new instance of the <see cref="FileMetaHandler"/> class.
	/// </summary>
	/// <param name="uiBuilder">A <see cref="UiBuilder"/> instance.</param>
	public FileMetaHandler(UiBuilder uiBuilder) {
		this._uiBuilder = uiBuilder;
	}

	/// <summary>
	/// Registers a metadata handler for the specified file type.
	/// </summary>
	/// <param name="type"></param>
	/// <param name="handler"></param>
	/// <returns></returns>
	public FileMetaHandler AddFileType(string type, FileMetaBuildDelegate handler) {
		this.Handlers.Add(type, handler);
		return this;
	}

	/// <summary>
	/// Tries to build a <see cref="FileMeta"/> instance for a given file path.
	/// </summary>
	/// <param name="path">The path of the file to access metadata for.</param>
	/// <param name="result">The resulting <see cref="FileMeta"/> instance is successful. Otherwise, null.</param>
	/// <returns>A boolean indicating whether the operation was successful or not.</returns>
	public bool TryGetData(string path, out FileMeta? result) {
		result = null;

		var ext = Path.GetExtension(path);
		if (!this.Handlers.TryGetValue(ext, out var handler))
			return false;
		
		result = handler.Invoke(path);
		if (result?.ImageData is { Length: > 0 })
			result.Texture = this._uiBuilder.LoadImage(result.ImageData);

		return result is { IsEmpty: false };
	}
}
