using Application.Services.Time;
using Infrastructure;
using Infrastructure.Adapters.Time;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntegrationTests;

public class InfrastructureRegistrationTests
{
    [Fact]
    public void InfrastructureServices_ShouldRegisterSystemClock()
    {
        ServiceCollection services = new();
        services.AddInfrastructureServices();

        using ServiceProvider provider = services.BuildServiceProvider();
        IClock clock = provider.GetRequiredService<IClock>();

        Assert.IsType<SystemClock>(clock);
    }
}
