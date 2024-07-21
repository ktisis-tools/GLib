namespace GLib.Popups.Decorators;

/// <summary>
/// An interface that provides filtering options for UI components, e.g. <see cref="PopupList{T}"/>.
/// </summary>
/// <typeparam name="T">The type representing each item being filtered.</typeparam>
public interface IFilterProvider<in T> {
	/// <summary>
	/// A predicate used by UI components to test whether an item passes filtering.
	/// </summary>
	/// <param name="item">The item to compare.</param>
	/// <returns>A boolean indicating whether this item passes filtering.</returns>
	public bool Filter(T item);
	
	/// <summary>
	/// A method called by UI components to draw filtering options.
	/// </summary>
	/// <returns>A boolean indicating whether filtering options were updated in this call.</returns>
	public bool DrawOptions() => false;
}
