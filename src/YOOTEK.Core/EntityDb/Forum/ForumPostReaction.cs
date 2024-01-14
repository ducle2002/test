using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;

namespace Yootek.Yootek.EntityDb.Forum
{
    [Table("ClbForumPostReactions")]
    public class ForumPostReaction : FullAuditedEntity<long>
    {
        public int? TenantId { get; set; }
        public long PostId { get; set; }
        public long? CommentId { get; set; }
        public EForumReactionType Type { get; set; }
    }
    
    public enum EForumReactionType
    {
        None = 0,
        Like = 1,
        Dislike = 2,
    }
}