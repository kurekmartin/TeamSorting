using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;

namespace TeamSorting.Models;

public class ValidatableObservableObject : ObservableObject, INotifyDataErrorInfo
{
    private readonly Dictionary<string, List<string>> _errors = new();
    private IValidator? _validator;

    public bool HasErrors => _errors.Count > 0;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    protected void SetValidator(IValidator validator)
    {
        _validator = validator;
    }

    protected void ValidateProperty(object? value, [CallerMemberName] string? propertyName = null)
    {
        if (_validator == null || string.IsNullOrEmpty(propertyName))
            return;

        var context = new ValidationContext<object>(this,
            new PropertyChain(),
            new MemberNameValidatorSelector([propertyName]));

        ValidationResult? validationResult = _validator.Validate(context);

        ClearErrors(propertyName);

        List<string> errors = validationResult.Errors
                                              .Where(e => e.PropertyName == propertyName)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();

        if (errors.Count > 0)
        {
            _errors[propertyName] = errors;
        }

        OnErrorsChanged(propertyName);
    }

    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return _errors.Values.SelectMany(e => e);
        }

        if (_errors.TryGetValue(propertyName, out List<string>? errors))
        {
            return errors;
        }

        return Enumerable.Empty<string>();
    }

    public void AddError(string propertyName, string error)
    {
        if (!_errors.TryGetValue(propertyName, out List<string>? value))
        {
            value = [];
            _errors[propertyName] = value;
        }

        if (value.Contains(error))
        {
            return;
        }

        value.Add(error);
        OnErrorsChanged(propertyName);
    }

    public void ClearErrors(string propertyName)
    {
        if (_errors.Remove(propertyName))
        {
            OnErrorsChanged(propertyName);
        }
    }

    public void ClearAllErrors()
    {
        List<string> propertyNames = _errors.Keys.ToList();
        _errors.Clear();
        foreach (var propertyName in propertyNames)
        {
            OnErrorsChanged(propertyName);
        }
    }

    private void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        OnPropertyChanged(nameof(HasErrors));
    }
}