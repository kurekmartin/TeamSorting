using TeamSorting.Models;

namespace TeamSorting.ViewModels;

public class CsvErrorViewModel(List<CsvError> errors)
{
    public List<CsvError> Errors { get; set; } = errors;

    public List<CsvError> OrderedErrors =>
        Errors.OrderBy(error => error.Row).ThenBy(error => error.Column).ToList();
}