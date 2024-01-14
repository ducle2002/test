using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Yootek.EntityDb.Forum
{
    [Table("ClbForumTags")]
    public class ForumTag : FullAuditedEntity<long>, IMayHaveTenant
    {
        public long ForumId { get; set; }
        public long TagId { get; set; }
        public int? TenantId { get; set; }
    }
}
