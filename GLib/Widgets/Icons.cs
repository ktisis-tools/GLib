using System.Numerics;

using Dalamud.Interface;

using ImGuiNET;

namespace GLib.Widgets;

public static class Icons {
	// TODO: Document
	public static void DrawIcon(FontAwesomeIcon icon, uint? color = null) {
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
