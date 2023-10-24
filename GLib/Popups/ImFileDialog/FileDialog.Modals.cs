using GLib.Popups.Modals;

namespace GLib.Popups.ImFileDialog; 

public partial class FileDialog {
	private PopupModalBase? PopupModal;

	private void DrawPopupModal() {
		var popup = this.PopupModal;
		if (popup == null) return;

		if (popup.Visible)
			popup.Draw();
		else
			this.PopupModal = null;
	}
	
	private void ActivatePopup(PopupModalBase popup) {
		this.PopupModal = popup;
		this.PopupModal.Open();
	}

	private void NotifyOverwrite(string fileName) {
		var popup = new PopupModalConfirm(
			"Save File",
			$"{fileName} will be overwritten. Are you sure?",
			isCancellable: true
		);
		popup.OnConfirm += () => this.Confirm(overwrite: true);
		this.ActivatePopup(popup);
	}

	private void NotifyError(Exception error)
		=> this.ActivatePopup(new PopupModalConfirm("Error",error.Message));
}
