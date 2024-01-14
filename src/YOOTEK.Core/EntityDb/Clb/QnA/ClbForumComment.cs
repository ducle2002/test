using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace Yootek.Yootek.EntityDb.Clb.QnA
{
    [Table("ClbForumComments")]
    public class ClbForumComment : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public string Comment { get; set; }
        public string FileUrl { get; set; }
        public long ForumId { get; set; }
        public bool? IsAdmin { get; set; }
    }
}