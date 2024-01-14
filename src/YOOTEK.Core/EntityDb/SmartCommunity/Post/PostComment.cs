using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("PostComment")]
    public class PostComment : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public string Comment { get; set; }
        public long? ParentCommentId { get; set; }
        public long? PostId { get; set; }
    }
}
