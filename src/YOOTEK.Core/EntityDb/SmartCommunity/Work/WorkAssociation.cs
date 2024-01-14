using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("WorkAssociations")]
    public class WorkAssociation : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public long RelatedId { get; set; }
        public long WorkId { get; set; }
        public int RelationshipType { get; set; }
    }
}
