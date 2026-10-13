using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Core.Persistence.DbMigrationApplier;

public static class DatabaseFacadeDbMigrationApplierExtensions
{
    public static DatabaseFacade EnsureDbApplied(this DatabaseFacade databaseFacade)
    {
        if (databaseFacade.IsInMemory())
            _ = databaseFacade.EnsureCreated();
        else if (databaseFacade.IsRelational())
            databaseFacade.Migrate();

        return databaseFacade;
    }
}
