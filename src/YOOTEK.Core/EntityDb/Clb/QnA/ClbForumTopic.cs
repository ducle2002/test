using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace Yootek.Yootek.EntityDb.Clb.QnA
{
    [Table("ClbForumTopics")]
    public class ClbForumTopic : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}