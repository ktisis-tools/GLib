using ImGuiNET;

using Dalamud.Interface.Utility.Raii;

using GLib.Lists;

namespace GLib.Popups;

public class PopupList<T> {
	private readonly string _label;
	
	private readonly ListBox<T> _listBox;
	
	public PopupList(
		string label,
		Func<T, bool, bool> drawItem
	) {
		this._label = label;
		this._listBox = new ListBox<T>(label, drawItem);
	}

	public virtual void Open() {
		ImGui.OpenPopup(this._label);
	}
	
	// Draw UI

	public bool Draw(List<T> list, out T? select)
		=> this.Draw(list, list.Count, out select);
    
	public bool Draw(IEnumerable<T> enumerable, int count, out T? select) {
		using var popup = ImRaii.Popup(this._label);
		select = default;
		return popup.Success && this._listBox.Draw(enumerable, count, out select);
	}
}
