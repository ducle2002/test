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
    [Table("DepartmentOrganizationUnits")]
    public class DepartmentOrganizationUnit : CreationAuditedEntity<long>, IMayHaveTenant, ISoftDelete
    {
        public long DeptId { get; set; }
        public long OrganizationUnitId { get; set; }
        public int? TenantId { get; set; }
        public bool IsDeleted { get; set; }
    }
}
