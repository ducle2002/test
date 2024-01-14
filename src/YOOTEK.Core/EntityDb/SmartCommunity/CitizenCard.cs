using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Organizations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.EntityDb
{

    public class CitizenCard : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveOrganizationUnit
    {
        public int? TenantId { get; set; }
        public long? OrganizationUnitId { get; set; }
        public long CitizenCode { get; set; }

        public bool IsLocked { get; set; }
    }
}
