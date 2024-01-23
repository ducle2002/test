using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Yootek.Yootek.Services.Yootek.Payments.Dto;

public class GetListOnepayMerchant
{
    [JsonPropertyName("tenantId")] public int? TenantId { get; set; }

    [JsonPropertyName("keyword")]
    [CanBeNull]
    public string Keyword { get; set; }

    [JsonPropertyName("urbanId")] public int? UrbanId { get; set; }
    [JsonPropertyName("buildingId")] public int? BuildingId { get; set; }
}