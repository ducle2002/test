using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    public enum Permission
    {
        CREATOR = 1,
        WORKER = 2,
        SUPERVISOR = 3,
    }
    public enum TabId
    {
        ASSIGNED = 1,
        RECEIVED = 2,
        FOLLOW = 3,
    }
    public enum EWorkStatus
    {
        DOING = 1,
        OVERDUE = 2,
        COMPLETED = 3,
        REJECTED = 4,
        CANCELED = 5,
    }

    [Table("WorkUsers")]
    public class WorkUser : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public long UserId { get; set; }
        public long WorkId { get; set; }
        public Permission Permission { get; set; }
        public EWorkStatus Status { get; set; }
    }
}
