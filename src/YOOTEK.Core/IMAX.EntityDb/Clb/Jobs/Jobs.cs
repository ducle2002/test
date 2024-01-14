using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;

namespace Yootek.Yootek.EntityDb.Clb.Jobs
{
    [Table("ClbJobs")]
    public class Jobs : FullAuditedEntity<long>
    {
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public long? Salary { get; set; }
        public string Position { get; set; }
        public string? Requirement { get; set; }
        public string Benefit { get; set; }
        public string EnterpriseName { get; set; }
        public string EnterpriseAddress { get; set; }
        public string? EnterprisePhone { get; set; }
        public string? EnterpriseEmail { get; set; }
        public EJobsType? Type { get; set; }
        public EJobsStatus? Status { get; set; }
    }
    
    public enum EJobsType
    {
        FullTime = 1,
        PartTime = 2,
        Intern = 3
    }
    
    public enum EJobsStatus
    {
        Open = 1,
        Close = 2
    }
}