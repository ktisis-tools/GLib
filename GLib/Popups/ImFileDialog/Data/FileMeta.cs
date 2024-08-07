using Dalamud.Utility;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;

namespace GLib.Popups.ImFileDialog.Data; 

/// <summary>
/// Class representing file metadata displayed by <see cref="FileDialog"/>.
/// </summary>
public class FileMeta {
	/// <summary>
	/// The name of the file.
	/// </summary>
	public readonly string Name;
	
	/// <summary>
	/// A description of the file's content.
	/// </summary>
	public string? Description;
	
	/// <summary>
	/// Raw buffer containing data for a preview image.
	/// </summary>
	public byte[]? ImageData;
	
	/// <summary>
	/// A <see cref="ISharedImmediateTexture"/> for displaying the preview image in ImGui.
	/// </summary>
	public ISharedImmediateTexture? Texture;

	/// <summary>
	/// A <see cref="IDalamudTextureWrap"/> for displaying the preview image in ImGui.
	/// </summary>
	public IDalamudTextureWrap? TextureWrap;
	
	/// <summary>
	/// A <see cref="Dictionary{TKey,TValue}"/> containing miscellaneous properties.
	/// </summary>
	public readonly Dictionary<string, string> Properties = new();

	/// <summary>
	/// A boolean indicating whether any metadata properties have been set.
	/// </summary>
	public bool IsEmpty => this.Description.IsNullOrEmpty() && this.Texture == null && this.ImageData == null && this.Properties.Count == 0;

	/// <summary>
	/// Constructs a new instance of the <see cref="FileMeta"/> class.
	/// </summary>
	/// <param name="name"></param>
	public FileMeta(string name) {
		this.Name = name;
	}

	/// <summary>
	/// Adds a new property to <see cref="FileMeta.Properties"/>.
	/// </summary>
	/// <param name="name">The name of the property.</param>
	/// <param name="value">The associated text to display.</param>
	/// <returns>Self-referential for fluent method chaining.</returns>
	public FileMeta AddProperty(string name, string value) {
		this.Properties.Add(name, value);
		return this;
	}
	
	/// <summary>
	/// Assigns <see cref="FileMeta.ImageData"/> converted frm a base64 string.
	/// </summary>
	/// <param name="base64">The base64 string to convert into an image/</param>
	/// <returns>Self-referential for fluent method chaining.</returns>
	public FileMeta WithBase64Image(string base64) {
		this.ImageData = Convert.FromBase64String(base64);
		return this;
	}

	/// <summary>
	/// Retrieves a <see cref="IDalamudTextureWrap"/> from <see cref="Texture"/> or <see cref="TextureWrap"/>.
	/// </summary>
	/// <param name="textureWrap">The resulting texture wrap.</param>
	/// <returns>A boolean indicating whether the function call succeeded.</returns>
	public bool TryGetTextureWrap(out IDalamudTextureWrap? textureWrap) {
		if (this.TextureWrap != null) {
			textureWrap = this.TextureWrap;
			return true;
		}

		if (this.Texture != null && this.Texture.TryGetWrap(out textureWrap, out _)) {
			this.TextureWrap = textureWrap;
			return true;
		}

		textureWrap = null;
		return false;
	}
}
