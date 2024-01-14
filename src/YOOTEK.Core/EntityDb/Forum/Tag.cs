using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.Yootek.EntityDb.Forum
{
    [Table("ClbTags")]
    public class Tag:  FullAuditedEntity<long>, IMayHaveTenant
    {
        [StringLength(256)]
        public string Name { get; set; }
        public int? TenantId { get; set; }
    }
}
