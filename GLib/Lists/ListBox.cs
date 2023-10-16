using System.Numerics;

using ImGuiNET;

using Dalamud.Interface.Utility.Raii;

using GLib.Utility;

namespace GLib.Lists;

public class ListBox<T> {
	private readonly string _label;

	private readonly Func<T, bool, bool> _drawItem;
	private readonly int _itemHeight;

	protected int ActiveIndex = -1;
	
	public ListBox(
		string label,
		Func<T, bool, bool> drawItem,
		int itemHeight = -1
	) {
		this._label = label;
		this._drawItem = drawItem;
		this._itemHeight = itemHeight;
	}
	
	// Draw UI

	private float ItemHeight => this._itemHeight != -1 ? this._itemHeight : ImGui.GetFrameHeight();

	public void Draw(List<T> list, out T? selected)
		=> this.Draw(list, list.Count, out selected);

	public bool Draw(IEnumerable<T> enumerable, int count, out T? selected) {
		// Clamp active index to valid range
		this.ActiveIndex = Math.Clamp(this.ActiveIndex, -1, count - 1);
		
		// Draw ListBox & return result
		using var box = ImRaii.ListBox(this._label);
		return this.DrawInner(enumerable, count, out selected);
	}

	private bool DrawInner(IEnumerable<T> enumerable, int count, out T? selected) {
		selected = default;
		
		var isSelected = false;

		var itemHeight = this.ItemHeight;
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
		if (dummyHeight > 0.0f)
			ImGui.Dummy(new Vector2(0, dummyHeight));
		
		return isSelected;
	}
	
	private int CalcTargetIndex() {
		if (ImGui.IsKeyPressed(ImGuiKey.UpArrow))
			return this.ActiveIndex - 1;
		if (ImGui.IsKeyPressed(ImGuiKey.DownArrow))
			return this.ActiveIndex + 1;
			
		return -1;
	}
}
