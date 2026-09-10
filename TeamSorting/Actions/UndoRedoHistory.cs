namespace TeamSorting.Actions;

public sealed class UndoRedoHistory
{
    private const int DefaultCapacity = 100;
    private readonly int _capacity;
    private readonly List<IUndoableAction> _undoStack = [];
    private readonly List<IUndoableAction> _redoStack = [];

    public UndoRedoHistory() : this(DefaultCapacity)
    {
    }

    public UndoRedoHistory(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        _capacity = capacity;
    }

    public event EventHandler? StateChanged;

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    public bool Execute(IUndoableAction action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (!action.Execute())
        {
            return false;
        }

        _undoStack.Add(action);
        if (_undoStack.Count > _capacity)
        {
            _undoStack.RemoveAt(0);
        }

        _redoStack.Clear();
        OnStateChanged();
        return true;
    }

    public bool Undo()
    {
        if (!CanUndo)
        {
            return false;
        }

        IUndoableAction action = _undoStack[^1];
        if (!action.Undo())
        {
            return false;
        }

        _undoStack.RemoveAt(_undoStack.Count - 1);
        _redoStack.Add(action);
        OnStateChanged();
        return true;
    }

    public bool Redo()
    {
        if (!CanRedo)
        {
            return false;
        }

        IUndoableAction action = _redoStack[^1];
        if (!action.Execute())
        {
            return false;
        }

        _redoStack.RemoveAt(_redoStack.Count - 1);
        _undoStack.Add(action);
        OnStateChanged();
        return true;
    }

    public void Clear()
    {
        if (!CanUndo && !CanRedo)
        {
            return;
        }

        _undoStack.Clear();
        _redoStack.Clear();
        OnStateChanged();
    }

    private void OnStateChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }
}
