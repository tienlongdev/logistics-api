namespace Logistics.Api.BuildingBlocks.Domain.Abstractions;

/// <summary>
/// Base class cho Entity theo DDD.
/// Entity có identity, equality theo Id.
/// </summary>
public abstract class Entity<TId>
    where TId : notnull
{
    public TId Id { get; protected set; } = default!;

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other) return false;
        if (ReferenceEquals(this, other)) return true;

        // Nếu Id chưa set (default) thì không nên coi là equal
        if (EqualityComparer<TId>.Default.Equals(Id, default!)) return false;
        if (EqualityComparer<TId>.Default.Equals(other.Id, default!)) return false;

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode()
        => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
        => Equals(left, right);

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
        => !Equals(left, right);
}
