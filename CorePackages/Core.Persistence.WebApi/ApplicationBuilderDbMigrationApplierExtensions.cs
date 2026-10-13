using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Core.Persistence.DbMigrationApplier;

namespace Core.Persistence.WebApi;

public static class ApplicationBuilderDbMigrationApplierExtensions
{
    public static IApplicationBuilder UseDbMigrationApplier(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        IEnumerable<IDbMigrationApplierService> migrationCreatorServices =
            scope.ServiceProvider.GetServices<IDbMigrationApplierService>();
        foreach (IDbMigrationApplierService service in migrationCreatorServices)
            service.Initialize();

        return app;
    }
}
