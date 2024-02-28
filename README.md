# Child Containers for .NET Dependency Injection

This package offers a solution for managing dependencies in .NET applications utilizing the standard Microsoft-provided Dependency Injection. Introducing the concept of child containers, it enables resolving services within the parent container after other parent services have been configured. This seamlessly allows for the utilization of singleton or transient dependencies such as IOptions, even when the target service lacks direct access to an IServiceProvider in its configuration methods.

With this package, you can efficiently manage dependencies without the need to construct services before the service container is built in order to configure other services. This approach fosters cleaner and more organized dependency management in your .NET applications.

### Example Usage

For applications utilizing frameworks like MassTransit, which might not directly support pulling services from an IServiceProvider during configuration, this package offers seamless integration with services like IOptions. By adding MassTransit to a child container, which maintains its own set of dependencies resolved at runtime, you can ensure that services such as IOptions are readily available when configuring MassTransit within the child container, without requiring direct access to an IServiceProvider from MassTransit.

This approach extends to other packages that may not inherently support pulling services from an IServiceProvider during configuration.

```csharp
builder.Services
    .AddOptions<UsageOptions>()
    .Configure<IConfiguration>((opt, conf) =>
    {
        var path = "Connections:Use";

        conf.GetSection(path)
            .Bind(opt);

        opt.AssertValid(path);
    });

builder.Services
    .AddChildContainer((childServices, parentServices) =>
    {
        var usageOptions = parentServices
            .GetRequiredService<IOptions<UsageOptions>>()
            .Value;

        childServices.AddMassTransit(opt =>
        {
            // Configuration based on UsageOptions
            if (usageOptions.MessageBroker == MessageBrokerType.InMemory)
            {
                opt.UsingInMemory((context, conf) =>
                {
                    // In-memory configuration
                });
            }
            else if (usageOptions.MessageBroker == MessageBrokerType.RabbitMQ)
            {
                var rabbitMQOptions = parentServices
                    .GetRequiredService<IOptions<RabbitMQOptions>>()
                    .Value;

                opt.UsingRabbitMq((context, conf) =>
                {
                    // RabbitMQ configuration based on RabbitMQOptions
                    // ...
                });
            }
            else
            {
                throw new InvalidOperationException($"Unknown message broker type: {usageOptions.MessageBroker}");
            }
        });
    })
    .ImportLogging() // Import logging from parent container
    .ForwardScoped<ISendEndpointProvider>(); // Forward ISendEndpointProvider to parent container
```

After building the service provider, you can resolve the `ISendEndpointProvider` by injecting `ISendEndpointProvider` into any service within the application or by directly calling `services.GetRequiredService<ISendEndpointProvider>()` from the parent provider.
