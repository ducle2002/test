using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Organizations;
using Yootek.Organizations.Interface;

namespace Yootek.Yootek.EntityDb.Clb.Hotlines
{
    [Table("ClbHotlines")]
    public class ClbHotlines : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        [StringLength(256)]
        public string Name { get; set; }
        public string Properties { get; set; }
    }
}