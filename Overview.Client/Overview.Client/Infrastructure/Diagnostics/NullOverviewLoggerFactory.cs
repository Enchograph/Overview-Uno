using System;

namespace Overview.Client.Infrastructure.Diagnostics;

public sealed class NullOverviewLoggerFactory : IOverviewLoggerFactory
{
    public static NullOverviewLoggerFactory Instance { get; } = new();

    private NullOverviewLoggerFactory()
    {
    }

    public IOverviewLogger CreateLogger<TCategory>()
    {
        return NullOverviewLogger.Instance;
    }

    public IOverviewLogger CreateLogger(string categoryName)
    {
        return NullOverviewLogger.Instance;
    }

    private sealed class NullOverviewLogger : IOverviewLogger
    {
        public static NullOverviewLogger Instance { get; } = new();

        public void LogDebug(string message, params object?[] arguments)
        {
        }

        public void LogInformation(string message, params object?[] arguments)
        {
        }

        public void LogWarning(string message, params object?[] arguments)
        {
        }

        public void LogError(Exception? exception, string message, params object?[] arguments)
        {
        }
    }
}
