using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace EtherGizmos.Extensions.DependenyInjection.Internal;

/// <summary>
/// Produces child service providers.
/// </summary>
internal class ChildServiceProviderFactory
{
    private readonly IServiceProvider _parentRootProvider;
    private readonly ConcurrentDictionary<Guid, IServiceProvider> _childProviders = new();

    public ChildServiceProviderFactory(
        IServiceProvider parentRootProvider)
    {
        _parentRootProvider = parentRootProvider;
    }

    public void TryAddServiceCollection(
        Guid id,
        IServiceCollection childServices,
        Action<IServiceCollection, IServiceProvider> configureChild,
        List<(Type ServiceType, ServiceLifetime Lifetime)> imports)
    {
        //Attempt to build and add the provider
        _childProviders.AddOrUpdate(
            id,
            id =>
            {
                childServices.AddSingleton<ParentServiceProviderSingletonSource>();
                childServices.AddScoped<ParentServiceProviderScopedSource>();

                configureChild(childServices, _parentRootProvider);

                foreach (var import in imports)
                {
                    ServiceDescriptor descriptor;
                    if (import.Lifetime == ServiceLifetime.Scoped)
                    {
                        descriptor = ServiceDescriptor.Describe(import.ServiceType, childProvider =>
                            childProvider.GetRequiredService<ParentServiceProviderScopedSource>()
                                .ParentProvider
                                .GetRequiredService(import.ServiceType),
                            import.Lifetime);
                    }
                    else
                    {
                        descriptor = ServiceDescriptor.Describe(import.ServiceType, childProvider =>
                            childProvider.GetRequiredService<ParentServiceProviderSingletonSource>()
                                .ParentProvider
                                .GetRequiredService(import.ServiceType),
                            import.Lifetime);
                    }
                    childServices.Add(descriptor);
                }

                return childServices.BuildServiceProvider();
            },
            (_, old) => old);
    }

    /// <summary>
    /// Produces a scoped service provider.
    /// </summary>
    /// <returns></returns>
    public IServiceProvider GetScopedServiceProvider(
        Guid id,
        IServiceProvider parentProvider)
    {
        var scope = _childProviders[id]
            .CreateScope()
            .ServiceProvider;

        var parentProviderSingletonSource = scope.GetRequiredService<ParentServiceProviderSingletonSource>();
        parentProviderSingletonSource.SetProvider(parentProvider);

        var parentProviderScopedSource = scope.GetRequiredService<ParentServiceProviderScopedSource>();
        parentProviderScopedSource.SetProvider(parentProvider);

        return scope;
    }

    /// <summary>
    /// Produces a scoped service provider.
    /// </summary>
    /// <returns></returns>
    public IServiceProvider GetSingletonServiceProvider(
        Guid id,
        IServiceProvider parentProvider)
    {
        var scope = _childProviders[id];

        var parentProviderSingletonSource = scope.GetRequiredService<ParentServiceProviderSingletonSource>();
        parentProviderSingletonSource.SetProvider(parentProvider);

        return scope;
    }
}
