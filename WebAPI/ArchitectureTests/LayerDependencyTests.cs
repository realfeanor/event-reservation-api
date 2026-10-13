using NetArchTest.Rules;
using Xunit;

namespace ArchitectureTests;

public class LayerDependencyTests
{
    [Fact]
    public void Domain_ShouldNotDependOnOuterApplicationLayers()
    {
        TestResult result = Types.InAssembly(typeof(Domain.Entities.User).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("Application", "Infrastructure", "Persistence", "WebAPI")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Application_ShouldNotDependOnInfrastructureOrPresentation()
    {
        TestResult result = Types.InAssembly(typeof(Application.ApplicationServiceRegistration).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("Infrastructure", "Persistence", "WebAPI")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }
}
