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
    [Table("Departments")]
    public class OrganizationStructureDept: FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public string DisplayName { get; set; }
        public long? UnitId { get; set; }
        public string ImageUrl { get; set; }
        public long? ParentId { get; set; }
        public string Description { get; set; }
    }
}
