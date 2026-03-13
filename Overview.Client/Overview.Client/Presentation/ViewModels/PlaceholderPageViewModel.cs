namespace Overview.Client.Presentation.ViewModels;

public abstract class PlaceholderPageViewModel
{
    protected PlaceholderPageViewModel(string title, string description)
    {
        Title = title;
        Description = description;
    }

    public string Title { get; }

    public string Description { get; }
}
