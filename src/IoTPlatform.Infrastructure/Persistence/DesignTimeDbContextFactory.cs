using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IoTPlatform.Infrastructure.Persistence;

/// <summary>
/// Lets the <c>dotnet ef</c> tools create the context (there is no startup project in Phase 1).
/// Uses the <c>IOTPLATFORM_CONNECTION</c> environment variable, or LocalDB when it is not set.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<IoTPlatformDbContext>
{
    public const string ConnectionStringEnvironmentVariable = "IOTPLATFORM_CONNECTION";

    public const string DefaultConnectionString =
        @"Server=(localdb)\MSSQLLocalDB;Database=IoTPlatformDb;Trusted_Connection=True;TrustServerCertificate=True";

    public IoTPlatformDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = DefaultConnectionString;
        }

        var options = new DbContextOptionsBuilder<IoTPlatformDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new IoTPlatformDbContext(options);
    }
}
