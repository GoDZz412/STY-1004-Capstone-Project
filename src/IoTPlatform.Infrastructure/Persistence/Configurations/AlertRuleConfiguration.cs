using IoTPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IoTPlatform.Infrastructure.Persistence.Configurations;

internal sealed class AlertRuleConfiguration
    : IEntityTypeConfiguration<AlertRule>
{
    public void Configure(EntityTypeBuilder<AlertRule> builder)
    {
        builder.ToTable("AlertRules");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(AlertRule.NameMaxLength);

        builder.Property(x => x.MetricType)
            .HasConversion<string>()
            .HasMaxLength(PersistenceConstants.EnumMaxLength);

        builder.Property(x => x.Operator)
            .HasConversion<string>()
            .HasMaxLength(PersistenceConstants.EnumMaxLength);

        builder.Property(x => x.Severity)
            .HasConversion<string>()
            .HasMaxLength(PersistenceConstants.EnumMaxLength);

        builder.Property(x => x.Threshold)
            .HasPrecision(18, 4);

        builder.HasIndex(x => new
        {
            x.DeviceAssetId,
            x.MetricType
        });

        builder.HasOne<DeviceAsset>()
            .WithMany(d => d.AlertRules)
            .HasForeignKey(r => r.DeviceAssetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
