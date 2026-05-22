using System.Numerics;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;

namespace GLib.Widgets;

//Thanks Abyeon <3
public class Separators {

	public enum LineHeight {
		Top,
		Middle,
		Bottom
	}

	public static void SeparatorText(ImU8String text, uint lineColor = 0 , float textPosition = 0.3f, float padding = 5f, LineHeight height = LineHeight.Bottom, uint textColor = 0) {

		if (textColor != 0 && lineColor == 0) {
			lineColor = textColor;
		}else if (lineColor == 0 && textColor == 0) {
			textColor = lineColor = ImGui.GetColorU32(ImGuiCol.Text);
		} else if(textColor == 0 && lineColor != 0)
		{
			textColor = ImGui.GetColorU32(ImGuiCol.Text);
		}
		
		var draw = ImGui.GetWindowDrawList();
		var textSize = ImGui.CalcTextSize(text).X;
        
		ImGui.SetCursorPosX((ImGui.GetWindowWidth() * textPosition)- (textSize/2));

		float linePosition = .5f;
		switch (height) {
			case LineHeight.Bottom:
				linePosition = .7f;
				break;
			case LineHeight.Middle:
				linePosition = .5f;
				break;
			case LineHeight.Top:
				linePosition = .3f;
				break;
		}
		var leftOfText = new Vector2
		{
			X = ImGui.GetCursorScreenPos().X - padding,
			Y = ImGui.GetCursorScreenPos().Y + (ImGui.GetTextLineHeight() * linePosition)
		};
        
		ImGui.TextColored(textColor, text);
        
		var rightOfText = leftOfText with
		{
			X = leftOfText.X + ImGui.CalcTextSize(text).X + (padding * 2f)
		};
        
		var width = ImGui.GetWindowWidth();
        
		draw.AddLine(leftOfText, leftOfText with { X = leftOfText.X - width }, lineColor);
		draw.AddLine(rightOfText, rightOfText with { X = rightOfText.X + width }, lineColor);
	}
	
}
