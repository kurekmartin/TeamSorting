using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;

namespace TeamSorting.Controls;

internal sealed class InputTreeDataGridElementFactory : TreeDataGridElementFactory
{
    protected override Control CreateElement(object? data)
    {
        return data is TemplateCell ? new NavigableTreeDataGridTemplateCell() : base.CreateElement(data);
    }

    protected override string GetDataRecycleKey(object? data)
    {
        return data is TemplateCell
            ? typeof(NavigableTreeDataGridTemplateCell).FullName!
            : base.GetDataRecycleKey(data);
    }
}

internal sealed class NavigableTreeDataGridTemplateCell : TreeDataGridTemplateCell
{
    protected override Type StyleKeyOverride => typeof(TreeDataGridTemplateCell);

    public void StartEdit()
    {
        BeginEdit();
    }

    public void CommitEdit()
    {
        EndEdit();
    }
}
