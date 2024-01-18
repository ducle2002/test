using Abp.AutoMapper;
using Yootek.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;

namespace Yootek.Yootek.Services.Yootek.SmartCommunity.Building.Dto
{
    [AutoMap(typeof(AppOrganizationUnit))]
    public class BuildingByUserDto : EntityDto<long>
    {
        [StringLength(256)]
        public string ProjectCode { get; set; }
        public APP_ORGANIZATION_TYPE Type { get; set; }
        public string Description { get; set; }
        [StringLength(1000)]
        public string ImageUrl { get; set; }

        [StringLength(10)]
        public string PhoneNumber { get; set; }

        [StringLength(50)]
        public string Email { get; set; }
        public decimal? Area { get; set; }

        [StringLength(250)]
        public string Address { get; set; }
        [StringLength(100)]
        public string BuildingType { get; set; }
        [StringLength(100)]
        public string ProvinceCode { get; set; }
        [StringLength(100)]
        public string DistrictCode { get; set; }
        public int? TenantId { get; set; }
        [StringLength(100)]
        public string WardCode { get; set; }
        public int? NumberFloor { get; set; }
        public bool IsManager { get; set; }
        public virtual long? ParentId { get; set; }
        [Required]
        [StringLength(95)]
        public  string Code { get; set; }

        [Required]
        [StringLength(128)]
        public  string DisplayName { get; set; }
        public BuildingByUserDto Urban { get; set; }
        public long? UrbanId { get; set; }

    }

}
