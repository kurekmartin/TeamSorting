using FluentValidation;
using TeamSorting.Models;

namespace TeamSorting.Validators;

public class TeamValidator : AbstractValidator<Team>
{
    public TeamValidator()
    {
        RuleFor(team => team.Name)
            .NotEmpty();
    }
}