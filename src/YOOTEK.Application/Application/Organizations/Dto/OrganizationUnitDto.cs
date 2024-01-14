using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Yootek.Common;
using System;

namespace Yootek.Organizations.Dto
{
    [AutoMap(typeof(AppOrganizationUnit), typeof(CreateOrganizationUnitInput))]
    public class OrganizationUnitDto : AuditedEntityDto<long>
    {
        public long? ParentId { get; set; }

        public string Code { get; set; }

        public string DisplayName { get; set; }

        public int MemberCount { get; set; }

        public int RoleCount { get; set; }
        public int DepartmentCount { get; set; }
        public int UnitChargeCount { get; set; }
        public string Description { get; set; }
        public string ProjectCode { get; set; }
        public string ImageUrl { get; set; }
        public APP_ORGANIZATION_TYPE Type { get; set; }
        public int? TenantId { get; set; }
        public bool IsManager { get; set; }
        public Guid? GroupId { get; set; }
        public APP_ORGANIZATION_TYPE[] Types { get; set; }
    }

    public class OrganizationUnitInput : CommonInputDto
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
    public class OrganizationUnitByParent : CommonInputDto
    {
        public long? ParentId { get; set; }
        public APP_ORGANIZATION_TYPE? Type { get; set; }

    }
    public class OrganizationUnitByParentNotPagingInput
    {
        public long? ParentId { get; set; }
    }
}