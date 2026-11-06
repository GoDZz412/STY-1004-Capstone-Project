using IoTPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IoTPlatform.Infrastructure.Persistence.Configurations;

internal sealed class MaintenanceLogConfiguration : IEntityTypeConfiguration<MaintenanceLog>
{
    public void Configure(EntityTypeBuilder<MaintenanceLog> builder)
    {
        builder.ToTable("MaintenanceLogs");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();

        builder.Property(l => l.MaintenanceType).HasConversion<string>().HasMaxLength(PersistenceConstants.EnumMaxLength);
        builder.Property(l => l.Technician).IsRequired().HasMaxLength(MaintenanceLog.TechnicianMaxLength);
        builder.Property(l => l.Description).IsRequired().HasMaxLength(MaintenanceLog.DescriptionMaxLength);
        builder.Property(l => l.PerformedOnUtc).HasColumnType("datetime2");

        builder.HasIndex(l => new { l.DeviceAssetId, l.PerformedOnUtc });

        // The child owns the relationship (PHASE_1 §5.2). Restrict: maintenance history must not be deleted with its device.
        builder.HasOne<DeviceAsset>()
            .WithMany(d => d.MaintenanceLogs)
            .HasForeignKey(l => l.DeviceAssetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
