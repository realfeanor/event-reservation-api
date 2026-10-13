using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Core.Persistence.DbMigrationApplier;

namespace Core.Persistence.DependencyInjection;

public static class ServiceCollectionDbMigrationApplierExtensions
{
    public static IServiceCollection AddDbMigrationApplier<TDbContext>(
        this IServiceCollection services,
        Func<IServiceProvider, TDbContext> contextFactory
    )
        where TDbContext : DbContext
    {
        _ = services.AddTransient<IDbMigrationApplierService, DbMigrationApplierManager<TDbContext>>(
            serviceProvider => new DbMigrationApplierManager<TDbContext>(contextFactory(serviceProvider))
        );
        _ = services.AddTransient<IDbMigrationApplierService<TDbContext>, DbMigrationApplierManager<TDbContext>>(
            serviceProvider => new DbMigrationApplierManager<TDbContext>(contextFactory(serviceProvider))
        );

        return services;
    }
}
