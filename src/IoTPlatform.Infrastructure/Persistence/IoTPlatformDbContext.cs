using IoTPlatform.Application.Abstractions.Repositories;
using IoTPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IoTPlatform.Infrastructure.Persistence;

/// <summary>
/// EF Core database context for the platform. It is provider-neutral: SQL Server is chosen by
/// <see cref="DesignTimeDbContextFactory"/>, SQLite by the unit-test helper.
/// </summary>
/// <remarks>
/// Each member adds the <c>DbSet</c> for their own entities here (A: DeviceAssets, B: TelemetryMetrics
/// and AlertRules, C: MaintenanceLogs). Mappings go in <c>Persistence/Configurations</c> and are applied automatically.
/// </remarks>
public sealed class IoTPlatformDbContext : DbContext, IUnitOfWork
{
    public IoTPlatformDbContext(DbContextOptions<IoTPlatformDbContext> options)
        : base(options)
    {
    }

    public DbSet<DeviceAsset> DeviceAssets => Set<DeviceAsset>();
    public DbSet<MaintenanceLog> MaintenanceLogs => Set<MaintenanceLog>();
    public DbSet<AlertRule> AlertRules => Set<AlertRule>();
    public DbSet<TelemetryMetric> TelemetryMetrics => Set<TelemetryMetric>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IoTPlatformDbContext).Assembly);
    }
}
