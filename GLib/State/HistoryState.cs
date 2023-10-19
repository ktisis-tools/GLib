namespace GLib.State; 

/// <summary>
/// A class that tracks the state of a history timeline with a cursor.
/// </summary>
/// <typeparam name="T"></typeparam>
public class HistoryState<T> {
	private int Cursor;
	private readonly List<T> Timeline = new();

	/// <summary>
	/// The number of items currently in the timeline.
	/// </summary>
	public int Count => this.Timeline.Count;

	/// <summary>
	/// The value at the current cursor position. If the cursor is <value>0</value>, returns null.
	/// </summary>
	public T? Current => this.Cursor > 0 ? this.Timeline[this.Cursor - 1] : default;
	
	/// <summary>
	/// A boolean indicating whether the cursor position can be decremented.
	/// </summary>
	public bool CanUndo => this.Cursor > 0;

	/// <summary>
	/// A boolean indicating whether the cursor position can be incremented.
	/// </summary>
	public bool CanRedo => this.Cursor <= this.Timeline.Count - 1;

	/// <summary>
	/// A boolean indicating whether the previous cursor is positioned before or at the head of the timeline.
	/// </summary>
	public bool IsHeadPrevious => this.Cursor <= 1;

	/// <summary>
	/// Adds a new entry to the history timeline.
	/// </summary>
	/// <param name="entry">The entry to add.</param>
	public void Add(T entry) {
		var count = this.Timeline.Count;
		if (this.Cursor < count)
			this.Timeline.RemoveRange(this.Cursor, count - this.Cursor);
		this.Timeline.Add(entry);
		this.Cursor++;
	}

	/// <summary>
	/// Adds a new entry to the history timeline if it doesn't already exist.
	/// </summary>
	/// <param name="entry">The entry to add.</param>
	/// <returns>A value indicating whether the entry was added.</returns>
	public bool AddUnique(T entry) {
		var add = !this.Timeline.Contains(entry);
		if (add) this.Add(entry);
		return add;
	}

	/// <summary>
	/// Decrements the cursor position.
	/// </summary>
	/// <returns>The head prior to decrementing the cursor.</returns>
	public T? Previous() {
		if (!this.CanUndo) return default;
		var head = this.Current;
		this.Cursor--;
		return head;
	}

	/// <summary>
	/// Increments the cursor position.
	/// </summary>
	/// <returns>The head prior to incrementing the cursor.</returns>
	public T? Next() {
		if (!this.CanRedo) return default;
		var head = this.Current;
		this.Cursor++;
		return head;
	}

	/// <summary>
	/// Clears the history timeline, resetting the cursor to <value>0</value>.
	/// </summary>
	public void Clear() {
		this.Cursor = 0;
		this.Timeline.Clear();
	}

	/// <summary>
	/// Returns the current timeline as an <see cref="IReadOnlyList{T}"/>.
	/// </summary>
	public IReadOnlyList<T> GetReadOnly()
		=> this.Timeline.AsReadOnly();
}
