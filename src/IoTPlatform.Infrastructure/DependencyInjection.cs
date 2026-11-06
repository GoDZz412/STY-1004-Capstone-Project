using IoTPlatform.Application.Abstractions.Repositories;
using IoTPlatform.Infrastructure.Persistence;
using IoTPlatform.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IoTPlatform.Infrastructure;

/// <summary>
/// Registers the persistence layer: the SQL Server DbContext, <see cref="IUnitOfWork"/> and the four repositories.
/// Everything is scoped, so one request (or unit of work) shares one DbContext.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<IoTPlatformDbContext>(options => options.UseSqlServer(connectionString));

        // The same scoped DbContext instance, so SaveChangesAsync saves what the repositories added.
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<IoTPlatformDbContext>());

        services.AddScoped<IDeviceAssetRepository, DeviceAssetRepository>();
        services.AddScoped<ITelemetryMetricRepository, TelemetryMetricRepository>();
        services.AddScoped<IAlertRuleRepository, AlertRuleRepository>();
        services.AddScoped<IMaintenanceLogRepository, MaintenanceLogRepository>();

        return services;
    }
}
