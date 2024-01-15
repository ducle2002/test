using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Yootek.Authorization.Roles;

namespace Yootek.Organizations.Dto
{
    [AutoMap(typeof(Role))]
    public class OrganizationUnitRoleListDto : EntityDto<long>
    {
        public string DisplayName { get; set; }

        public string Name { get; set; }

        public DateTime AddedTime { get; set; }
    }
}