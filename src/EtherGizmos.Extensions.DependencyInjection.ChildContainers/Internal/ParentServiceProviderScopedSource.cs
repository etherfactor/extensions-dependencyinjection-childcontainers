using System;

namespace EtherGizmos.Extensions.DependenyInjection.Internal;

internal class ParentServiceProviderScopedSource
{
    private IServiceProvider? _parentProvider;

    public IServiceProvider ParentProvider => _parentProvider
        ?? throw new InvalidOperationException("No parent provider was specified");

    public void SetProvider(IServiceProvider parentProvider)
    {
        _parentProvider = parentProvider;
    }
}
