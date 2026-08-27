namespace TeamSorting.Models;

public sealed class DisciplineRecordChangedEventArgs(DisciplineRecord record) : EventArgs
{
    public DisciplineRecord Record { get; } = record;
}
