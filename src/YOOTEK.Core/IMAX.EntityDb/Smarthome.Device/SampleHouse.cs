using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{

    [Table("SampleHouse")]
    public class SampleHouse : FullAuditedEntity<long>, IMayHaveTenant
    {
        public string Properties { get; set; }
        public int? TenantId { get; set; }
    }
}
