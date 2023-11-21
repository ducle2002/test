using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations.Schema;

namespace IMAX.EntityDb
{
    [Table("WorkDetails")]
    public class WorkDetail : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public long WorkTypeId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
