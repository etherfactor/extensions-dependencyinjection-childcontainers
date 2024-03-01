using EtherGizmos.Extensions.DependencyInjection.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherGizmos.Extensions.DependencyInjection.ChildContainers.Tests;

internal class IChildContainerBuilderExtensionsTests
{
    private IServiceCollection _serviceCollection;

    [SetUp]
    public void SetUp()
    {
        _serviceCollection = new ServiceCollection();
    }

    [Test]
    public void ImportLogging_WithParentLogger_ForwardsLogger()
    {
        //Arrange
        var logger = new TestLogger();
        _serviceCollection
            .AddSingleton<ILoggerFactory>(e => new TestLoggerFactory(logger))
            .AddChildContainer((childServices, parentServices) =>
            {
                childServices.AddTransient<Wrapper>();
            })
            .ImportLogging()
            .ForwardTransient<Wrapper>();

        //Act
        var provider = _serviceCollection.BuildServiceProvider();

        var factory = provider.GetRequiredService<ILoggerFactory>();
        var wrapper = provider.GetRequiredService<Wrapper>();

        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(wrapper, Is.Not.Null);
            Assert.That(wrapper.Logger, Is.Not.Null);
            Assert.That(wrapper.Logger.GetType(), Is.EqualTo(typeof(LoggerForward<Wrapper>)));
        });

        Assert.Multiple(() =>
        {
            Assert.That(factory, Is.Not.Null);
            Assert.That(factory.GetType(), Is.EqualTo(typeof(TestLoggerFactory)));
        });

        Assert.That(logger.IsLogged, Is.False);
        wrapper.Logger.Log(LogLevel.Information, "");
        Assert.That(logger.IsLogged, Is.True);
    }

    private class TestLoggerFactory : ILoggerFactory
    {
        public ILogger Logger { get; }

        public TestLoggerFactory(ILogger logger)
        {
            Logger = logger;
        }

        public void AddProvider(ILoggerProvider provider)
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return Logger;
        }

        public void Dispose()
        {
        }
    }

    private class TestLogger : ILogger
    {
        public bool IsLogged { get; private set; } = false;

        public IDisposable BeginScope<TState>(TState state)
        {
            return null!;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            IsLogged = true;
        }
    }

    private class Wrapper
    {
        public ILogger Logger { get; }

        public Wrapper(ILogger<Wrapper> logger)
        {
            Logger = logger;
        }
    }
}
