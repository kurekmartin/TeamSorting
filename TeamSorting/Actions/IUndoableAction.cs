namespace TeamSorting.Actions;

public interface IUndoableAction
{
    bool Execute();
    bool Undo();
}
