﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace EtherGizmos.Extensions.DependencyInjection.Internal;

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

    /// <summary>
    /// Attempts to add the child container to the parent container.
    /// </summary>
    /// <param name="id">The id of the child container.</param>
    /// <param name="childServices">The child service registrations.</param>
    /// <param name="configureChild">The action to configure the child services.</param>
    /// <param name="imports">Any additional imports from the parent.</param>
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
                childServices.AddSingleton<ParentServiceProviderSingletonSource>(services =>
                {
                    var service = new ParentServiceProviderSingletonSource();
                    service.SetProvider(_parentRootProvider);

                    return service;
                });
                childServices.AddScoped<ParentServiceProviderScopedSource>();

                configureChild(childServices, _parentRootProvider);

                foreach (var import in imports)
                {
                    ServiceDescriptor descriptor;
                    if (import.Lifetime == ServiceLifetime.Scoped)
                    {
                        //Scoped imports need to be pulled from a scoped parent context (note 2nd line)
                        descriptor = ServiceDescriptor.Describe(
                            import.ServiceType,
                            childProvider =>
                            {
                                var source = childProvider.GetRequiredService<ParentServiceProviderScopedSource>();

                                var provider = source.ParentProvider;
                                if (provider is null)
                                {
                                    //Since we are the ones making a child scope in a parent scope, this will only be needed
                                    //if a child scope is created outside a parent context. Since no parent scope is
                                    //associated, one will need to be created
                                    provider = childProvider.GetRequiredService<ParentServiceProviderSingletonSource>().ParentProvider
                                        .CreateScope().ServiceProvider;

                                    source.SetProvider(provider);
                                }

                                var service = provider.GetRequiredService(import.ServiceType);

                                return service;
                            },
                            import.Lifetime);
                    }
                    else
                    {
                        //Singleton and transient imports can be pulled from the root parent context (note 2nd line)
                        descriptor = ServiceDescriptor.Describe(
                            import.ServiceType,
                            childProvider =>
                            {
                                var source = childProvider.GetRequiredService<ParentServiceProviderSingletonSource>();

                                var provider = source.ParentProvider;

                                var service = provider.GetRequiredService(import.ServiceType);

                                return service;
                            },
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

        //Associate the scoped parent provider for singleton/transient services
        var parentProviderSingletonSource = scope.GetRequiredService<ParentServiceProviderSingletonSource>();
        parentProviderSingletonSource.SetProvider(parentProvider);

        //Associate the scoped parent provider for scoped services
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

        //Associate the root parent provider for singleton/transient services
        var parentProviderSingletonSource = scope.GetRequiredService<ParentServiceProviderSingletonSource>();
        parentProviderSingletonSource.SetProvider(parentProvider);

        return scope;
    }
}
