using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence;
using Persistence.Contexts;
using Xunit;

namespace IntegrationTests;

public class PersistenceRegistrationTests
{
    [Fact]
    public void PersistenceServices_ShouldUseConfiguredSqlServerProvider()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:BaseDb"] =
                        "Server=localhost;Database=EventReservation;Trusted_Connection=True;TrustServerCertificate=True"
                }
            )
            .Build();
        ServiceCollection services = new();
        services.AddSingleton(configuration);
        services.AddPersistenceServices(configuration);

        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();
        BaseDbContext context = scope.ServiceProvider.GetRequiredService<BaseDbContext>();

        Assert.True(context.Database.IsSqlServer());
    }

    [Fact]
    public void PersistenceServices_WithoutConnectionString_ShouldFailFast()
    {
        ServiceCollection services = new();
        IConfiguration configuration = new ConfigurationBuilder().Build();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => services.AddPersistenceServices(configuration)
        );

        Assert.Equal("Connection string 'BaseDb' was not found.", exception.Message);
    }
}
