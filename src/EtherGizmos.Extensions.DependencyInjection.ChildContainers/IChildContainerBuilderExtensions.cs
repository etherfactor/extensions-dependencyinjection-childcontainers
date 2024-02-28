using EtherGizmos.Extensions.DependenyInjection.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EtherGizmos.Extensions.DependenyInjection;

/// <summary>
/// Provides extension methods for <see cref="IChildContainerBuilder"/>.
/// </summary>
public static class IChildContainerBuilderExtensions
{
    /// <summary>
    /// Imports the parent container's logging into the child container, so references to <see cref="ILoggerFactory"/> and
    /// <see cref="ILogger{TCategoryName}"/> resolve to the parent's loggers.
    /// </summary>
    /// <param name="this">Itself.</param>
    /// <returns>Itself.</returns>
    public static IChildContainerBuilder ImportLogging(this IChildContainerBuilder @this)
    {
        @this.ImportSingleton<ILoggerFactory>();
        @this.ChildServices.AddSingleton(typeof(ILogger<>), typeof(LoggerForward<>));

        return @this;
    }
}
