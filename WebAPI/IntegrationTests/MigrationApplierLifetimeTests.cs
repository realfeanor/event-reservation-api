using Core.Persistence.DbMigrationApplier;
using Core.Persistence.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntegrationTests;

public class MigrationApplierLifetimeTests
{
    [Fact]
    public void UseDbMigrationApplier_ShouldResolveMigrationServicesWithinScope()
    {
        ServiceCollection services = new();
        services.AddSingleton<MigrationProbe>();
        services.AddScoped<ScopedDependency>();
        services.AddTransient<IDbMigrationApplierService, ScopedMigrationApplier>();

        using ServiceProvider provider = services.BuildServiceProvider(
            new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true }
        );
        ApplicationBuilder application = new(provider);

        application.UseDbMigrationApplier();

        Assert.True(provider.GetRequiredService<MigrationProbe>().WasInitialized);
    }

    private sealed class ScopedDependency;

    private sealed class MigrationProbe
    {
        public bool WasInitialized { get; set; }
    }

    private sealed class ScopedMigrationApplier : IDbMigrationApplierService
    {
        private readonly MigrationProbe _probe;

        public ScopedMigrationApplier(ScopedDependency dependency, MigrationProbe probe)
        {
            _ = dependency;
            _probe = probe;
        }

        public void Initialize() => _probe.WasInitialized = true;
    }
}
