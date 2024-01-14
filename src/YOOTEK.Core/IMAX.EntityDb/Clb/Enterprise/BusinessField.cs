using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;

namespace Yootek.Yootek.EntityDb.Clb.Enterprise
{
    [Table("ClbBusinessField")]
    public class BusinessField : FullAuditedEntity<long>
    {
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}