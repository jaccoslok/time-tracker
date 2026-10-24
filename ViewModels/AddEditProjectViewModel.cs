using TimeTracker.Helpers;
using TimeTracker.Models;

namespace TimeTracker.ViewModels;

public class AddEditProjectViewModel : ViewModelBase
{
    public string Title { get; }

    private string _code = string.Empty;
    public string Code
    {
        get => _code;
        set => SetProperty(ref _code, value);
    }

    private string _customer = string.Empty;
    public string Customer
    {
        get => _customer;
        set => SetProperty(ref _customer, value);
    }

    private string _description = string.Empty;
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    // Kept as text so the user can type freely; parsed in Validate/ApplyTo
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

    // Pass null to add a new project, or an existing project to edit it
    public AddEditProjectViewModel(Project? project = null)
    {
        Title = project is null ? "New Project" : "Edit Project";
        if (project is null) return;

        _code         = project.Code;
        _customer     = project.Customer;
        _description  = project.Description;
        _hourlyRate   = NumberInput.Format(project.HourlyRate);
        _isBillable   = project.IsBillable;
        _isProductive = project.IsProductive;
        _isClosed     = !project.IsActive;
    }

    // Returns null when the input is valid, or an error message the dialog can display
    public string? Validate()
    {
        if (string.IsNullOrWhiteSpace(Code) && string.IsNullOrWhiteSpace(Customer))
            return "Enter a code or a customer name for the project.";
        if (!NumberInput.TryParse(HourlyRate, out decimal _))
            return "Enter a valid hourly rate, or leave it empty.";
        return null;
    }

    // Copies the dialog values onto the project; call only after Validate() returned null
    public void ApplyTo(Project project)
    {
        NumberInput.TryParse(HourlyRate, out decimal rate);

        project.Code         = Code.Trim();
        project.Customer     = Customer.Trim();
        project.Description  = Description.Trim();
        project.HourlyRate   = rate;
        project.IsBillable   = IsBillable;
        project.IsProductive = IsProductive;
        project.IsActive     = !IsClosed;
    }
}
