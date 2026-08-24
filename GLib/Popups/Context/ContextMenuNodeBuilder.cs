namespace GLib.Popups.Context;

/// <summary>
/// TODO
/// </summary>
public abstract class ContextMenuNodeBuilder<T, TBuilder>
	where T : ContextMenu
	where TBuilder : ContextMenuNodeBuilder<T, TBuilder>
{
	private readonly List<IContextMenuNode> Nodes = new();

	/// <summary>
	/// Constructs a new instance of <see cref="TBuilder"/> for building node subgroups.
	/// </summary>
	/// <returns>The newly constructed <see cref="TBuilder"/> instance.</returns>
	protected abstract TBuilder Builder();
	
	protected IEnumerable<IContextMenuNode> GetNodes()
		=> this.Nodes.AsReadOnly();
	
	public TBuilder AddNode(IContextMenuNode node) {
		this.Nodes.Add(node);
		return (TBuilder)this;
	}
	
	public TBuilder Action(string name, Action handler)
		=> this.AddNode(new ContextMenu.ActionNode(name, handler));
	
	public TBuilder Group(Action<TBuilder> handler) {
		var builder = this.Builder();
		handler.Invoke(builder);
		return this.AddNode(new ContextMenu.NodeGroup(builder.GetNodes()));
	}
	
	public TBuilder SubMenu(string name, Action<TBuilder> handler) {
		var builder = this.Builder();
		handler.Invoke(builder);
		return this.AddNode(new ContextMenu.NodeSubMenu(name, builder.GetNodes()));
	}

	public TBuilder Separator()
		=> this.AddNode(new ContextMenu.Separator());

	public TBuilder WithDisabled(Func<bool> condition) {
		var node = this.Nodes[^1];
		if (node == null) throw new NullReferenceException("Nodes has not been populated");
		this.Nodes[^1] = new ContextMenu.Disabled(node, condition);
		return (TBuilder)this;
	}
	
	public abstract T Build(string id);
}
