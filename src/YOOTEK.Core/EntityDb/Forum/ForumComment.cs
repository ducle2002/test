using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("ClbForumComments")]
    public class ForumComment : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public string Comment { get; set; }
        public List<string> FileUrls { get; set; }
        public List<string> LinkUrls { get; set; }
        public long ForumPostId { get; set; }
        public long? ReplyId { get; set; }
    }
}
