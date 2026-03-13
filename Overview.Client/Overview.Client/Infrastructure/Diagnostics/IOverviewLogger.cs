using System;

namespace Overview.Client.Infrastructure.Diagnostics;

public interface IOverviewLogger
{
    void LogDebug(string message, params object?[] arguments);

    void LogInformation(string message, params object?[] arguments);

    void LogWarning(string message, params object?[] arguments);

    void LogError(Exception? exception, string message, params object?[] arguments);
}
