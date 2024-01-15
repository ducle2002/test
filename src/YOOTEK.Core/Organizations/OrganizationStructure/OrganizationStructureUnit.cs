using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Organizations.OrganizationStructure
{
    [Table("Units")]
    public class OrganizationStructureUnit : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public string DisplayName { get; set; }
        public string DeptIds { get; set; }
        public string ParentIds { get; set; }
    }
}
