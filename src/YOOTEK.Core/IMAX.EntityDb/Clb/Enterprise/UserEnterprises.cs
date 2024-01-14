using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using JetBrains.Annotations;

namespace Yootek.Yootek.EntityDb.Clb.Enterprise
{
    [Table("ClbUserEnterprises")]
    public class UserEnterprises : FullAuditedEntity<long>
    {
        public int? TenantId { get; set; }
        public long EnterpriseId { get; set; }
        public long MemberId { get; set; }
        public UserEnterpriseRole Role { get; set; }
        public UserEnterpriseStatus Status { get; set; } = UserEnterpriseStatus.Active;
        public string? Description { get; set; }
    }
    
    public enum UserEnterpriseRole
    {
        President = 1,
        VicePresident = 2,
        Manager = 3,
        Employee = 4,
        Intern = 5,
        Other = 6
    }
    
    public enum UserEnterpriseStatus
    {
        Active = 1,
        Inactive = 2
    }
}