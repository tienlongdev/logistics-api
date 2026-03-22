using MediatR;

namespace Logistics.Api.BuildingBlocks.Application.Abstractions.CQRS;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>;
