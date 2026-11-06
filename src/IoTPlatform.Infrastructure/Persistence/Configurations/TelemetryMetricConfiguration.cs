using IoTPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IoTPlatform.Infrastructure.Persistence.Configurations;

internal sealed class TelemetryMetricConfiguration
    : IEntityTypeConfiguration<TelemetryMetric>
{
    public void Configure(EntityTypeBuilder<TelemetryMetric> builder)
    {
        builder.ToTable("TelemetryMetrics");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.MetricType)
            .HasConversion<string>()
            .HasMaxLength(PersistenceConstants.EnumMaxLength);

        builder.Property(x => x.Value)
            .HasPrecision(18, 4);

        builder.Property(x => x.Unit)
            .IsRequired()
            .HasMaxLength(TelemetryMetric.UnitMaxLength);

        builder.Property(x => x.RecordedAtUtc)
            .HasColumnType("datetime2");

        builder.HasIndex(x => new
        {
            x.DeviceAssetId,
            x.RecordedAtUtc
        });

        builder.HasIndex(x => new
        {
            x.DeviceAssetId,
            x.MetricType
        });

        builder.HasOne<DeviceAsset>()
            .WithMany()
            .HasForeignKey(m => m.DeviceAssetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}