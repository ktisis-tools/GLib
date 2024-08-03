using System.Collections.Immutable;

namespace GLib.Popups; 

/// <summary>
/// An interface that manages <see cref="IPopup"/> instances.
/// Automatically culls popups after closing.
/// </summary>
public class PopupManager {
	private readonly List<IPopup> Popups = new();
	
	/// <summary>
	/// Constructs a new instance of the <see cref="PopupManager"/> class.
	/// </summary>
	public PopupManager() {
		
	}
	
	/// <summary>
	/// Draws all visible popups.
	/// </summary>
	public void Draw() {
		foreach (var popup in this.GetPopups())
			popup.Draw();
	}

	private IEnumerable<IPopup> GetPopups() {
		foreach (var popup in this.Popups.ToImmutableList()) {
			if (!popup.IsOpen) {
				this.Popups.Remove(popup);
				continue;
			}
			yield return popup;
		}
	}
	
	/// <summary>
	/// Attaches an <see cref="IPopup"/> implementation to display.
	/// </summary>
	/// <param name="popup">The <see cref="IPopup"/> implementation to attach.</param>
	/// <returns>Self-referential for fluent method chaining.</returns>
	public PopupManager Add(IPopup popup) {
		this.Popups.Add(popup);
		return this;
	}

	/// <summary>
	/// TODO: Document
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public T? Get<T>() where T : class, IPopup => (T?)this.Popups.FirstOrDefault(popup => popup is T);

	/// <summary>
	/// Gets all popups of a given type.
	/// </summary>
	/// <returns></returns>
	public IEnumerable<T> GetAll<T>() where T : class, IPopup => this.Popups.Where(popup => popup is T).Cast<T>();

	/// <summary>
	/// Removes an <see cref="IPopup"/> implementation.
	/// </summary>
	/// <param name="popup">The <see cref="IPopup"/> implementation to remove.</param>
	/// <returns>The return value of <see cref="List{T}.Remove"/>, indicating whether the item was successfully removed.</returns>
	public bool Remove(IPopup popup)
		=> this.Popups.Remove(popup);
}
