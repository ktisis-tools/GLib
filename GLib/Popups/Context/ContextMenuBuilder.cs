namespace GLib.Popups.Context; 

/// <summary>
/// Builder class for constructing a <see cref="ContextMenu"/> instance.
/// </summary>
public sealed class ContextMenuBuilder : ContextMenuNodeBuilder<ContextMenu, ContextMenuBuilder> {
	/// <inheritdoc cref="ContextMenuNodeBuilder{T,TBuilder}.Builder"/>
	protected override ContextMenuBuilder Builder() => new();
	
	/// <summary>
	/// Builds the <see cref="ContextMenu"/>.
	/// </summary>
	/// <returns>you'll never guess</returns>
	public override ContextMenu Build(string id) => new(id, this.GetNodes());
}
