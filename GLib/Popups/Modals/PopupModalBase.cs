using ImGuiNET;

using Dalamud.Interface.Utility.Raii;

namespace GLib.Popups.Modals; 

/// <summary>
/// Base implementation of a <see cref="IPopupModal"/> class.
/// </summary>
public abstract class PopupModalBase : IPopupModal {
	private bool _isOpen;
	
	/// <summary>
	/// The name of this popup window.
	/// </summary>
	public readonly string Name;

	/// <summary>
	/// See <see cref="ImGuiWindowFlags"/>.
	/// </summary>
	protected ImGuiWindowFlags Flags;
	
	/// <inheritdoc cref="IPopupModal.Visible"/>
	public bool Visible {
		get => this._isOpen;
		set => this._isOpen = value;
	}

	/// <summary>
	/// Constructs a new instance of a <see cref="PopupModalBase"/> class.
	/// </summary>
	/// <param name="name">The name of the popup window.</param>
	/// <param name="flags">See <see cref="ImGuiWindowFlags"/>.</param>
	protected PopupModalBase(
		string name,
		ImGuiWindowFlags flags = ImGuiWindowFlags.AlwaysAutoResize
	) {
		this.Name = name;
		this.Flags = flags;
	}

	/// <inheritdoc cref="IPopupModal.Open"/>
	public void Open() {
		// OpenPopup should not be called here due to the possibility of being on a different thread.
		this.Visible = true;
	}
	
	/// <inheritdoc cref="IPopupModal.Draw"/>
	public bool Draw() {
		if (this._isOpen && !ImGui.IsPopupOpen(this.Name))
			ImGui.OpenPopup(this.Name);
		
		using var popup = ImRaii.PopupModal(this.Name, ref this._isOpen, this.Flags);
		if (!popup.Success) return false;
		
		this.DrawInner();
		
		return true;
	}

	/// <summary>
	/// Method called by <see cref="PopupModalBase"/> to draw the inner contents of the popup window.
	/// </summary>
	protected abstract void DrawInner();
}
