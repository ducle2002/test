using Abp.AutoMapper;
using Yootek.EntityDb;
using Yootek.Organizations.Interface;
using System;
using System.ComponentModel.DataAnnotations;

namespace Yootek.Services
{
    [AutoMap(typeof(CitizenReflect))]
    public class CreateOrUpdateCitizenReflectInput : IMayHaveBuilding, IMayHaveUrban
    {
        public long Id { get; set; }
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
        [StringLength(2000)]
        public string? RatingContent { get; set; }
        public long? OrganizationUnitId { get; set; }
        public bool? CheckVerify { get; set; }
        [StringLength(256)]
        public string Phone { get; set; }
        [StringLength(256)]
        public string NameFeeder { get; set; }
        public int? CountUnreadComment { get; set; }
        public long? HandleUserId { get; set; }
        public long? HandleOrganizationUnitId { get; set; }
        [StringLength(1000)]
        public string ReflectReport { get; set; }
        [StringLength(1000)]
        public string ReportName { get; set; }
        public string? ApartmentCode { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string ImageUrl { get; set; }
        public int? CountAllComment { get; set; }
        public string Note { get; set; }
        public string FileOfNote { get; set; }
        public long? BuildingId { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string OrganizationUnitName { get; set; }
        public string HandlerName { get; set; }
        public long? UrbanId { get; set; }
    }
}
