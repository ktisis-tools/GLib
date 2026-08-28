using System.Numerics;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace GLib.Widgets;

public static class Buttons {
	public static float CalcSize() => (UiBuilder.DefaultFontSizePx + ImGui.GetStyle().CellPadding.X * 2) * ImGuiHelpers.GlobalScale;
	
	public static bool IconButton(FontAwesomeIcon icon, Vector2? size = null, Vector4? iconColor = null) {
		if (size == null) {
			var newSize = CalcSize();
			size = new Vector2(newSize, newSize);
		}
		bool ret;
		using (ImRaii.PushFont(UiBuilder.IconFont)) {
			using var _ = iconColor != null ? ImRaii.PushColor(ImGuiCol.Text, iconColor!.Value) : null;
			ret = ImGui.Button(icon.ToIconString(), size.Value);
		}
		return ret;
	}

	public static bool IconButtonTooltip(FontAwesomeIcon icon, string tooltip, Vector2? size = null, Vector4? iconColor = null) {
		var result = IconButton(icon, size, iconColor);
		if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) {
			using var _ = ImRaii.Tooltip();
			ImGui.Text(tooltip);
		}
		return result;
	}

	public static bool IconButtonDropdown(FontAwesomeIcon icon, Action list, Vector2? size = null, Vector4? iconColor = null) {
		var style = new ImRaii.StyleDisposable();
		style.Push(ImGuiStyleVar.ItemInnerSpacing, Vector2.Zero);
		style.Push(ImGuiStyleVar.ItemSpacing, Vector2.Zero);

		const FontAwesomeIcon caret = FontAwesomeIcon.CaretDown;
		var cursor = ImGui.GetCursorPos();
		var calcedSize = CalcSize();
		var ret = false;

		size ??= new Vector2(calcedSize * 1.8f, calcedSize);
		var iconSize = ImGui.CalcTextSize(icon.ToIconString());
		var caretSize = ImGui.CalcTextSize(caret.ToIconString());
		var leftoverSpacing = size.Value - caretSize - iconSize;

		using (ImRaii.Group()) {
			using (ImRaii.PushFont(UiBuilder.IconFont)) {
				using var _ = iconColor != null ? ImRaii.PushColor(ImGuiCol.Text, iconColor!.Value) : null;
				ret = ImGui.Button($"##KtisisDropdownButton_{icon.ToString()}", size: size.Value);

				// draw icon
				ImGui.SameLine();
				ImGui.AlignTextToFramePadding();
				ImGui.SetCursorPosX(cursor.X + ImGui.GetStyle().FramePadding.X);
				ImGui.Text(icon.ToIconString());

				// draw caret
				ImGui.SameLine();
				ImGui.SetCursorPosX(cursor.X + size.Value.X - (caretSize.X + ImGui.GetStyle().FramePadding.X));
				ImGui.Text(caret.ToIconString());

				// draw separator
				ImGui.SetCursorPosX(cursor.X);
				ImGuiP.GetCurrentWindow().DrawList.AddLine(
					ImGui.GetWindowPos() + cursor + new Vector2(
						iconSize.X + ImGui.GetStyle().FramePadding.X + (leftoverSpacing.X /2), 2
					),ImGui.GetWindowPos() + cursor + new Vector2(
						iconSize.X + ImGui.GetStyle().FramePadding.X + (leftoverSpacing.X /2), calcedSize - 2
					), ImGui.GetColorU32(ImGuiCol.TextDisabled)
				);
			}
		}

		// check for mouse click inside caret collision box
		var popupBox = new ImRect(ImGui.GetWindowPos() + cursor + new Vector2(iconSize.X + ImGui.GetStyle().FramePadding.X + (leftoverSpacing.X / 2), 0), ImGui.GetWindowPos() + cursor + size.Value);
		style.Dispose();

		if (ret && popupBox.Contains(ImGui.GetMousePos())) {
			list.Invoke();
			return false;
		}

		return ret;
	}
}
