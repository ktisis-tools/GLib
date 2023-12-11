using System.Numerics;

using ImGuiNET;

using Dalamud.Interface.Utility.Raii;

using GLib.Utility;

namespace GLib.Lists;

/// <summary>
/// An implementation of ImGui's <c>ListBox</c> utilizing <see cref="UiCulling"/> for improved performance.
/// </summary>
/// <typeparam name="T">The type representing each item in a list.</typeparam>
public class ListBox<T> {
	/// <summary>
	/// Invoked when drawing each item in a <see cref="ListBox{T}"/>.
	/// </summary>
	/// <param name="item">The item to draw.</param>
	/// <param name="isFocused">A value indicating whether this item is currently focused.</param>
	/// <returns>A value indicating whether the item was activated in this draw call.</returns>
	public delegate bool DrawItemDelegate(T item, bool isFocused);
	
	// Properties
	
	private readonly string _label;

	private readonly DrawItemDelegate _drawItem;

	/// <summary>
	/// The index of the current focus target.
	/// </summary>
	protected int ActiveIndex = -1;
	
	/// <summary>
	/// Constructs a new instance of the <see cref="ListBox{T}"/> class.
	/// </summary>
	/// <param name="label">The label displayed next to the ListBox frame, which uniquely identifies it.</param>
	/// <param name="drawItem">See <see cref="DrawItemDelegate"/>.</param>
	public ListBox(
		string label,
		DrawItemDelegate drawItem
	) {
		this._label = label;
		this._drawItem = drawItem;
	}
	
	// Draw UI
	
	private float HeightOrCalcDefault(float itemHeight)
		=> itemHeight <= 0.0f ? ImGui.GetTextLineHeight() : itemHeight;

	/// <summary>
	/// Draws an ImGui ListBox containing items provided by an enumerable.
	/// </summary>
	/// <param name="list">A <see cref="List{T}"/> containing items to draw.</param>
	/// <param name="selected">The selected item if applicable, otherwise null by default.</param>
	/// <param name="itemHeight">
	/// The height of each item to draw, calculated automatically if value is set to zero or lower.
	/// </param>
	/// <param name="boxSize">Optional: A fixed size for the ListBox to draw in.</param>
	/// <returns>A value indicating whether a selection was made by the user.</returns>
	public bool Draw(
		List<T> list,
		out T? selected,
		float itemHeight = -1,
		Vector2? boxSize = null
	) => this.Draw(list, list.Count, out selected, itemHeight, boxSize);
	
	/// <summary>
	/// Draws an ImGui ListBox containing items provided by an enumerable.
	/// </summary>
	/// <param name="enumerable">An <see cref="IEnumerable{T}"/> containing items to draw.</param>
	/// <param name="count">The number of items to account for, determining the maximum scroll height.</param>
	/// <param name="selected">The selected item if applicable, otherwise null by default.</param>
	/// <param name="itemHeight">
	/// The height of each item to draw, calculated automatically if value is set to zero or lower.
	/// </param>
	/// <param name="boxSize">Optional: A fixed size for the ListBox to draw in.</param>
	/// <returns>A value indicating whether a selection was made by the user.</returns>
	public bool Draw(
		IEnumerable<T> enumerable,
		int count,
		out T? selected,
		float itemHeight = -1,
		Vector2? boxSize = null
	) {
		selected = default;
		
		// Clamp active index to valid range
		this.ActiveIndex = Math.Clamp(this.ActiveIndex, -1, count - 1);
		
		// Draw ListBox & return result
		using var box = ImRaii.ListBox(this._label, boxSize ?? Vector2.Zero);
		return box.Success && this.DrawInner(enumerable, count, out selected, itemHeight);
	}

	/// <summary>
	/// Draws the inner contents of the ImGui <c>ListBox</c> frame.
	/// </summary>
	private bool DrawInner(
		IEnumerable<T> enumerable,
		int count,
		out T? selected,
		float itemHeight = -1
	) {
		selected = default;
		
		var isSelected = false;
		
		itemHeight = this.HeightOrCalcDefault(itemHeight) + ImGui.GetStyle().ItemSpacing.Y;
		var frameHeight = ImGui.GetWindowSize().Y;
		
		// Take keyboard input
		
		var target = this.CalcTargetIndex();
		var isEnter = ImGui.IsKeyPressed(ImGuiKey.Enter);

		if (target != -1)
			ImGui.SetScrollY(((target + 1) * itemHeight) - (frameHeight / 2));
		
		// Calculate bounds

		var scroll = ImGui.GetScrollY();
		enumerable = UiCulling.Scroll(
			enumerable,
			count,
			itemHeight,
			frameHeight,
			ref scroll,
			out var maxScroll,
			out var start
		);
		
		// Empty content region for items skipped by scrolling
		ImGui.Dummy(new Vector2(0, scroll));
		
		// Display visible content

		var index = start;
		foreach (var item in enumerable) {
			using var _ = new ImRaii.Id().Push(index);
			
			var isFocus = index == this.ActiveIndex;
			var activate = this._drawItem(item, isFocus);
			activate |= index == target || isFocus && isEnter;
			if (activate) {
				selected = item;
				this.ActiveIndex = index;
				isSelected = true;
			}
				
			index++;
		}
		
		// Empty content region for remaining scrollable space
		var dummyHeight = maxScroll - ImGui.GetCursorPosY();
		if (dummyHeight >= 1.0f)
			ImGui.Dummy(new Vector2(0, dummyHeight));
		
		return isSelected;
	}
	
	/// <summary>
	/// Checks user input for up/down keys to choose a new focus target.
	/// </summary>
	/// <returns>The new active index if applicable, otherwise <c>-1</c> by default.</returns>
	private int CalcTargetIndex() {
		if (ImGui.IsKeyPressed(ImGuiKey.UpArrow))
			return this.ActiveIndex - 1;
		if (ImGui.IsKeyPressed(ImGuiKey.DownArrow))
			return this.ActiveIndex + 1;
			
		return -1;
	}
}
