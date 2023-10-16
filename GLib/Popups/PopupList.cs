using ImGuiNET;

using Dalamud.Interface.Utility.Raii;

using GLib.Lists;

namespace GLib.Popups;

/// <summary>
/// An interactable popup containing a searchable <see cref="ListBox{T}"/>.
/// </summary>
/// <typeparam name="T">The type representing each item in a list.</typeparam>
public class PopupList<T> {
	private readonly string _label;
	
	private readonly ListBox<T> _listBox;
	
	/// <summary>
	/// Constructs a new instance of the <see cref="PopupList{T}"/> class.
	/// </summary>
	/// <param name="label">The label displayed next to the ListBox frame, which uniquely identifies it and the popup.</param>
	/// <param name="drawItem">See <see cref="ListBox{T}.DrawItemDelegate"/>.</param>
	public PopupList(
		string label,
		ListBox<T>.DrawItemDelegate drawItem
	) {
		this._label = label;
		this._listBox = new ListBox<T>(label, drawItem);
	}

	/// <summary>
	/// Opens this popup.
	/// </summary>
	public void Open() {
		ImGui.OpenPopup(this._label);
	}
	
	// Draw UI

	/// <summary>
	/// Draws an ImGui popup containing a searchable list of the items provided.
	/// </summary>
	/// <param name="list">A <see cref="List{T}"/> containing items to draw.</param>
	/// <param name="selected">The selected item if applicable, otherwise null by default.</param>
	/// <returns>A value indicating whether a selection was made by the user.</returns>
	public bool Draw(List<T> list, out T? selected)
		=> this.Draw(list, list.Count, out selected);
    
	/// <summary>
	/// Draws an ImGui popup containing a searchable list of the items provided.
	/// </summary>
	/// <param name="enumerable">An <see cref="IEnumerable{T}"/> containing items to draw.</param>
	/// <param name="count">The number of items to account for, determining the maximum scroll height.</param>
	/// <param name="selected">The selected item if applicable, otherwise null by default.</param>
	/// <returns>A value indicating whether a selection was made by the user.</returns>
	public bool Draw(IEnumerable<T> enumerable, int count, out T? selected) {
		using var popup = ImRaii.Popup(this._label);
		selected = default;
		return popup.Success && this._listBox.Draw(enumerable, count, out selected);
	}
}
