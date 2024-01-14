using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Yootek.Organizations.Interface;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("ClbForums")]
    public class ForumPost : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        [StringLength(1000)]
        public string ThreadTitle { get; set; }
        public Guid Code { get; set; }
        public int State { get; set; }
        public string Content { get; set; }
        [MaxLength(10)]
        public string[] ImageUrls { get; set; }
        [MaxLength(10)]
        public string[] LinkUrls { get; set; }
        public long? TopicId { get; set; }
        public int? Type { get; set; }
        public long? GroupId { get; set; } 
    }

    public enum EForumPostType
    {
        Business = 1,
        Job = 2,
        Project = 3,
        Event = 4,
        Other = 5
    }
}
