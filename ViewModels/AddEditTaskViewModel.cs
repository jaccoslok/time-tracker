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

    public AddEditTaskViewModel(string title, string name = "", string description = "")
    {
        Title = title;
        _name = name;
        _description = description;
    }
}
