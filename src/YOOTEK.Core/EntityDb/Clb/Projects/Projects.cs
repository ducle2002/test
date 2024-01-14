using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;

namespace Yootek.Yootek.EntityDb.Clb.Projects
{
    [Table("ClbProjects")]
    public class Projects : FullAuditedEntity<long>
    {
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Code { get; set; }
        public EProjectType? Type { get; set; }
        public EProjectStatus? Status { get; set; }
        public int? ParentId { get; set; }
        public long? ManagerId { get; set; }
        public long? Capital { get; set; }
        public EProjectStage? Stage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
    
    public enum EProjectType
    {
        Internal = 1,
        External = 2
    }
    
    public enum EProjectStatus
    {
        Open = 1,
        Close = 2
    }
    
    public enum EProjectStage
    {
        Init = 1,
        Planning = 2,
        Execution = 3,
        Monitoring = 4,
        Closing = 5
    }
}