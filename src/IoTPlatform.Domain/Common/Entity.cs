namespace IoTPlatform.Domain.Common;

/// <summary>Base class for all entities. Identity is the <see cref="Id"/>; equality is by Id and type.</summary>
public abstract class Entity : IEquatable<Entity>
{
    /// <summary>Required by EF Core.</summary>
    protected Entity() { }

    protected Entity(Guid id)
    {
        Id = Guard.NotEmpty(id, nameof(Id));
    }

    public Guid Id { get; protected set; }

    public bool Equals(Entity? other) =>
        other is not null
        && (ReferenceEquals(this, other)
            || (GetType() == other.GetType() && Id != Guid.Empty && Id == other.Id));

    public override bool Equals(object? obj) => Equals(obj as Entity);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity? left, Entity? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(Entity? left, Entity? right) => !(left == right);
}