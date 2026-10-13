using Domain.Entities;
using Xunit;

namespace Domain.UnitTests;

public class AssemblySmokeTests
{
    [Fact]
    public void DomainAssembly_ShouldContainDomainEntities()
    {
        Type[] entityTypes = typeof(User).Assembly
            .GetTypes()
            .Where(type => type.Namespace == typeof(User).Namespace)
            .ToArray();

        Assert.NotEmpty(entityTypes);
    }
}
