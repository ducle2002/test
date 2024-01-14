using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("Post")]
    public class Post : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public string ImageContent { get; set; }
        public string VideoContent { get; set; }
        public int? EmotionState { get; set; }
        public string ContentPost { get; set; }
        public int State { get; set; }
        public int? Type { get; set; }
        public long? FeedbackId { get; set; }
        public int? Status { get; set; }
    }
}
