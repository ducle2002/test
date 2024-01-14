using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("LikePost")]
    public class LikePost : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public int? StateLike { get; set; }
        public long? CommentId { get; set; }
        public long? PostId { get; set; }
    }
}
