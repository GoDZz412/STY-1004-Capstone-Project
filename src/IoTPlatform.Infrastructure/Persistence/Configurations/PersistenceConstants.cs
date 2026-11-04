namespace IoTPlatform.Infrastructure.Persistence.Configurations;

/// <summary>Values shared by all EF Core entity configurations (PHASE_1 §5.2).</summary>
internal static class PersistenceConstants
{
    /// <summary>Max length of enum columns. Enums are stored as strings so the data stays readable.</summary>
    public const int EnumMaxLength = 30;
}
