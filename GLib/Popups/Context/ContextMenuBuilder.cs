namespace GLib.Popups.Context; 

/// <summary>
/// Builder class for constructing a <see cref="ContextMenu"/> instance.
/// </summary>
/// <typeparam name=""></typeparam>
public sealed class ContextMenuBuilder : ContextMenuNodeBuilder<ContextMenu, ContextMenuBuilder> {
	/// <summary>
	/// Builds the <see cref="ContextMenu"/>.
	/// </summary>
	/// <returns>you'll never guess</returns>
	public override ContextMenu Build() => new(this.GetNodes());
}
