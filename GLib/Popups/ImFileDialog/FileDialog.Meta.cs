using GLib.Popups.ImFileDialog.Data;

namespace GLib.Popups.ImFileDialog; 

public partial class FileDialog {
	private readonly MetaState Meta = new();
	
	private class MetaState {
		public FileMetaHandler? Handler;

		public string? CurrentPath;
		public FileMeta? CurrentData;

		public CancellationTokenSource? TokenSource;

		public void Clear() {
			this.CurrentPath = null;
			this.CurrentData = null;
		}
	}

	/// <summary>
	/// Attaches a <see cref="FileMetaHandler"/> to the file dialog.
	/// </summary>
	/// <param name="handler">The <see cref="FileMetaHandler"/> to attach.</param>
	/// <returns>Self-referential for fluent method chaining.</returns>
	public FileDialog WithMetadata(FileMetaHandler handler) {
		lock (this.Meta) {
			this.Meta.Handler = handler;
		}
		return this;
	}

	private void UpdateMetadata(Entry? entry) {
		var path = entry?.File.FullPath;
		
		var tokenSrc = new CancellationTokenSource();
		lock (this.Meta) {
			if (path == this.Meta.CurrentPath)
				return;

			if (entry == null) {
				this.Meta.Clear();
				return;
			}
			
			this.Meta.TokenSource?.Cancel();
			this.Meta.TokenSource = tokenSrc;

			this.Meta.CurrentPath = path!;
		}
		
		var token = tokenSrc.Token;
		tokenSrc.CancelAfter(TimeSpan.FromSeconds(60));

		this.UpdateMetadataTask(path!, token).ContinueWith(task => {
			var failure = token.IsCancellationRequested;
			if (task.Exception != null) {
				failure = true;
				this.Options.Logger?.Error($"Failed to load metadata:\n{task.Exception}");
			}
			
			lock (this.Meta) {
				this.Meta.TokenSource = null;
				this.Meta.CurrentData = failure ? null : task.Result;
				
				this.Options.Logger?.Debug($"Loaded metadata? {this.Meta.CurrentData != null}");
			}
		}, token);
	}

	private async Task<FileMeta?> UpdateMetadataTask(string path, CancellationToken token) {
		await Task.Yield();

		FileMetaHandler handler;
		lock (this.Meta) {
			if (this.Meta.Handler == null)
				return null;
			handler = this.Meta.Handler;
		}

		var success = handler.TryGetData(path, out var data);
		if (!success || data == null) return null;
		token.ThrowIfCancellationRequested();

		return data;
	}
}
