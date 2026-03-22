namespace Logistics.Api.BuildingBlocks.Domain.Time;

/// <summary>
/// Abstraction để time testable (tránh DateTime.UtcNow trong domain/app).
/// </summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
