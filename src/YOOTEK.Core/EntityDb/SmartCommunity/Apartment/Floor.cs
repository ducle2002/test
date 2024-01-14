using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Yootek.Organizations.Interface;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("Floors")]
    public class Floor : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveUrban, IMayHaveBuilding
    {
        public int? TenantId { get; set; }
        [StringLength(256)]
        public string Code { get; set; }
        [StringLength(1000)]
        public string DisplayName { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        [StringLength(2001)]
        public string Description { get; set; }
    }
}