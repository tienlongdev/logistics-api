using Logistics.Api.Shipments.Domain.Entities;

namespace Logistics.Api.Search.Infrastructure.Models;

public sealed record ShipmentSearchDocument(
    Guid ShipmentId,
    string TrackingCode,
    string ShipmentCode,
    Guid MerchantId,
    string MerchantCode,
    string ReceiverPhone,
    string ReceiverName,
    string SenderName,
    string Status,
    string ServiceType,
    decimal CodAmount,
    decimal ShippingFee,
    decimal TotalFee,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static ShipmentSearchDocument FromShipment(Shipment shipment) =>
        new(
            shipment.Id,
            shipment.TrackingCode,
            shipment.ShipmentCode,
            shipment.MerchantId,
            shipment.MerchantCode,
            shipment.ReceiverPhone,
            shipment.ReceiverName,
            shipment.SenderName,
            shipment.CurrentStatus.ToString(),
            shipment.ServiceType.ToString(),
            shipment.CodAmount,
            shipment.ShippingFee,
            shipment.TotalFee,
            shipment.CreatedAt,
            shipment.UpdatedAt);
}