using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("DocumentTypes")]
    public class DocumentTypes : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        [StringLength(512)]
        public string DisplayName { get; set; }
        [StringLength(2000)]
        public string Icon { get; set; }
        [StringLength(2000)]
        public string Description { get; set; }
        public DocumentScope? Scope { get; set; }
        public bool IsGlobal { get; set; }    
    }
}