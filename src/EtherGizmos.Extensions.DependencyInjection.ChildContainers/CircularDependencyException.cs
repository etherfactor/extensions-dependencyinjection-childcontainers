using EtherGizmos.Extensions.DependenyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace EtherGizmos.Extensions.DependenyInjection;

/// <summary>
/// Thrown when a circular reference is found while resolving services from child containers during runtime.
/// </summary>
public class CircularDependencyException : Exception
{
    private readonly IEnumerable<Type> _dependencyChain;

    /// <summary>
    /// The failing dependency chain.
    /// </summary>
    public IEnumerable<Type> DependencyChain => _dependencyChain;

    internal CircularDependencyException(Type failingDependency) : base(GenerateMessage(failingDependency.Yield()))
    {
        _dependencyChain = failingDependency.Yield();
    }

    private CircularDependencyException(IEnumerable<Type> dependencyChain, CircularDependencyException innerException) : base(GenerateMessage(dependencyChain), innerException)
    {
        _dependencyChain = dependencyChain;
    }

    private static string GenerateMessage(IEnumerable<Type> dependencyChain)
    {
        //The chain is complete if there are at least 2 elements that complete a cycle
        if (dependencyChain.Skip(1).Any() && dependencyChain.First() == dependencyChain.Last())
        {
            return "Encountered a circular dependency. Service chain resolved as follows:" + Environment.NewLine +
                string.Join(" -> ", dependencyChain);
        }
        else
        {
            return "Dependency chain unwrapping in progress... it is advised not to catch these exceptions. Tail of circular dependency chain is as follows:" + Environment.NewLine +
                string.Join(" -> ", dependencyChain);
        }
    }

    [DoesNotReturn]
    internal void PrependAndThrow(Type failingParentDependency)
    {
        throw new CircularDependencyException(_dependencyChain.Prepend(failingParentDependency), this);
    }
}
