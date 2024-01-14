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
    [Table("ClbForumTopics")]
    public class ForumTopic : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        [StringLength(256)]
        public string Name { get; set; }
        [StringLength(1000)]
        public string ImageUrl { get; set; }
        public string Description { get; set; }
    }
}
