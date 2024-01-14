using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Yootek.Common.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yootek.Common.Enum;

namespace Yootek.EntityDb
{
    [Table("MeterTypes")]
    public class MeterType : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        [StringLength(512)]
        public string Name { get; set; }
        [StringLength(512)]
        public string Description { get; set; }
        public BillType BillType { get; set; }
    }
}