using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Persistence.Contexts;

public sealed class BaseDbContextFactory : IDesignTimeDbContextFactory<BaseDbContext>
{
    private const string DefaultConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=EventReservation;Trusted_Connection=True;TrustServerCertificate=True";

    public BaseDbContext CreateDbContext(string[] args)
    {
        string connectionString = Environment.GetEnvironmentVariable("EVENT_RESERVATION_CONNECTION_STRING")
            ?? DefaultConnectionString;
        DbContextOptions<BaseDbContext> options = new DbContextOptionsBuilder<BaseDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?> { ["ConnectionStrings:BaseDb"] = connectionString }
            )
            .Build();

        return new BaseDbContext(options, configuration);
    }
}
