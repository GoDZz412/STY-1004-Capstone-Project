using IoTPlatform.Application.Abstractions.Repositories;
using IoTPlatform.Domain.Entities;
using IoTPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IoTPlatform.Infrastructure.Repositories;

public sealed class MaintenanceLogRepository : IMaintenanceLogRepository
{
    private readonly IoTPlatformDbContext _context;

    public MaintenanceLogRepository(IoTPlatformDbContext context)
    {
        _context = context;
    }

    public Task<MaintenanceLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.MaintenanceLogs.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

    public async Task<IReadOnlyList<MaintenanceLog>> GetForDeviceAsync(Guid deviceAssetId, CancellationToken cancellationToken = default) =>
        await _context.MaintenanceLogs
            .Where(l => l.DeviceAssetId == deviceAssetId)
            .OrderByDescending(l => l.PerformedOnUtc)
            .ToListAsync(cancellationToken);
}
