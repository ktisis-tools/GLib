using ImGuiNET;

namespace GLib.Popups.Modals; 

/// <summary>
/// A basic popup modal that displays text with
/// </summary>
public class PopupModalConfirm : PopupModalBase {
	private readonly string Text;
	private readonly bool IsCancellable;

	/// <summary>
	/// Event fired when the user presses confirm.
	/// </summary>
	public event Action? OnConfirm;
	
	/// <summary>
	/// Constructs a new instance of the <see cref="PopupModalConfirm"/> class.
	/// </summary>
	/// <param name="name">The name of the popup window.</param>
	/// <param name="text">The text displayed by the popup.</param>
	/// <param name="isCancellable">Whether the cancel button should be displayed.</param>
	public PopupModalConfirm(string name, string text, bool isCancellable = false) : base(name) {
		this.Text = text;
		this.IsCancellable = isCancellable;
	}
	
	/// <inheritdoc cref="PopupModalBase.DrawInner"/>
	protected override void DrawInner() {
		ImGui.Text(this.Text);
		ImGui.Spacing();
		
		if (ImGui.Button("Confirm")) {
			this.Visible = false;
			this.OnConfirm?.Invoke();
		}
		
		ImGui.SameLine(0, ImGui.GetStyle().ItemInnerSpacing.X);
		if (this.IsCancellable && ImGui.Button("Cancel"))
			this.Visible = false;
	}
}
