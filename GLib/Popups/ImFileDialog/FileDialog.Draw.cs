using System.Numerics;

using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;

using GLib.Widgets;
using GLib.Popups.ImFileDialog.Data;

namespace GLib.Popups.ImFileDialog; 

public partial class FileDialog {
	private const uint DirectoryColor = 0xFFFDE98B;
	private const uint WarningColor = 0xFF00D8FF;

	private class UiState {
		public bool IsOpen;

		public string PathInput = string.Empty;
		public string FileInput = string.Empty;

		public Entry? LastSelected;

		public bool ShowMetadata = true;
	}
	
	/// <inheritdoc cref="IPopup.Draw"/>
	public bool Draw() {
		var isOpen = this.IsOpen;
		if (!isOpen) return false;
		
		// TODO: Something else
		var size = ImGui.GetIO().DisplaySize;
		ImGui.SetNextWindowSize(size * 0.25f, ImGuiCond.FirstUseEver);
		ImGui.SetNextWindowSizeConstraints(
			new Vector2(200, 200),
			size
		);

		if (!ImGui.Begin(this.Title, ref isOpen, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
			return false;

		try {
			lock (this.Files) {
				this.DrawInner();
				this.DrawPopupModal();
			}
		} finally {
			ImGui.End();
		}
		
		if (!isOpen) this.Close();

		return true;
	}
	
	// Draw UI

	private void DrawInner() {
		this.DrawHeader();
		this.DrawBody();
		this.DrawFooter();
	}
	
	// Draw header; context buttons, path input, search box.

	private void DrawHeader() {
		var isLoading = this.IsLoading;
		
		bool hasMetaHandler;
		bool hasMetadata;
		lock (this.Meta) {
			hasMetaHandler = this.Meta.Handler != null;
			hasMetadata = hasMetaHandler && this.Meta.CurrentData != null;
		}
		
		var innerSpace = ImGui.GetStyle().ItemInnerSpacing.X;
		this.DrawHeaderButtonsLeft(innerSpace, isLoading);
		
		ImGui.SameLine(0, innerSpace);
		var avail = ImGui.GetContentRegionAvail().X - innerSpace;

		if (hasMetaHandler)
			avail -= ImGui.GetFrameHeightWithSpacing();

		using (var _ = ImRaii.Disabled(isLoading)) {
			this.DrawPathInput(avail * 0.70f, isLoading);

			ImGui.SameLine(0, innerSpace);

			ImGui.SetNextItemWidth(avail * 0.30f);
			this.DrawSearchInput();
		}
		
		if (!hasMetaHandler) return;
		
		ImGui.SameLine();
		using (var _ = ImRaii.Disabled(!hasMetadata)) {
			ImGui.SameLine(0, innerSpace);
			if (Buttons.IconButtonTooltip(FontAwesomeIcon.Edit, "Toggle Metadata"))
				this.Ui.ShowMetadata = !this.Ui.ShowMetadata;
		}
	}

	private void DrawHeaderButtonsLeft(float innerSpace, bool isLoading = false) {
		using (ImRaii.Disabled(!this.CanGoPrevious)) {
			if (Buttons.IconButtonTooltip(FontAwesomeIcon.ArrowLeft, "Last Folder"))
				this.GoPrevious();
		}

		ImGui.SameLine(0, innerSpace);
		using (ImRaii.Disabled(!this.CanGoNext)) {
			if (Buttons.IconButtonTooltip(FontAwesomeIcon.ArrowRight, "Next Folder"))
				this.GoNext();
		}

		ImGui.SameLine(0, innerSpace);
		using (ImRaii.Disabled(this.ActiveDirectory == null || isLoading)) {
			if (Buttons.IconButtonTooltip(FontAwesomeIcon.Sync, "Refresh Files") && this.ActiveDirectory != null)
				this.OpenDirectory(this.ActiveDirectory);
			
			ImGui.SameLine(0, innerSpace);
			
			// TODO
			//Buttons.IconButtonTooltip(FontAwesomeIcon.FolderPlus, "Create New Folder");
		}
	}

	private void DrawPathInput(float width, bool isLoading = false) {
		var cursor = ImGui.GetCursorPosX();
		ImGui.SetNextItemWidth(width);
		ImGui.InputTextWithHint($"##{this.Title}_PathInput", "C:/", ref this.Ui.PathInput, 255);

		if (isLoading) {
			using var _ = ImRaii.PushFont(UiBuilder.IconFont);
			
			var icon = FontAwesomeIcon.Spinner.ToIconString();
			var iconSize = ImGui.CalcTextSize(icon);

			var spacing = ImGui.GetStyle().ItemInnerSpacing.X;
			ImGui.SameLine(0, 0);
			ImGui.SetCursorPosX(cursor + width - iconSize.X - spacing);
			ImGui.Text(icon);
			ImGui.SameLine(0, 0);
			ImGui.Dummy(new Vector2(spacing, 0));
		}

		if (!ImGui.IsItemDeactivatedAfterEdit())
			return;

		this.OpenDirectory(this.Ui.PathInput).ContinueWith(_ => {
			this.Ui.PathInput = this.ActiveDirectory ?? string.Empty;
		}, TaskContinuationOptions.NotOnRanToCompletion);
	}

	private void DrawSearchInput() {
		if (ImGui.InputTextWithHint($"##{this.Title}_Search", "Search...", ref this.Filter.Search, 255))
			this.ApplyEntryFilters();
	}
	
	// Draw body; sidebar & folder view

	private bool CanDrawMetadata {
		get {
			lock (this.Meta) {
				return this.Ui.ShowMetadata && this.Meta.CurrentData != null;
			}	
		}
	}

	private void DrawBody() {
		var frameSize = ImGui.GetContentRegionAvail();
		frameSize.Y -= ImGui.GetFrameHeightWithSpacing();

		const ImGuiWindowFlags frameFlags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
		using var frame = ImRaii.Child($"{this.Title}_Body", frameSize, default, frameFlags);
		if (!frame.Success) return;
		
		const ImGuiTableFlags tableFlags = ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInner;
		using var table = ImRaii.Table("##Table", 3, tableFlags, new Vector2(0, 50));
		if (!table.Success) return;
		
		ImGui.TableSetupColumn("##Sidebar", ImGuiTableColumnFlags.WidthFixed);
		ImGui.TableSetupColumn("##Contents");
		ImGui.TableSetupColumn("##Metadata", this.CanDrawMetadata ? 0 : ImGuiTableColumnFlags.Disabled);
		ImGui.TableNextRow();

		this.DrawSidebar(0);
		this.DrawContents(1);
		lock (this.Meta) {
			if (this.CanDrawMetadata)
				this.DrawMetadata(2, this.Meta.CurrentData!);
		}
	}
	
	// Location sidebar

	private void DrawSidebar(int index) {
		float iconSpace;
		using (var _ = ImRaii.PushFont(UiBuilder.IconFont)) {
			iconSpace = this.Locations.Select(loc => loc.Icon)
				.Distinct()
				.Select(icon => ImGui.CalcTextSize(icon.ToIconString()).X)
				.Aggregate((a, b) => a > b ? a : b);
		}
		
		ImGui.TableSetColumnIndex(index);
		ImGui.Spacing();
		foreach (var loc in this.Locations) {
			if (ImGui.Selectable($"##{loc.Name}"))
				this.OpenDirectory(loc.FullPath);
			this.DrawLocationLabel(loc, iconSpace);
		}
		
		ImGui.Dummy(ImGui.GetContentRegionAvail() with { X = 0 });
	}

	// TODO: Merge with DrawEntryLabel
	private void DrawLocationLabel(FileDialogLocation loc, float iconSpace) {
		float padding;
		using (var _ = ImRaii.PushFont(UiBuilder.IconFont)) {
			var icon = loc.Icon.ToIconString();
			var size = ImGui.CalcTextSize(icon).X;
			padding = iconSpace - size / 2;
			ImGui.SameLine(0, padding);
			ImGui.Text(icon);
		}
		ImGui.SameLine(0, padding);
		ImGui.Text(loc.Name);
	}
	
	// Folder contents

	private void DrawContents(int index) {
		ImGui.TableSetColumnIndex(index);
		
		if (this.Files.AllEntries.Count == 0 && this.IsLoading) {
			this.DrawLoadingState();
			return;
		}

		const ImGuiTableFlags flags = ImGuiTableFlags.Resizable | ImGuiTableFlags.Sortable | ImGuiTableFlags.Hideable | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY;
		using var table = ImRaii.Table("##Contents", 4, flags);
		if (!table.Success) return;
		
		// Handle previous & next mouse keys
		var pos = ImGui.GetWindowPos();
		if (ImGui.IsMouseHoveringRect(pos, pos + ImGui.GetWindowSize())) {
			if (this.CanGoPrevious && ImGui.IsMouseClicked((ImGuiMouseButton)3))
				this.GoPrevious();
			if (this.CanGoNext && ImGui.IsMouseClicked((ImGuiMouseButton)4))
				this.GoNext();
 		}
		
		ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.NoHide);
		ImGui.TableSetupColumn("Date modified", ImGuiTableColumnFlags.WidthStretch);
		ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.WidthStretch);
		ImGui.TableSetupColumn("Size", ImGuiTableColumnFlags.WidthStretch);
		ImGui.TableSetupScrollFreeze(4, 1);
		ImGui.TableHeadersRow();
		
		this.DrawFileEntries();
	}

	private void DrawFileEntries() {
		var entries = this.Files.FilteredEntries;
		if (entries.Count == 0)
			return;
		
		float iconSpace;
		using (var _ = ImRaii.PushFont(UiBuilder.IconFont)) {
			iconSpace = entries
				.Select(entry => entry.Icon)
				.Distinct()
				.Select(icon => ImGui.CalcTextSize(icon.ToIconString()).X)
				.Aggregate((a, b) => a > b ? a : b);
		}
		
		var i = 0;
		foreach (var entry in entries) {
			using var _id = ImRaii.PushId(i++);
			using var _col = ImRaii.PushColor(ImGuiCol.Text, DirectoryColor, entry.IsDirectory);
			
			ImGui.TableNextRow();
			ImGui.TableSetColumnIndex(0);
			
			const ImGuiSelectableFlags flags = ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowItemOverlap | ImGuiSelectableFlags.AllowDoubleClick;

			if (ImGui.Selectable(string.Empty, entry.IsSelected, flags))
				this.HandleEntryClicked(entry);
			this.DrawEntryLabel(entry, iconSpace);
			
			ImGui.TableSetColumnIndex(1);
			ImGui.Text(entry.Modified);

			ImGui.TableSetColumnIndex(2);
			ImGui.Text(entry.Type);

			ImGui.TableSetColumnIndex(3);
			ImGui.Text(entry.Size);
			
			if (!this.IsOpen) break;
		}
	}

	private void HandleEntryClicked(Entry entry) {
		var isDouble = ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left);
		var canSelect = !entry.IsDirectory || this.IsFolderMode;
		if (isDouble) {
			entry.Open(this);
		} else if (canSelect) {
			var isShift = ImGui.IsKeyDown(ImGuiKey.ModShift);
			var isCtrl = !isShift && ImGui.IsKeyDown(ImGuiKey.ModCtrl);
			this.HandleSelect(entry, isCtrl, isShift);
		}
	}

	// TODO: Merge with DrawLocationLabel
	private void DrawEntryLabel(Entry entry, float iconSpace) {
		float padding;
		using (var _ = ImRaii.PushFont(UiBuilder.IconFont)) {
			var icon = entry.Icon.ToIconString();
			var size = ImGui.CalcTextSize(icon).X;
			padding = iconSpace - size / 2;
			ImGui.SameLine(0, padding);
			ImGui.Text(icon);
		}
		ImGui.SameLine(0, padding);
		ImGui.Text(entry.Name);
	}
	
	private void DrawLoadingState() {
		const string label = "Loading directory...";

		var avail = ImGui.GetContentRegionAvail();
		var labelSize = ImGui.CalcTextSize(label);

		var center = ImGui.GetCursorPos() + avail / 2;

		using (var _ = ImRaii.PushFont(UiBuilder.IconFont)) {
			var icon = FontAwesomeIcon.Spinner.ToIconString();
			var iconSize = ImGui.CalcTextSize(icon);
			var iconPos = center - iconSize with { X = iconSize.X / 2 };
			ImGui.SetCursorPos(iconPos);
			ImGui.Text(icon);
		}

		var labelPos = center - labelSize with { Y = -labelSize.Y } / 2;
		ImGui.SetCursorPos(labelPos);
		ImGui.Text(label);
	}
	
	// Draw footer; file name, filters, export & cancel buttons

	private void DrawFooter() {
		var style = ImGui.GetStyle();
		
		var framePad = style.FramePadding.X * 2;
		var frameHeight = ImGui.GetFrameHeight();
		
		var confirmWidth = ImGui.CalcTextSize(this.Options.ConfirmButtonLabel).X + framePad;
		var cancelWidth = ImGui.CalcTextSize(this.Options.CancelButtonLabel).X + framePad;
		
		ImGui.AlignTextToFramePadding();
		ImGui.Text("File Name: ");
		ImGui.SameLine(0, style.ItemSpacing.X);
		
		var avail = ImGui.GetContentRegionAvail().X - confirmWidth - cancelWidth - style.ItemSpacing.X * 3;

		var newFileName = false;
		ImGui.SetNextItemWidth(avail * 0.725f);
		if (this.Files.SelectedCount > 1) {
			using var _ = ImRaii.Disabled();
			var text = $"{this.Files.SelectedCount} files selected";
			ImGui.InputTextWithHint("##FileName", string.Empty, ref text, 256);
		} else {
			using var _ = ImRaii.Disabled(this.IsOpenMode);
			newFileName = ImGui.InputTextWithHint("##FileName", this.IsOpenMode ? string.Empty : "File Name", ref this.Ui.FileInput, 256);
		}
		
		if (newFileName) this.UpdateUiFileInput();

		ImGui.SameLine();
		this.DrawFilterSelect(avail * 0.275f);

		ImGui.SameLine();
		using (var _ = ImRaii.Disabled(!this.CanConfirm)) {
			if (ImGui.Button(this.Options.ConfirmButtonLabel, new Vector2(confirmWidth, frameHeight)))
				this.Confirm();
		}

		ImGui.SameLine();
		if (ImGui.Button(this.Options.CancelButtonLabel, new Vector2(cancelWidth, frameHeight)))
			this.Close();
	}

	private void DrawFilterSelect(float width) {
		var filters = this.Filter.Filters;

		ImGui.SetNextItemWidth(width);
		using var _disable = ImRaii.Disabled(filters.Count == 0);
		using var _combo = ImRaii.Combo("##Filter", this.Filter.Active?.Name ?? "All Files");
		if (!_combo.Success) return;

		var i = 0;
		foreach (var filter in filters) {
			using var _ = ImRaii.PushId(i++);
			var select = ImGui.MenuItem(filter.Name, string.Join(", ", filter.Label));
			if (select) this.SetFilter(filter);
		}
	}
	
	// Metadata

	private void DrawMetadata(int index, FileMeta meta) {
		ImGui.TableSetColumnIndex(index);
		
		//using var _style = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, ImGui.GetStyle().ItemSpacing);
		using var _frame = ImRaii.Child(
			"##MetadataFrame",
			ImGui.GetContentRegionAvail(),
			false,
			 ImGuiWindowFlags.AlwaysUseWindowPadding | ImGuiWindowFlags.NoScrollbar
		);
		
		if (!_frame.Success) return;
		
		//ImGui.Dummy(ImGui.GetStyle().ItemSpacing);
		
		if (meta.Texture != null) {
			if (meta.Texture.TryGetWrap(out var texture, out var err)) {
				var avail = ImGui.GetContentRegionAvail();
				avail.Y *= 0.65f;

				var size = texture.Size;
				var ratio = Math.Min(avail.X / size.X, avail.Y / size.Y);
				size *= ratio;

				var padding = (avail.X / 2) - (size.X / 2);
				ImGui.SetCursorPosX(ImGui.GetCursorPosX() + padding);
				ImGui.Image(texture.Handle, size);
				ImGui.Spacing();
			} else if (err != null) {
				using var _ = ImRaii.PushColor(ImGuiCol.Text, WarningColor);
				ImGui.Text("Failed to load image.");
			}
		}
		
		ImGui.Text(meta.Name);

		ImGui.AlignTextToFramePadding();
		if (meta.Description != null)
			ImGui.TextWrapped(meta.Description);
		
		this.DrawMetaProperties(meta.Properties);
	}

	private void DrawMetaProperties(Dictionary<string, string> props) {
		if (props.Count == 0) return;
		
		using var _table = ImRaii.Table("##MetaProperties", 2, ImGuiTableFlags.SizingFixedFit);
		if (!_table.Success) return;

		using var _ = ImRaii.Disabled();

		ImGui.TableSetupColumn("##Property", ImGuiTableColumnFlags.WidthFixed);
		ImGui.TableSetupColumn("##Value", ImGuiTableColumnFlags.WidthStretch);
		
		foreach (var (prop, value) in props) {
			ImGui.TableNextRow();

			ImGui.TableSetColumnIndex(0);
			ImGui.Text($"{prop}:");

			ImGui.TableSetColumnIndex(1);
			ImGui.TextWrapped(value);
		}
	}
	
	// Helpers

	private void UpdateUiFileInput(bool isAuto = false) {
		switch (this.Files.SelectedCount) {
			case 0 when this.IsOpenMode && isAuto:
				this.Ui.FileInput = ".";
				break;
			case 1 when isAuto:
				var selected = this.Files.AllEntries.First(item => item.IsSelected);
				this.Ui.FileInput = selected.Name;
				break;
		}

		var ext = this.Options.Extension;
		if (ext != null && Path.ChangeExtension(this.Ui.FileInput, ext) is string newFileInput)
			this.Ui.FileInput = newFileInput;
	}
}
