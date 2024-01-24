using JetBrains.Annotations;

namespace Yootek.Yootek.Services.Yootek.Payments.Dto;

public class UpdateOnepayMerchantDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int? UrbanId { get; set; }
    public int? BuildingId { get; set; }
    public string Merchant { get; set; }
    public string AccessCode { get; set; }
    [CanBeNull] public string Description { get; set; }
}