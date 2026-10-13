using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Persistence.Contexts;
using Core.Persistence.DbMigrationApplier;
using Application.Services.Time;

namespace IntegrationTests;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _keepAliveConnection;
    private readonly string _connectionString;
    public TestClock Clock { get; } = new(DateTimeOffset.UtcNow);

    public CustomWebApplicationFactory()
    {
        _connectionString = $"Data Source=IntegrationTests-{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        _keepAliveConnection = new SqliteConnection(_connectionString);
        _keepAliveConnection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration(
            (_, configuration) => configuration.AddInMemoryCollection(CreateConfiguration())
        );
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<BaseDbContext>();
            services.RemoveAll<DbContextOptions<BaseDbContext>>();
            services.RemoveAll<IDbMigrationApplierService>();
            services.RemoveAll<IClock>();
            services.AddDbContext<BaseDbContext>(options => options.UseSqlite(_connectionString));
            services.AddTransient<IDbMigrationApplierService, SqliteSchemaInitializer>();
            services.AddSingleton<IClock>(Clock);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _keepAliveConnection.Dispose();
    }

    private static Dictionary<string, string?> CreateConfiguration() =>
        new()
        {
            ["AllowedHosts"] = "*",
            ["CacheSettings:SlidingExpiration"] = "2",
            ["ReservationExpiration:PollingIntervalSeconds"] = "3600",
            ["ReservationExpiration:BatchSize"] = "100",
            ["ConnectionStrings:BaseDb"] = "Server=(localdb)\\MSSQLLocalDB;Database=EventReservationTests;Trusted_Connection=True",
            ["ElasticSearchConfig:ConnectionString"] = "http://localhost:9200",
            ["ElasticSearchConfig:Password"] = string.Empty,
            ["ElasticSearchConfig:UserName"] = string.Empty,
            ["MailSettings:AuthenticationRequired"] = "false",
            ["MailSettings:DkimPrivateKey"] = string.Empty,
            ["MailSettings:DkimSelector"] = string.Empty,
            ["MailSettings:DomainName"] = "localhost",
            ["MailSettings:Password"] = string.Empty,
            ["MailSettings:Port"] = "25",
            ["MailSettings:SenderEmail"] = "noreply@localhost",
            ["MailSettings:SenderFullName"] = "Integration Tests",
            ["MailSettings:Server"] = "localhost",
            ["MailSettings:UserName"] = string.Empty,
            ["SeriLogConfigurations:FileLogConfiguration:FolderPath"] = "logs",
            ["TokenOptions:AccessTokenExpiration"] = "10",
            ["TokenOptions:Audience"] = "integration-tests",
            ["TokenOptions:Issuer"] = "integration-tests",
            ["TokenOptions:RefreshTokenTTL"] = "2",
            ["TokenOptions:SecurityKey"] = "integration-test-key-with-at-least-sixty-four-characters-1234567890",
            ["WebAPIConfiguration:APIDomain"] = "http://localhost/api",
            ["WebAPIConfiguration:AllowedOrigins:0"] = "http://localhost"
        };

    private sealed class SqliteSchemaInitializer : IDbMigrationApplierService
    {
        private readonly BaseDbContext _context;

        public SqliteSchemaInitializer(BaseDbContext context) => _context = context;

        public void Initialize() => _context.Database.EnsureCreated();
    }
}

public sealed class TestClock : IClock
{
    private readonly object _lock = new();
    private DateTimeOffset _utcNow;

    public TestClock(DateTimeOffset utcNow) => _utcNow = utcNow;

    public DateTimeOffset UtcNow
    {
        get
        {
            lock (_lock)
                return _utcNow;
        }
    }

    public void Advance(TimeSpan duration)
    {
        lock (_lock)
            _utcNow = _utcNow.Add(duration);
    }
}
