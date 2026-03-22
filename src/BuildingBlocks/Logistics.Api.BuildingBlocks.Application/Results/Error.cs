namespace Logistics.Api.BuildingBlocks.Application.Results;

/// <summary>
/// Error dùng cho Result pattern.
/// Code nên là stable string để client/map dễ dàng.
/// </summary>
public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new("none", string.Empty);
}
