namespace Logistics.Api.BuildingBlocks.Domain.Abstractions;

/// <summary>
/// Base class cho ValueObject.
/// Equality dựa trên các thành phần (components).
/// </summary>
public abstract class ValueObject
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (obj.GetType() != GetType()) return false;

        var other = (ValueObject)obj;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        // HashCode.Combine cho sequence (stable)
        return GetEqualityComponents()
            .Aggregate(0, (current, obj) => HashCode.Combine(current, obj));
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
        => Equals(left, right);

    public static bool operator !=(ValueObject? left, ValueObject? right)
        => !Equals(left, right);
}
