using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Yootek.Organizations.Interface;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("Documents")]
    public class Documents : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveUrban, IMayHaveBuilding
    {
        public int? TenantId { get; set; }
        [StringLength(1000)]
        public string DisplayName { get; set; }
        [StringLength(2000)]
        public string FileUrl { get; set; }
        [StringLength(2000)]
        public string Link { get; set; }
        public long DocumentTypeId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
    }

    public enum DocumentScope
    {
        Web = 1,
        App = 2,
        All = 3
    }
}