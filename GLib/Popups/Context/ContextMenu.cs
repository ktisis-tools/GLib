using ImGuiNET;

namespace GLib.Popups.Context; 

public class ContextMenu {
	private readonly NodeContainer _nodes = new();

	public bool IsOpen { get; set; }
	
	/// <summary>
	/// Constructs a new instance of the <see cref="ContextMenu"/> class.
	/// </summary>
	public ContextMenu() { }

	public ContextMenu(IEnumerable<IContextMenuNode> nodes)
		=> this._nodes.AddRange(nodes);

	public ContextMenu AddNode(IContextMenuNode node) {
		this._nodes.Add(node);
		return this;
	}

	public ContextMenu AddNodes(IEnumerable<IContextMenuNode> nodes) {
		this._nodes.AddRange(nodes);
		return this;
	}

	public bool Draw() {
		this._nodes.Draw();
		return true;
	}
	
	// Node implementations

	private class NodeContainer : List<IContextMenuNode>, IContextMenuNode {
		public void Draw() => this.ForEach(node => node.Draw());
	}

	internal class NodeGroup : IContextMenuNode {
		private readonly NodeContainer Nodes = new();

		public NodeGroup(IEnumerable<IContextMenuNode> nodes) {
			this.Nodes.AddRange(nodes);
		}
		
		public virtual void Draw() => this.Nodes.Draw();
	}

	internal class NodeSubMenu : NodeGroup {
		private readonly string Name;

		public NodeSubMenu(string name, IEnumerable<IContextMenuNode> nodes) : base(nodes) {
			this.Name = name;
		}
		
		public override void Draw() {
			if (!ImGui.BeginMenu(this.Name)) return;
			
			try {
				base.Draw();
			} finally {
				ImGui.EndMenu();
			}
		}
	}
	
	internal class ActionNode : IContextMenuNode {
		private readonly string Name;
		private readonly Action Handler;
		
		public string? Shortcut;

		public ActionNode(string name, Action handler) {
			this.Name = name;
			this.Handler = handler;
		}

		public void Draw() {
			var invoke = this.Shortcut switch {
				string => ImGui.MenuItem(this.Name, this.Shortcut),
				_ => ImGui.MenuItem(this.Name)
			};
			
			if (invoke) this.Handler.Invoke();
		}
	}

	internal class Separator : IContextMenuNode {
		public void Draw() => ImGui.Separator();
	}
}
