using JetBrains.Annotations;

namespace Yootek.Yootek.Services.Yootek.Payments.Dto;

public class GetListOnepayMerchant
{
    public int? TenantId { get; set; }
    [CanBeNull] public string Keyword { get; set; }
    public int? UrbanId { get; set; }
    public int? BuildingId { get; set; }
}