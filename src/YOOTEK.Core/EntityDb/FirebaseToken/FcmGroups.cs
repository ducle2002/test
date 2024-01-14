using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("FcmGroups")]
    public class FcmGroups : FullAuditedEntity<long>, IMayHaveTenant
    {
        public string GroupName { get; set; }
        public string NotificationKey { get; set; }
        public int? TenantId { get; set; }
    }
}