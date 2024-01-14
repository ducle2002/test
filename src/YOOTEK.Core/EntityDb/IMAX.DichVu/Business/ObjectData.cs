using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Yootek.Organizations.Interface;
using System.ComponentModel.DataAnnotations;

namespace Yootek.EntityDb
{
    public class ObjectPartner : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveUrban, IMayHaveBuilding
    {
        public int? TenantId { get; set; }
        public int? SocialTenantId { get; set; }
        public int? Type { get; set; }
        public int? GroupType { get; set; }
        [StringLength(2000)]
        public string QueryKey { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string PropertyHistories { get; set; }
        public string Properties { get; set; }
        [StringLength(256)]
        public string Name { get; set; }
        [StringLength(256)]
        public string Owner { get; set; }
        [StringLength(2000)]
        public string Operator { get; set; }
        public int? Like { get; set; }
        public int? State { get; set; }
        public string StateProperties { get; set; }
        public int? CountRate { get; set; }
        public double? RatePoint { get; set; }
        public bool? IsDataStatic { get; set; }
        public bool? IsAdminCreate { get; set; }
        [StringLength(6)]
        public string DistrictId { get; set; }
        [StringLength(6)]
        public string ProvinceId { get; set; }
        [StringLength(6)]
        public string WardId { get; set; }

        [StringLength(500)]
        public string Address { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
    }
}
