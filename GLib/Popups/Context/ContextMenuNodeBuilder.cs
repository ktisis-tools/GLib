namespace GLib.Popups.Context;

/// <summary>
/// TODO
/// </summary>
public abstract class ContextMenuNodeBuilder<T, TBuilder>
	where T : ContextMenu
	where TBuilder : ContextMenuNodeBuilder<T, TBuilder>, new()
{
	private readonly List<IContextMenuNode> Nodes = new();
	
	public TBuilder AddNode(IContextMenuNode node) {
		this.Nodes.Add(node);
		return (TBuilder)this;
	}
	
	public TBuilder Action(string name, Action handler)
		=> this.AddNode(new ContextMenu.ActionNode(name, handler));
	
	public TBuilder Group(Action<TBuilder> handler) {
		var builder = new TBuilder();
		handler.Invoke(builder);
		return this.AddNode(new ContextMenu.NodeGroup(builder.GetNodes()));
	}
	
	public TBuilder SubMenu(string name, Action<TBuilder> handler) {
		var builder = new TBuilder();
		handler.Invoke(builder);
		return this.AddNode(new ContextMenu.NodeSubMenu(name, builder.GetNodes()));
	}

	public TBuilder Separator()
		=> this.AddNode(new ContextMenu.Separator());

	protected IReadOnlyList<IContextMenuNode> GetNodes()
		=> this.Nodes.AsReadOnly();

	public abstract T Build();
}
