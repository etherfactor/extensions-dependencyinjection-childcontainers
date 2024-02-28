using System.Collections.Generic;

namespace EtherGizmos.Extensions.DependenyInjection.Extensions;

internal static class IEnumerableExtensions
{
    /// <summary>
    /// Converts an object to an <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="TObject">The object type.</typeparam>
    /// <param name="this">Itself.</param>
    /// <returns>Itself, as an enumerable.</returns>
    internal static IEnumerable<TObject> Yield<TObject>(this TObject @this)
    {
        yield return @this;
    }
}
