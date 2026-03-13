namespace Overview.Server.Infrastructure.Diagnostics;

public interface IOverviewLoggerFactory
{
    IOverviewLogger CreateLogger<TCategory>();

    IOverviewLogger CreateLogger(string categoryName);
}
