using TimeTracker.Helpers;
using TimeTracker.Models;

namespace TimeTracker.ViewModels;

public class AddEditTaskViewModel : ViewModelBase
{
    public string Title { get; }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string _description = string.Empty;
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    // Number fields are kept as text so the user can type freely; parsed in Validate/ApplyTo
    private string _estimatedHours = string.Empty;
    public string EstimatedHours
    {
        get => _estimatedHours;
        set => SetProperty(ref _estimatedHours, value);
    }

    private string _hourlyRate = string.Empty;
    public string HourlyRate
    {
        get => _hourlyRate;
        set => SetProperty(ref _hourlyRate, value);
    }

    private bool _isBillable = true;
    public bool IsBillable
    {
        get => _isBillable;
        set => SetProperty(ref _isBillable, value);
    }

    private bool _isProductive = true;
    public bool IsProductive
    {
        get => _isProductive;
        set => SetProperty(ref _isProductive, value);
    }

    private bool _isClosed;
    public bool IsClosed
    {
        get => _isClosed;
        set => SetProperty(ref _isClosed, value);
    }

    // Pass null to add a new task, or an existing task to edit it
    public AddEditTaskViewModel(ProjectTask? task = null)
    {
        Title = task is null ? "New Task" : "Edit Task";
        if (task is null) return;

        _name           = task.Name;
        _description    = task.Description;
        _estimatedHours = NumberInput.Format(task.EstimatedHours);
        _hourlyRate     = NumberInput.Format(task.HourlyRate);
        _isBillable     = task.IsBillable;
        _isProductive   = task.IsProductive;
        _isClosed       = !task.IsActive;
    }

    // Returns null when the input is valid, or an error message the dialog can display
    public string? Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            return "Enter a name for the task.";
        if (!NumberInput.TryParse(EstimatedHours, out double _))
            return "Enter a valid number of estimated hours, or leave it empty.";
        if (!NumberInput.TryParse(HourlyRate, out decimal _))
            return "Enter a valid hourly rate, or leave it empty.";
        return null;
    }

    // Copies the dialog values onto the task; call only after Validate() returned null
    public void ApplyTo(ProjectTask task)
    {
        NumberInput.TryParse(EstimatedHours, out double estimated);
        NumberInput.TryParse(HourlyRate, out decimal rate);

        task.Name           = Name.Trim();
        task.Description    = Description.Trim();
        task.EstimatedHours = estimated;
        task.HourlyRate     = rate;
        task.IsBillable     = IsBillable;
        task.IsProductive   = IsProductive;
        task.IsActive       = !IsClosed;
    }
}
