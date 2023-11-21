using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using IMAX.Authorization.Users;

namespace IMAX.Organizations.Dto
{
    [AutoMap(typeof(User))]
    public class OrganizationUnitUserListDto : EntityDto<long>
    {
        public string Name { get; set; }

        public string Surname { get; set; }

        public string UserName { get; set; }
        public string FullName { get; set; }
        public string? PositionName { get; set; }

        public string EmailAddress { get; set; }

        public Guid? ProfilePictureId { get; set; }

        public DateTime AddedTime { get; set; }
    }
}