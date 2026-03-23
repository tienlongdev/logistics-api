using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Api.Search.Application.Abstractions;
using Logistics.Api.Search.Infrastructure.Models;
using Logistics.Api.Search.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;

namespace Logistics.Api.Search.Infrastructure.Services;

internal sealed class ElasticsearchShipmentSearchService : IShipmentSearchReadService, IShipmentSearchIndexService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ShipmentSearchOptions _options;
    private readonly ILogger<ElasticsearchShipmentSearchService> _logger;

    public ElasticsearchShipmentSearchService(
        IHttpClientFactory httpClientFactory,
        IOptions<ShipmentSearchOptions> options,
        ILogger<ElasticsearchShipmentSearchService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task EnsureIndexAsync(CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("elasticsearch");
        using var headResponse = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, _options.IndexName), ct);
        if (headResponse.IsSuccessStatusCode)
            return;

        if (headResponse.StatusCode != HttpStatusCode.NotFound)
        {
            var body = await headResponse.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"Elasticsearch index check failed: {(int)headResponse.StatusCode} {body}");
        }

        var payload = BuildCreateIndexPayload();
        using var createResponse = await client.PutAsync(
            _options.IndexName,
            new StringContent(payload, Encoding.UTF8, "application/json"),
            ct);

        if (createResponse.IsSuccessStatusCode)
        {
            _logger.LogInformation("Created Elasticsearch index {IndexName}", _options.IndexName);
            return;
        }

        var createBody = await createResponse.Content.ReadAsStringAsync(ct);
        if (createResponse.StatusCode == HttpStatusCode.BadRequest &&
            createBody.Contains("resource_already_exists_exception", StringComparison.OrdinalIgnoreCase))
            return;

        throw new InvalidOperationException($"Failed to create Elasticsearch index {_options.IndexName}: {createBody}");
    }

    public async Task UpsertShipmentAsync(ShipmentSearchDocument document, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("elasticsearch");
        var payload = JsonSerializer.Serialize(document, SerializerOptions);
        using var response = await client.PutAsync(
            $"{_options.IndexName}/_doc/{document.ShipmentId:D}",
            new StringContent(payload, Encoding.UTF8, "application/json"),
            ct);

        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync(ct);
        throw new InvalidOperationException($"Failed to upsert shipment {document.ShipmentId} into Elasticsearch: {body}");
    }

    public async Task<SearchShipmentsResult> SearchAsync(SearchShipmentsCriteria criteria, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("elasticsearch");
        var payload = BuildSearchPayload(criteria);

        using var response = await client.PostAsync(
            $"{_options.IndexName}/_search",
            new StringContent(payload, Encoding.UTF8, "application/json"),
            ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return new SearchShipmentsResult(0, Array.Empty<SearchShipmentDocumentDto>());

        var responseBody = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Elasticsearch search failed: {responseBody}");

        using var json = JsonDocument.Parse(responseBody);
        var root = json.RootElement;
        var hitsNode = root.GetProperty("hits");
        var total = hitsNode.GetProperty("total").GetProperty("value").GetInt64();

        var items = new List<SearchShipmentDocumentDto>();
        foreach (var hit in hitsNode.GetProperty("hits").EnumerateArray())
        {
            if (!hit.TryGetProperty("_source", out var source))
                continue;

            var document = source.Deserialize<ShipmentSearchDocument>(SerializerOptions);
            if (document is null)
                continue;

            items.Add(new SearchShipmentDocumentDto(
                document.ShipmentId,
                document.TrackingCode,
                document.ShipmentCode,
                document.MerchantCode,
                document.ReceiverPhone,
                document.ReceiverName,
                document.SenderName,
                document.Status,
                document.ServiceType,
                document.CodAmount,
                document.ShippingFee,
                document.TotalFee,
                document.CreatedAt,
                document.UpdatedAt));
        }

        return new SearchShipmentsResult(total, items);
    }

    private string BuildCreateIndexPayload()
    {
        var payload = new JsonObject
        {
            ["settings"] = new JsonObject
            {
                ["number_of_shards"] = 1,
                ["number_of_replicas"] = 0,
                ["analysis"] = new JsonObject
                {
                    ["normalizer"] = new JsonObject
                    {
                        ["lowercase_keyword"] = new JsonObject
                        {
                            ["type"] = "custom",
                            ["filter"] = new JsonArray("lowercase")
                        }
                    }
                }
            },
            ["mappings"] = new JsonObject
            {
                ["dynamic"] = "strict",
                ["properties"] = new JsonObject
                {
                    ["shipmentId"] = KeywordProperty(),
                    ["trackingCode"] = KeywordProperty(normalizer: true),
                    ["shipmentCode"] = KeywordProperty(normalizer: true),
                    ["merchantId"] = KeywordProperty(),
                    ["merchantCode"] = KeywordProperty(normalizer: true),
                    ["receiverPhone"] = KeywordProperty(),
                    ["receiverName"] = TextWithKeywordProperty(),
                    ["senderName"] = TextWithKeywordProperty(),
                    ["status"] = KeywordProperty(normalizer: true),
                    ["serviceType"] = KeywordProperty(normalizer: true),
                    ["codAmount"] = new JsonObject { ["type"] = "double" },
                    ["shippingFee"] = new JsonObject { ["type"] = "double" },
                    ["totalFee"] = new JsonObject { ["type"] = "double" },
                    ["createdAt"] = new JsonObject { ["type"] = "date" },
                    ["updatedAt"] = new JsonObject { ["type"] = "date" }
                }
            }
        };

        return payload.ToJsonString(SerializerOptions);
    }

    private string BuildSearchPayload(SearchShipmentsCriteria criteria)
    {
        var filters = new JsonArray();
        AddTermFilter(filters, "trackingCode", criteria.TrackingCode, normalize: true);
        AddTermFilter(filters, "shipmentCode", criteria.ShipmentCode, normalize: true);
        AddTermFilter(filters, "receiverPhone", criteria.ReceiverPhone, normalize: false);
        AddTermFilter(filters, "merchantCode", criteria.MerchantCode, normalize: true);
        AddTermFilter(filters, "status", criteria.Status, normalize: true);

        if (criteria.FromDate.HasValue || criteria.ToDate.HasValue)
        {
            var rangeBody = new JsonObject();
            if (criteria.FromDate.HasValue)
                rangeBody["gte"] = criteria.FromDate.Value;
            if (criteria.ToDate.HasValue)
                rangeBody["lte"] = criteria.ToDate.Value;

            filters.Add(new JsonObject
            {
                ["range"] = new JsonObject
                {
                    ["createdAt"] = rangeBody
                }
            });
        }

        var payload = new JsonObject
        {
            ["from"] = (criteria.Page - 1) * criteria.PageSize,
            ["size"] = criteria.PageSize,
            ["track_total_hits"] = true,
            ["query"] = new JsonObject
            {
                ["bool"] = new JsonObject
                {
                    ["filter"] = filters
                }
            },
            ["sort"] = new JsonArray(
                new JsonObject
                {
                    [criteria.SortField] = new JsonObject
                    {
                        ["order"] = criteria.SortOrder
                    }
                },
                new JsonObject
                {
                    ["shipmentId"] = new JsonObject
                    {
                        ["order"] = "asc"
                    }
                })
        };

        return payload.ToJsonString(SerializerOptions);
    }

    private static void AddTermFilter(JsonArray filters, string field, string? value, bool normalize)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        filters.Add(new JsonObject
        {
            ["term"] = new JsonObject
            {
                [field] = normalize ? value.Trim().ToLowerInvariant() : value.Trim()
            }
        });
    }

    private static JsonObject KeywordProperty(bool normalizer = false)
    {
        var property = new JsonObject { ["type"] = "keyword" };
        if (normalizer)
            property["normalizer"] = "lowercase_keyword";

        return property;
    }

    private static JsonObject TextWithKeywordProperty() => new()
    {
        ["type"] = "text",
        ["fields"] = new JsonObject
        {
            ["keyword"] = new JsonObject
            {
                ["type"] = "keyword",
                ["ignore_above"] = 256
            }
        }
    };
}