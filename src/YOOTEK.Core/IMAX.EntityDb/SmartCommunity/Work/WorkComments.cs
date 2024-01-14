using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("WorkComments")]
    public class WorkComment : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public long WorkId { get; set; }
        public long UserId { get; set; }
        public string Content { get; set; }
        public List<string> ImageUrls { get; set; }
    }
}