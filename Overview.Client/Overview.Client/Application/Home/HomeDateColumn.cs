namespace Overview.Client.Application.Home;

public sealed record HomeDateColumn
{
    public required DateOnly Date { get; init; }

    public required string HeaderLabel { get; init; }

    public bool IsToday { get; init; }
}
