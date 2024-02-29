using Microsoft.Extensions.Logging;
using System;

namespace EtherGizmos.Extensions.DependencyInjection.Internal;

/// <summary>
/// Forwards logging to the parent container using the parent container's <see cref="ILoggerFactory"/>.
/// </summary>
/// <typeparam name="T">The type of logger.</typeparam>
internal class LoggerForward<T> : ILogger<T>
{
    private readonly ILogger _logger;

    public LoggerForward(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<T>();
    }

    /// <inheritdoc/>
    public IDisposable BeginScope<TState>(TState state)
    {
        return _logger.BeginScope(state);
    }

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel)
    {
        return _logger.IsEnabled(logLevel);
    }

    /// <inheritdoc/>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _logger.Log(logLevel, eventId, state, exception, formatter);
    }
}