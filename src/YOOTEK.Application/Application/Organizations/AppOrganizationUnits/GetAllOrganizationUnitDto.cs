using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Yootek.Organizations;
using Yootek.Organizations.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Application.Organizations.AppOrganizationUnits
{
    [AutoMap(typeof(AppOrganizationUnit))]
    public class GetAllOrganizationUnitDto: AuditedEntityDto<long>
    {
        public long? ParentId { get; set; }

        public string Code { get; set; }

        public string DisplayName { get; set; }

        public int MemberCount { get; set; }
        public int RoleCount { get; set; }
        public string Description { get; set; }
        public string ProjectCode { get; set; }
        public string ImageUrl { get; set; }
        public int? TenantId { get; set; }
        public bool IsManager { get; set; }
        public Guid? GroupId { get; set; }
        public APP_ORGANIZATION_TYPE[] Types { get; set; }
        public List<OrganizationUnitDto> OrganizationUnitTypes { get; set; }
    }
}
