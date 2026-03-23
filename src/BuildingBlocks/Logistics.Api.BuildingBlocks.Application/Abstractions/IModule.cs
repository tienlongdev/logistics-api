namespace Logistics.Api.BuildingBlocks.Application.Abstractions;

/// <summary>
/// Marker interface for module discovery.
/// Each module self-registers via IServiceCollection extension methods
/// (e.g. <c>services.AddIdentityModule(configuration)</c>).
/// The Host wires modules via explicit list or assembly scanning.
/// </summary>
public interface IModule
{
    string Name { get; }
}
