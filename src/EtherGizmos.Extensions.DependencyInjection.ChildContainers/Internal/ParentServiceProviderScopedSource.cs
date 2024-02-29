using System;

namespace EtherGizmos.Extensions.DependencyInjection.Internal;

/// <summary>
/// Contains a reference to the current scoped parent service provider.
/// </summary>
internal class ParentServiceProviderScopedSource
{
    private IServiceProvider? _parentProvider;

    /// <summary>
    /// The scoped parent service provider.
    /// </summary>
    public IServiceProvider ParentProvider => _parentProvider
        ?? throw new InvalidOperationException("No parent provider was specified");

    public void SetProvider(IServiceProvider parentProvider)
    {
        _parentProvider = parentProvider;
    }
}
