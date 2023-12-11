using ImGuiNET;

using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;

using GLib.Lists;

namespace GLib.Popups;

/// <summary>
/// An interactable popup containing a searchable <see cref="ListBox{T}"/>.
/// </summary>
/// <typeparam name="T">The type representing each item in a list.</typeparam>
public class PopupList<T> {
	private readonly string _id;
	
	private readonly ListBox<T> _listBox;

	private string _searchInput = string.Empty;
	private List<T>? _searchResults;
	
	// Events

	/// <summary>
	/// Method called when filtering list items with the search bar.
	/// </summary>
	/// <param name="item">The item to compare.</param>
	/// <param name="query">The search query to test.</param>
	/// <returns>Whether this item matches this search query.</returns>
	public delegate bool SearchPredicate(T item, string query);
	
	private SearchPredicate? _search;
	
	/// <summary>
	/// Constructs a new instance of the <see cref="PopupList{T}"/> class.
	/// </summary>
	/// <param name="id">The label displayed next to the ListBox frame, which uniquely identifies it and the popup.</param>
	/// <param name="drawItem">See <see cref="ListBox{T}.DrawItemDelegate"/>.</param>
	/// <param name="itemHeight">
	/// The height of each item to draw.
	/// If set to zero or lower, the return value of <c>ImGui.GetFrameHeight()</c> will be used.
	/// </param>
	public PopupList(
		string id,
		ListBox<T>.DrawItemDelegate drawItem,
		float itemHeight = -1
	) {
		this._id = id;
		this._listBox = new ListBox<T>(id, drawItem, itemHeight);
	}
	
	// Factory methods

	/// <summary>
	/// Supply a <see cref="PopupList{T}.SearchPredicate"/> to be used by a search bar.
	/// </summary>
	/// <param name="predicate">See <see cref="PopupList{T}.SearchPredicate"/>.</param>
	/// <returns>Returns this <see cref="PopupList{T}"/> instance for fluent configuration.</returns>
	public PopupList<T> WithSearch(SearchPredicate predicate) {
		this._search = predicate;
		return this;
	}

	/// <summary>
	/// Opens this popup.
	/// </summary>
	public void Open() {
		ImGui.OpenPopup(this._id);
	}
	
	/// <summary>
	/// Wrapper around `ImGui.IsPopupOpen`.
	/// </summary>
	/// <returns>A value indicating whether this popup is currently open.</returns>
	public bool IsOpen => ImGui.IsPopupOpen(this._id);
	
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
		selected = default;
		
		using var popup = ImRaii.Popup(this._id);
		if (!popup.Success)
			return false;
		
		this.DrawSearchBar(enumerable);
        
		return this._listBox.Draw(
			this._searchResults ?? enumerable,
			this._searchResults?.Count ?? count,
			out selected
		);
	}
	
	// Search draw

	private void DrawSearchBar(IEnumerable<T> enumerable) {
		if (this._search == null)
			return;

		if (ImGui.InputTextWithHint($"##{this._id}_Search", "Search...", ref this._searchInput, 256)) {
			if (this._searchInput.IsNullOrEmpty())
				this._searchResults = null;
			else
				this._searchResults = enumerable
					.Where(item => this._search.Invoke(item, this._searchInput))
					.ToList();
		}

		if (!ImGui.IsAnyItemActive())
			ImGui.SetKeyboardFocusHere(-1);
	}
}
