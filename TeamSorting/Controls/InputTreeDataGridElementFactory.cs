using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace TeamSorting.Controls;

internal sealed class InputTreeDataGridElementFactory(
    Action<NavigableTreeDataGridTemplateCell> editStarted) : TreeDataGridElementFactory
{
    protected override Control CreateElement(object? data)
    {
        return data is TemplateCell ? new NavigableTreeDataGridTemplateCell(editStarted) : base.CreateElement(data);
    }

    protected override string GetDataRecycleKey(object? data)
    {
        return data is TemplateCell
            ? typeof(NavigableTreeDataGridTemplateCell).FullName!
            : base.GetDataRecycleKey(data);
    }
}

internal sealed class NavigableTreeDataGridTemplateCell(
    Action<NavigableTreeDataGridTemplateCell> editStarted) : TreeDataGridTemplateCell
{
    protected override Type StyleKeyOverride => typeof(TreeDataGridTemplateCell);

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        bool wasEditing = IsEditing;
        base.OnPointerReleased(e);

        if (!wasEditing && IsEditing)
        {
            editStarted(this);
        }
    }

    public void StartEdit()
    {
        BeginEdit();
    }

    public void CommitEdit()
    {
        EndEdit();
    }
}
