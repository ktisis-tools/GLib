using System.Numerics;

using ImGuiNET;

using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;

namespace GLib.Widgets; 

public static class Buttons {
	public static bool IconButton(FontAwesomeIcon icon, Vector2? size = null) {
		var font = UiBuilder.IconFont;

		if (size == null) {
			var newSize = font.FontSize + ImGui.GetStyle().CellPadding.X * 2;
			size = new Vector2(newSize, newSize);
		}

		using var _ = ImRaii.PushFont(font);
		return ImGui.Button(icon.ToIconString(), size.Value);
	}

	public static bool IconButtonTooltip(FontAwesomeIcon icon, string tooltip, Vector2? size = null) {
		var result = IconButton(icon, size);
		if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) {
			using var _ = ImRaii.Tooltip();
			ImGui.Text(tooltip);
		}
		return result;
	}
}
