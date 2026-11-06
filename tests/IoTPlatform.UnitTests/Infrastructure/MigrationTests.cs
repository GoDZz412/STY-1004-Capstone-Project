using IoTPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IoTPlatform.UnitTests.Infrastructure;

[TestClass]
public class MigrationTests
{
    // Builds the SQL Server model only; no database connection is opened.
    // Fails when an EF configuration changed but nobody added a migration for it (only Member C adds migrations).
    [TestMethod]
    public void Model_ComparedWithLatestMigration_HasNoPendingChanges()
    {
        using var context = new DesignTimeDbContextFactory().CreateDbContext([]);

        Assert.IsFalse(context.Database.HasPendingModelChanges(),
            "The EF model differs from the migration snapshot: add a new migration (dotnet ef migrations add <Name>).");
    }
}
