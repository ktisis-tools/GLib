namespace GLib.Popups; 

/// <summary>
/// An interface for implementing ImGui popups.
/// </summary>
public interface IPopup {
	/// <summary>
	/// Gets or sets a value indicating whether this popup is open or not.
	/// </summary>
	public bool IsOpen { get; set; }

	/// <summary>
	/// Draws the popup.
	/// </summary>
	/// <returns>True if the popup was drawn. False if isn't opened.</returns>
	public bool Draw();
}
