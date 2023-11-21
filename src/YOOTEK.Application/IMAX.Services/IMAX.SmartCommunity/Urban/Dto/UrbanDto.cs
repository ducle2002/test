using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Domain.Entities;
using IMAX.Common;
using IMAX.Organizations;
using IMAX.Services.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IMAX.Common.Enum.CommonENum;
using static IMAX.IMAXServiceBase;

namespace IMAX.Services
{
    [AutoMap(typeof(AppOrganizationUnit))]
    public class TenantProjectDto : AppOrganizationUnit
    {
    }

    [AutoMap(typeof(AppOrganizationUnit))]
    public class UrbanDto : Entity<long>
    {
        public string ProjectCode { get; set; }
        public APP_ORGANIZATION_TYPE Type { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string ProvinceCode { get; set; }
        public string DistrictCode { get; set; }
        public string WardCode { get; set; }
        public string DisplayName { get; set; }
        public int? TenantId { get; set; }
        public long? ParentId { get; set; }
        public string Code { get; set; }
        public int NumberBuilding { get; set; }
        public int NumberCitizen { get; set; }
    }

    [AutoMap(typeof(AppOrganizationUnit))]
    public class UrbanGetAllDto : Entity<long>
    {
        public string Address { get; set; }
        public string ImageUrl { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string ProjectCode { get; set; }
        public int NumberBuilding { get; set; }
        public int NumberCitizen { get; set; }
    }


    public class GetAllUrbansInput : CommonInputDto
    {
        public long Id { get; set; }
        public OrderByUrban? OrderBy { get; set; }
    }

    public enum OrderByUrban
    {
        [FieldName("DisplayName")]
        DISPLAY_NAME = 1,
        [FieldName("ProjectCode")]
        PROJECT_CODE = 2
    }

    public class GetTenantProjectInput : CommonInputDto
    {
        public long Id { get; set; }
        public APP_ORGANIZATION_TYPE Type { get; set; }
        public int? FormId { get; set; }
        public DateTime? FromDay { get; set; }
        public DateTime? ToDay { get; set; }
        public int? State { get; set; }
        public long? ParentId { get; set; }
        public long? OrganizationId { get; set; }
        public bool IsManager { get; set; }
    }
}
