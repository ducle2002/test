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
    [Table("DepartmentToUnits")]
    public class DeptToUnit : FullAuditedEntity<long>, IMayHaveTenant
    {
        public long DeptId { get; set; }
        public long UnitId { get; set; }
        public int? TenantId { get; set; }
    }
    
    [Table("UnitToUnits")]
    public class UnitToUnit : FullAuditedEntity<long>, IMayHaveTenant
    {
        public long ParentId { get; set; }
        public long ChildId { get; set; }
        public int? TenantId { get; set; }
    }
}
