namespace GLib.Popups.Modals; 

/// <summary>
/// An interface that handles popup modals.
/// </summary>
public interface IPopupModal {
	/// <summary>
	/// Boolean indicating whether or not this popup is visible.
	/// </summary>
	public bool Visible { get; set; }

	/// <summary>
	/// Opens the popup.
	/// </summary>
	public void Open();

	/// <summary>
	/// Draws the popup.
	/// </summary>
	/// <returns>A boolean indicating whether the popup was drawn or not.</returns>
	public bool Draw();
}
