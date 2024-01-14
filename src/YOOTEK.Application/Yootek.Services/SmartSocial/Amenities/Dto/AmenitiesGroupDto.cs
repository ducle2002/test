using Abp.Application.Services.Dto;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace Yootek.Services.SmartSocial.Advertisements
{
    public class AmenitiesGroupDto : EntityDto<long>
    {
        public int? TenantId { get; set; }
        [StringLength(1000)]
        public string Name { get; set; }
        public string Description { get; set; }
        public string AttributeExtensions { get; set; }
    }

    public class AmenitiesGroupGetDetailDto : EntityDto<long>
    {
        public int? TenantId { get; set; }
        [StringLength(1000)]
        public string Name { get; set; }
        public string Description { get; set; }
        public string AttributeExtensions { get; set; }
        public List<AmenitiesItemDto> Items { get; set; }
    }
}
