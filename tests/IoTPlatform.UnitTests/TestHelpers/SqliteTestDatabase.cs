using IoTPlatform.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace IoTPlatform.UnitTests.TestHelpers;

/// <summary>
/// A fresh, isolated SQLite in-memory database with every configured table created.
/// Use one per test; disposing it destroys the database.
/// </summary>
/// <example>
/// <code>
/// using var db = SqliteTestDatabase.Create();
/// var repo = new DeviceAssetRepository(db.Context);
/// await repo.AddAsync(device);
/// await db.Context.SaveChangesAsync();
///
/// using var readContext = db.CreateNewContext();   // reads from the database, not EF's cache
/// var loaded = await new DeviceAssetRepository(readContext).GetByIdAsync(device.Id);
/// </code>
/// </example>
public sealed class SqliteTestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<IoTPlatformDbContext> _options;

    private SqliteTestDatabase(SqliteConnection connection, DbContextOptions<IoTPlatformDbContext> options)
    {
        _connection = connection;
        _options = options;
        Context = new IoTPlatformDbContext(options);
        Context.Database.EnsureCreated();
    }

    /// <summary>The main context for arranging and acting in a test.</summary>
    public IoTPlatformDbContext Context { get; }

    public static SqliteTestDatabase Create()
    {
        // The in-memory database lives only while this connection stays open.
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<IoTPlatformDbContext>()
            .UseSqlite(connection)
            .Options;

        return new SqliteTestDatabase(connection, options);
    }

    /// <summary>
    /// Creates a second context on the same database with an empty change tracker.
    /// The caller must dispose it.
    /// </summary>
    public IoTPlatformDbContext CreateNewContext() => new(_options);

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
