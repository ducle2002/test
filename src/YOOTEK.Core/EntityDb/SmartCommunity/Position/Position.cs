using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Organizations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.EntityDb
{
    [Table("Position")]
    public class Position : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveOrganizationUnit
    {
        [StringLength(256)]
        public string Name { get; set; }
        public string Description { get; set; }
        public int? TenantId { get; set; }
        [StringLength(256)]
        public string DisplayName { get; set; }
        [StringLength(128)]
        public string? Code { get; set; }
        public long? OrganizationUnitId { get; set; }

    }

}
