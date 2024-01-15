using Yootek.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.AutoMapper;

namespace Yootek.Yootek.Services.Yootek.SmartCommunity.Building.Dto
{
    [AutoMap(typeof(AppOrganizationUnit))]
    public class CreateBuildingDto
    {
        public string DisplayName { get; set; }
        public long? ParentId { get; set; }
        public string ProjectCode { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public decimal? Area { get; set; }
        public string? Address { get; set; }
        public string? ProvinceCode { get; set; }
        public string? DistrictCode { get; set; }
        public string? WardCode { get; set; }
        public int? NumberFloor { get; set; }
    }
}
