using System.Numerics;

using Dalamud.Interface;
using Dalamud.Bindings.ImGui;

namespace GLib.Widgets;

public static class Icons {
	// TODO: Document
	public static void DrawIcon(FontAwesomeIcon icon, uint? color = null) {
		// Workaround: FontAwesomeIcon.None now renders as a caravan instead of being invisible.
		if (icon == FontAwesomeIcon.None) {
			ImGui.Text(string.Empty);
			return;
		}
		
		var hasColor = color.HasValue;
		if (hasColor) ImGui.PushStyleColor(ImGuiCol.Text, color!.Value);
		
		ImGui.PushFont(UiBuilder.IconFont);
		ImGui.Text(icon.ToIconString());
		ImGui.PopFont();

		if (hasColor) ImGui.PopStyleColor();
	}

	// TODO: Document
	public static Vector2 CalcIconSize(FontAwesomeIcon icon) {
		ImGui.PushFont(UiBuilder.IconFont);
		var result = ImGui.CalcTextSize(icon.ToIconString());
		ImGui.PopFont();
		return result;
	}
}
