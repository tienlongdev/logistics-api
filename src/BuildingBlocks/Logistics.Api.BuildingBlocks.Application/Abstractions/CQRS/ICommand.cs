using MediatR;

namespace Logistics.Api.BuildingBlocks.Application.Abstractions.CQRS;

/// <summary>
/// Command (write side). Trả về TResponse.
/// </summary>
public interface ICommand<out TResponse> : IRequest<TResponse>;
