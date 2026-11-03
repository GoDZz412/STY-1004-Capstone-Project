using IoTPlatform.Application.Abstractions.Repositories;
using IoTPlatform.UnitTests.TestHelpers;
using Microsoft.EntityFrameworkCore;

namespace IoTPlatform.UnitTests.Infrastructure;

[TestClass]
public sealed class SqliteTestDatabaseTests
{
    [TestMethod]
    public void Create_WhenCalled_ReturnsContextThatCanConnect()
    {
        using var db = SqliteTestDatabase.Create();

        Assert.IsTrue(db.Context.Database.CanConnect());
    }

    [TestMethod]
    public async Task SaveChangesAsync_ThroughUnitOfWorkWithNoChanges_ReturnsZero()
    {
        using var db = SqliteTestDatabase.Create();
        IUnitOfWork unitOfWork = db.Context;

        var saved = await unitOfWork.SaveChangesAsync();

        Assert.AreEqual(0, saved);
    }

    [TestMethod]
    public void CreateNewContext_AfterWriteInMainContext_SeesSameData()
    {
        using var db = SqliteTestDatabase.Create();
        db.Context.Database.ExecuteSqlRaw("CREATE TABLE Probe (Id INTEGER PRIMARY KEY)");
        db.Context.Database.ExecuteSqlRaw("INSERT INTO Probe (Id) VALUES (1)");

        using var readContext = db.CreateNewContext();
        var count = readContext.Database
            .SqlQueryRaw<int>("SELECT COUNT(*) AS \"Value\" FROM Probe")
            .Single();

        Assert.AreEqual(1, count);
    }

    [TestMethod]
    public void CreateNewContext_WhenCalled_ReturnsDifferentInstance()
    {
        using var db = SqliteTestDatabase.Create();

        using var other = db.CreateNewContext();

        Assert.AreNotSame(db.Context, other);
    }

    [TestMethod]
    public void Create_CalledTwice_ReturnsIsolatedDatabases()
    {
        using var first = SqliteTestDatabase.Create();
        using var second = SqliteTestDatabase.Create();
        first.Context.Database.ExecuteSqlRaw("CREATE TABLE Probe (Id INTEGER PRIMARY KEY)");

        var probeTables = second.Context.Database
            .SqlQueryRaw<int>("SELECT COUNT(*) AS \"Value\" FROM sqlite_master WHERE type = 'table' AND name = 'Probe'")
            .Single();

        Assert.AreEqual(0, probeTables);
    }
}
