using MediatR;

namespace Logistics.Api.BuildingBlocks.Application.Abstractions.CQRS;

/// <summary>
/// Query (read side). Trả về TResponse.
/// </summary>
public interface IQuery<out TResponse> : IRequest<TResponse>;
