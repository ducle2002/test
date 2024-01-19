using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Yootek.EntityDb;
using System;
using System.ComponentModel.DataAnnotations;

namespace Yootek.Services
{
    [AutoMap(typeof(CitizenReflect))]
    public class GovernmentReflectDto : EntityDto<long>
    {
        [StringLength(2000)]
        public string Name { get; set; }
        public string Data { get; set; }
        [StringLength(2000)]
        public string FileUrl { get; set; }
        public int? Type { get; set; }
        public int? TenantId { get; set; }
        public DateTime? FinishTime { get; set; }
        public int? State { get; set; }
        public bool? IsPublic { get; set; }
        public int? Rating { get; set; }
        public string? RatingContent { get; set; }
        public long? OrganizationUnitId { get; set; }
        public string Phone { get; set; }
        public long UserId { get; set; }
        public string NameFeeder { get; set; }
        public bool CheckVerify { get; set; }
        public int? CountUnreadComment { get; set; }

        public string FullName { get; set; }
        public string UserName { get; set; }
        public string ImageUrl { get; set; }
        public int? CountAllComment { get; set; }
        public string Note { get; set; }
        public string FileOfNote { get; set; }
        public string ApartmentCode { get; set; }
        public string OrganizationUnitName { get; set; }
    }

}
