using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using IMAX.Authorization.Roles;

namespace IMAX.Organizations.Dto
{
    [AutoMap(typeof(Role))]
    public class OrganizationUnitRoleListDto : EntityDto<long>
    {
        public string DisplayName { get; set; }

        public string Name { get; set; }

        public DateTime AddedTime { get; set; }
    }
}