using MediatR;

namespace Logistics.Api.BuildingBlocks.Application.Abstractions.CQRS;

public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>;
