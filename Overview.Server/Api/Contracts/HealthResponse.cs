namespace Overview.Server.Api.Contracts;

public sealed class HealthResponse
{
    public required string Status { get; init; }

    public required string Service { get; init; }
}
