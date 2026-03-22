namespace Logistics.Api.BuildingBlocks.Domain.Time;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
