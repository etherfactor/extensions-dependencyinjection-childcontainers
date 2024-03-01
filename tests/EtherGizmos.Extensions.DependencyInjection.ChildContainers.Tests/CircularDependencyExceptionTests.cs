namespace EtherGizmos.Extensions.DependencyInjection.ChildContainers.Tests;

internal class CircularDependencyExceptionTests
{
    [Test]
    public void Constructor_SpecifiesDependencies_CanGet()
    {
        //Arrange
        var exception = new CircularDependencyException(new Type[] { typeof(object), typeof(string), typeof(object) });

        //Act

        //Assert
        Assert.That(exception.DependencyChain, Is.Not.Null);
        Assert.That(exception.DependencyChain.Count(), Is.EqualTo(3));
    }
}
