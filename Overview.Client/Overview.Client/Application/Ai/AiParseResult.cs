using Overview.Client.Domain.Enums;

namespace Overview.Client.Application.Ai;

public sealed record AiParseResult
{
    public AiStructuredResponse? Response { get; init; }

    public IReadOnlyList<string> ValidationErrors { get; init; } = Array.Empty<string>();

    public bool CanApplyWriteOperation =>
        Response is not null &&
        ValidationErrors.Count == 0 &&
        Response.Confidence >= 0.85 &&
        Response.Intent is AiRequestType.CreateItem or AiRequestType.DeleteItem;
}
