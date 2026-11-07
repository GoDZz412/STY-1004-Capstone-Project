using IoTPlatform.Application.Abstractions.Repositories;
using IoTPlatform.Infrastructure;
using IoTPlatform.Infrastructure.Persistence;
using IoTPlatform.Infrastructure.Repositories;
using IoTPlatform.Infrastructure.Seeding;
using Microsoft.Extensions.DependencyInjection;

namespace IoTPlatform.UnitTests.Infrastructure;

[TestClass]
public class DependencyInjectionTests
{
    // Resolving services never opens a database connection, so any well-formed connection string works here.
    private const string ConnectionString = DesignTimeDbContextFactory.DefaultConnectionString;

    private static ServiceProvider BuildProvider() =>
        new ServiceCollection().AddInfrastructure(ConnectionString).BuildServiceProvider(validateScopes: true);

    [TestMethod]
    [DataRow(typeof(IDeviceAssetRepository), typeof(DeviceAssetRepository))]
    [DataRow(typeof(ITelemetryMetricRepository), typeof(TelemetryMetricRepository))]
    [DataRow(typeof(IAlertRuleRepository), typeof(AlertRuleRepository))]
    [DataRow(typeof(IMaintenanceLogRepository), typeof(MaintenanceLogRepository))]
    [DataRow(typeof(IUnitOfWork), typeof(IoTPlatformDbContext))]
    [DataRow(typeof(JsonSeedLoader), typeof(JsonSeedLoader))]
    public void AddInfrastructure_ValidConnectionString_ResolvesService(Type serviceType, Type expectedType)
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var service = scope.ServiceProvider.GetRequiredService(serviceType);

        Assert.IsInstanceOfType(service, expectedType);
    }

    [TestMethod]
    public void AddInfrastructure_SameScope_UnitOfWorkIsTheRepositoriesDbContext()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<IoTPlatformDbContext>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // If these were different instances, SaveChangesAsync would save nothing the repositories added.
        Assert.AreSame(context, unitOfWork);
    }

    [TestMethod]
    public void AddInfrastructure_DifferentScopes_GetDifferentDbContexts()
    {
        using var provider = BuildProvider();
        using var first = provider.CreateScope();
        using var second = provider.CreateScope();

        Assert.AreNotSame(
            first.ServiceProvider.GetRequiredService<IUnitOfWork>(),
            second.ServiceProvider.GetRequiredService<IUnitOfWork>());
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void AddInfrastructure_BlankConnectionString_ThrowsArgumentException(string connectionString)
    {
        Assert.Throws<ArgumentException>(() => new ServiceCollection().AddInfrastructure(connectionString));
    }
}
