using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Organizations;
using IMAX.Organizations.Interface;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IMAX.EntityDb
{
    [Table("UserFeedbacks")]
    public class CitizenReflect : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveOrganizationUnit, IMayHaveUrban, IMayHaveBuilding
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
        [StringLength(256)]
        public string AddressFeeder { get; set; }
        [StringLength(256)]
        public string? ApartmentCode { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
    }
}
