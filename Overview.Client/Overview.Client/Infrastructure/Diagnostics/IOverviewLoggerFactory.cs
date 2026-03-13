namespace Overview.Client.Infrastructure.Diagnostics;

public interface IOverviewLoggerFactory
{
    IOverviewLogger CreateLogger<TCategory>();

    IOverviewLogger CreateLogger(string categoryName);
}
