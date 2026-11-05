using IoTPlatform.Domain.Entities;
using IoTPlatform.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IoTPlatform.Infrastructure.Persistence.Configurations;

internal sealed class DeviceAssetConfiguration : IEntityTypeConfiguration<DeviceAsset>
{
    private const int MoneyLikePrecision = 18;
    private const int MoneyLikeScale = 4;

    public void Configure(EntityTypeBuilder<DeviceAsset> builder)
    {
        // CAST is required: SQLite stores decimal as TEXT, so a plain comparison would compare text ('10' > '9' is false).
        builder.ToTable("DeviceAssets", t =>
        {
            t.HasCheckConstraint("CK_DeviceAssets_OperatingRange_Positive", "CAST(OperatingMin AS REAL) >= 0");
            t.HasCheckConstraint("CK_DeviceAssets_OperatingRange_Order", "CAST(OperatingMax AS REAL) > CAST(OperatingMin AS REAL)");
        });

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.SerialNumber)
            .HasConversion(s => s.Value, v => SerialNumber.Create(v))
            .HasMaxLength(SerialNumber.MaxLength)
            .IsRequired();
        builder.HasIndex(d => d.SerialNumber).IsUnique().HasDatabaseName("IX_DeviceAssets_SerialNumber");

        builder.Property(d => d.Name).IsRequired().HasMaxLength(DeviceAsset.NameMaxLength);
        builder.Property(d => d.Location).IsRequired().HasMaxLength(DeviceAsset.LocationMaxLength);

        builder.Property(d => d.DeviceType).HasConversion<string>().HasMaxLength(PersistenceConstants.EnumMaxLength);
        builder.Property(d => d.Status).HasConversion<string>().HasMaxLength(PersistenceConstants.EnumMaxLength);
        builder.HasIndex(d => d.Status);

        builder.Property(d => d.InstalledOnUtc).HasColumnType("datetime2");
        builder.Property(d => d.CreatedAtUtc).HasColumnType("datetime2");

        builder.OwnsOne(d => d.OperatingRange, r =>
        {
            r.Property(x => x.Min).HasColumnName("OperatingMin").HasPrecision(MoneyLikePrecision, MoneyLikeScale);
            r.Property(x => x.Max).HasColumnName("OperatingMax").HasPrecision(MoneyLikePrecision, MoneyLikeScale);
            r.Property(x => x.Unit).HasColumnName("OperatingUnit").HasMaxLength(OperatingRange.UnitMaxLength).IsRequired();
        });

        // Relationships to AlertRule / MaintenanceLog are configured by their own configurations (B3, C3).
        builder.Navigation(d => d.AlertRules).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(d => d.MaintenanceLogs).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
