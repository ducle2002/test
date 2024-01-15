using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace Yootek.Yootek.EntityDb.Clb.Event
{
    [Table("ClbEventComments")]
    public class ClbEventComment : FullAuditedEntity<long>, IMayHaveTenant
    {
        public string Comment { get; set; }
        public bool? IsLike { get; set; }
        public int? TenantId { get; set; }
        public long EventId { get; set; }
        public int? Type { get; set; }
    }
}