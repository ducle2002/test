using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Yootek.Authorization.Users;
using Newtonsoft.Json;

namespace Yootek.Users.Dto
{
    [AutoMapFrom(typeof(User))]
    public class UserDto : EntityDto<long>
    {

        [Required]
        [StringLength(AbpUserBase.MaxUserNameLength)]
        public string UserName { get; set; }
        public string Name { get; set; }

        public string Surname { get; set; }
        public string EmailAddress { get; set; }

        public bool IsActive { get; set; }
        public string FullName { get; set; }
        //[JsonProperty]
        public string PhoneNumber { get; set; }

        public DateTime? LastLoginTime { get; set; }
        public DateTime CreationTime { get; set; }
        public string[] RoleNames { get; set; }

        //Thong tin dan cu
        [StringLength(1000)]
        public string HomeAddress { get; set; }
        [StringLength(1000)]
        public string AddressOfBirth { get; set; }
        public DateTime? DateOfBirth { get; set; }
        [StringLength(128)]
        public string Gender { get; set; }
        [StringLength(256)]
        public string Nationality { get; set; }

        public virtual Guid? ProfilePictureId { get; set; }

        [StringLength(256)]
        public string ImageUrl { get; set; }
        public string CoverImageUrl { get; set; }

        public string IdentityNumber { get; set; }
    }

    public class GetStatisticsUserInput
    {
        //public long? OrganizationUnitId { get; set; }
        public int NumberRange { get; set; }
        public QueryCaseUserStatistics QueryCase { get; set; }
    }

    public enum QueryCaseUserStatistics
    {
        ByYear = 1,
        ByMonth = 2,
        ByWeek = 3,
        ByDay = 4
    }
}
