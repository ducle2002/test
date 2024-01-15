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
    [Table("DepartmentUsers")]
    public class OrganizationStructureDeptUser : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public long UserId { get; set; }
        public long DeptId { get; set; }
    }
}
