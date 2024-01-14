using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Yootek.Organizations.Interface;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("ClbForums")]
    public class Forum : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        [StringLength(1000)]
        public string ThreadTitle { get; set; }
        public int State { get; set; }
        public int? TypeTitle { get; set; }
        public int Type { get; set; }
        public string Content { get; set; }
        public string FileUrl { get; set; }
        [StringLength(1000)]
        public string Tags { get; set; }
        public long? OrganizationUnitId { get; set; }
        public long? TopicId { get; set; }
    }
}
