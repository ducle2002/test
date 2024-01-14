using Abp.Application.Services.Dto;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Yootek.Services.SmartSocial.Advertisements
{
    public class AmenitiesComboDto: EntityDto<long>
    {
        public int? TenantId { get; set; }
        [StringLength(1000)]
        public string Name { get; set; }
        public double OriginPrice { get; set; }
        public double TotalPrice { get; set; }
        public double Deposit {  get; set; }
        public string Description { get; set; }
        public string AttributeExtensions { get; set; }
        public List<long> ItemIds { get; set; }
        public int? Stock { get; set; }
    }

    public class AmenitiesComboGetDetailDto : EntityDto<long>
    {
        public int? TenantId { get; set; }
        [StringLength(1000)]
        public string Name { get; set; }
        public double OriginPrice { get; set; }
        public double TotalPrice { get; set; }
        public double Deposit { get; set; }
        public string Description { get; set; }
        public string AttributeExtensions { get; set; }
        public List<long> ItemIds { get; set; }
        public List<AmenitiesItemDto> Items { get; set; }
        public int? Stock { get; set; }
    }
}
