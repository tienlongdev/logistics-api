using Logistics.Api.BuildingBlocks.Application.Abstractions.CQRS;
using Logistics.Api.BuildingBlocks.Application.Results;
using Logistics.Api.BuildingBlocks.Domain.Time;
using Logistics.Api.Pricing.Application.Services;
using Logistics.Api.Shipments.Application.Abstractions;
using Logistics.Api.Shipments.Application.Errors;
using Logistics.Api.Shipments.Domain.Entities;
using Logistics.Api.Shipments.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Logistics.Api.Shipments.Application.Commands.CreateShipment;

internal sealed class CreateShipmentCommandHandler
    : ICommandHandler<CreateShipmentCommand, Result<CreateShipmentResponse>>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IMerchantScopeService _merchantScopeService;
    private readonly IPricingCalculator _pricingCalculator;
    private readonly IIdempotencyService _idempotencyService;
    private readonly IClock _clock;
    private readonly ILogger<CreateShipmentCommandHandler> _logger;

    public CreateShipmentCommandHandler(
        IShipmentRepository shipmentRepository,
        IMerchantScopeService merchantScopeService,
        IPricingCalculator pricingCalculator,
        IIdempotencyService idempotencyService,
        IClock clock,
        ILogger<CreateShipmentCommandHandler> logger)
    {
        _shipmentRepository = shipmentRepository;
        _merchantScopeService = merchantScopeService;
        _pricingCalculator = pricingCalculator;
        _idempotencyService = idempotencyService;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Result<CreateShipmentResponse>> Handle(
        CreateShipmentCommand command,
        CancellationToken cancellationToken)
    {
        // ── A4: Idempotency — Redis fast-path ─────────────────────────────────
        if (!string.IsNullOrWhiteSpace(command.IdempotencyKey))
        {
            var cachedId = await _idempotencyService.TryGetAsync(command.IdempotencyKey, cancellationToken);
            if (cachedId.HasValue)
            {
                var cached = await _shipmentRepository.GetByIdAsync(cachedId.Value, cancellationToken);
                if (cached is not null)
                {
                    _logger.LogDebug("Idempotency hit (cache) for key {Key}", command.IdempotencyKey);
                    return Result<CreateShipmentResponse>.Success(ToResponse(cached, 0m));
                }
            }

            // ── A4: DB idempotency check ─────────────────────────────────────
            var existing = await _shipmentRepository.GetByIdempotencyKeyAsync(
                command.IdempotencyKey, cancellationToken);
            if (existing is not null)
            {
                _logger.LogDebug("Idempotency hit (DB) for key {Key}", command.IdempotencyKey);
                await _idempotencyService.StoreAsync(command.IdempotencyKey, existing.Id, cancellationToken);
                return Result<CreateShipmentResponse>.Success(ToResponse(existing, 0m));
            }
        }

        // ── Merchant scope enforcement ────────────────────────────────────────
        var merchant = await _merchantScopeService.GetByUserIdAsync(command.RequestingUserId, cancellationToken);
        if (merchant is null)
        {
            _logger.LogWarning("Merchant not found for user {UserId}", command.RequestingUserId);
            return Result<CreateShipmentResponse>.Failure(ShipmentErrors.MerchantScopeForbidden);
        }

        // ── D1: Fee calculation ───────────────────────────────────────────────
        var pricingServiceType = (Logistics.Api.Pricing.Domain.Enums.ServiceType)(int)command.ServiceType;
        var feeResult = await _pricingCalculator.CalculateAsync(
            new CalculateFeeRequest(
                ServiceType: pricingServiceType,
                SenderProvince: command.Sender.Province,
                ReceiverProvince: command.Receiver.Province,
                WeightGram: command.Package.WeightGram,
                CodAmount: command.CodAmount),
            cancellationToken);

        if (feeResult.IsFailure)
        {
            _logger.LogWarning("No pricing rule found for ServiceType={ServiceType} Province={Province}→{ReceiverProv} Weight={Weight}g",
                command.ServiceType, command.Sender.Province, command.Receiver.Province, command.Package.WeightGram);
            return Result<CreateShipmentResponse>.Failure(ShipmentErrors.NoPricingRule);
        }

        var fees = feeResult.Value;

        // ── Generate sequential codes ─────────────────────────────────────────
        var seqNum = await _shipmentRepository.GetNextSequenceNumberAsync(cancellationToken);
        var today = DateOnly.FromDateTime(_clock.UtcNow.UtcDateTime);
        var trackingCode = $"LGA{today:yyMM}{seqNum:000000}";
        var shipmentCode = $"SHIP-{today:yyyyMMdd}-{seqNum % 10_000:0000}";

        // ── Create aggregate ──────────────────────────────────────────────────
        var shipment = Shipment.Create(
            trackingCode: trackingCode,
            shipmentCode: shipmentCode,
            idempotencyKey: command.IdempotencyKey,
            merchantId: merchant.MerchantId,
            merchantCode: merchant.MerchantCode,
            senderName: command.Sender.Name,
            senderPhone: command.Sender.Phone,
            senderAddress: command.Sender.Address,
            senderProvince: command.Sender.Province,
            senderDistrict: command.Sender.District,
            senderWard: command.Sender.Ward,
            receiverName: command.Receiver.Name,
            receiverPhone: command.Receiver.Phone,
            receiverAddress: command.Receiver.Address,
            receiverProvince: command.Receiver.Province,
            receiverDistrict: command.Receiver.District,
            receiverWard: command.Receiver.Ward,
            weightGram: command.Package.WeightGram,
            lengthCm: command.Package.LengthCm,
            widthCm: command.Package.WidthCm,
            heightCm: command.Package.HeightCm,
            packageDescription: command.Package.Description,
            declaredValue: command.DeclaredValue,
            codAmount: command.CodAmount,
            shippingFee: fees.ShippingFee,
            insuranceFee: 0m,                       // TODO: insurance module
            totalFee: fees.TotalFee,
            serviceType: command.ServiceType,
            deliveryNote: command.DeliveryNote,
            createdAt: _clock.UtcNow);

        // ── Persist with idempotency key unique-constraint safety ─────────────
        try
        {
            _shipmentRepository.Add(shipment);
            await _shipmentRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (IsIdempotencyConflict(ex))
        {
            // Race condition: another request with the same key committed first
            _logger.LogWarning("Idempotency unique constraint race for key {Key}", command.IdempotencyKey);
            var conflict = await _shipmentRepository.GetByIdempotencyKeyAsync(
                command.IdempotencyKey!, cancellationToken);
            if (conflict is not null)
                return Result<CreateShipmentResponse>.Success(ToResponse(conflict, 0m));

            throw; // different unique constraint was violated
        }

        // ── Cache for A4 ──────────────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(command.IdempotencyKey))
            await _idempotencyService.StoreAsync(command.IdempotencyKey, shipment.Id, cancellationToken);

        _logger.LogInformation(
            "Shipment {ShipmentId} ({TrackingCode}) created for merchant {MerchantId}",
            shipment.Id, shipment.TrackingCode, merchant.MerchantId);

        return Result<CreateShipmentResponse>.Success(ToResponse(shipment, fees.CodFee));
    }

    private static CreateShipmentResponse ToResponse(Shipment s, decimal codFee) =>
        new(s.Id,
            s.TrackingCode,
            s.ShipmentCode,
            s.CurrentStatus.ToString(),
            s.ShippingFee,
            codFee,
            s.TotalFee,
            s.CreatedAt);

    /// <summary>
    /// Detects a PostgreSQL unique-constraint violation (error code 23505)
    /// regardless of which ORM layer wraps it.
    /// </summary>
    private static bool IsIdempotencyConflict(Exception ex)
    {
        var msg = (ex.InnerException ?? ex).Message;
        return msg.Contains("23505") ||
               msg.Contains("idempotency_key", StringComparison.OrdinalIgnoreCase);
    }
}
