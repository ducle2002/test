using Abp.AutoMapper;
using Yootek.Organizations;
using Yootek.Organizations.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Yootek.Services.Yootek.SmartCommunity.Building.Dto
{
    [AutoMap(typeof(AppOrganizationUnit))]
    public class BuidingDto
    {
        public long Id { get; set; }
        public string DisplayName { get; set; }
        public string Code { get; set; }
        public long? ParentId { get; set; }
        public int? TenantId { get; set; }
        public string ProjectCode { get; set; }
        public APP_ORGANIZATION_TYPE Type { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string UrbanName { get; set; }
        public bool? IsManager { get; set; }
        public long? UrbanId { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public decimal? Area { get; set; }
        public int? NumberFloor { get; set; }
        public int? NumberCitizen { get; set; }
        public int? NumberApartment { get; set; }
        public string? BuildingType { get; set; }
        public string? ProvinceCode { get; set; }
        public string? DistrictCode { get; set; }
        public string? WardCode { get; set; }

    }

    [AutoMap(typeof(AppOrganizationUnit))]
    public class BuidingAllDto: IMayHaveUrban, IMayHaveBuilding
    {
        public long Id { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public string DisplayName { get; set; }
        public string ProjectCode { get; set; }
        public string ImageUrl { get; set; }
        public string UrbanName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public decimal? Area { get; set; }
        public int? NumberFloor { get; set; }
        public int? NumberCitizen { get; set; }
        public int? NumberApartment { get; set; }
        public string? BuildingType { get; set; }
        public string? ProvinceCode { get; set; }
        public string? DistrictCode { get; set; }
        public string? WardCode { get; set; }

    }

}
