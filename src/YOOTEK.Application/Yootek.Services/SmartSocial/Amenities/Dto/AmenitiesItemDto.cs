using Abp.Application.Services.Dto;

namespace Yootek.Services.SmartSocial.Advertisements
{
    public class AmenitiesItemDto : EntityDto<long>
    {
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public string Detail { get; set; }
        public double? OriginPrice { get; set; }
        public double Price { get; set; }
        public double MinimumDeposit { get; set; }
        public string AttributeExtensions { get; set; }
        public long GroupId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDisplay { get; set; }
        public int? Stock { get; set; }
    }
}
