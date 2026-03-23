namespace Logistics.Api.Shipments.Application.Abstractions;

public interface IOutboxMessageWriter
{
    void Add<TMessage>(TMessage message) where TMessage : class;
}