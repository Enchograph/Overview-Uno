using System;
using Microsoft.Extensions.Logging;

namespace Overview.Server.Infrastructure.Diagnostics;

public sealed class MicrosoftOverviewLoggerFactory : IOverviewLoggerFactory
{
    private readonly ILoggerFactory _loggerFactory;

    public MicrosoftOverviewLoggerFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public IOverviewLogger CreateLogger<TCategory>()
    {
        return new MicrosoftOverviewLogger(_loggerFactory.CreateLogger<TCategory>());
    }

    public IOverviewLogger CreateLogger(string categoryName)
    {
        return new MicrosoftOverviewLogger(_loggerFactory.CreateLogger(categoryName));
    }

    private sealed class MicrosoftOverviewLogger : IOverviewLogger
    {
        private readonly ILogger _logger;

        public MicrosoftOverviewLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void LogDebug(string message, params object?[] arguments)
        {
            _logger.LogDebug(message, arguments);
        }

        public void LogInformation(string message, params object?[] arguments)
        {
            _logger.LogInformation(message, arguments);
        }

        public void LogWarning(string message, params object?[] arguments)
        {
            _logger.LogWarning(message, arguments);
        }

        public void LogError(Exception? exception, string message, params object?[] arguments)
        {
            _logger.LogError(exception, message, arguments);
        }
    }
}
