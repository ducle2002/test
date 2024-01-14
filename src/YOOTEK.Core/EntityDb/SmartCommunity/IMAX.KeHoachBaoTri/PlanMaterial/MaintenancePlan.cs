using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.EntityDb
{
    [Table("MaintenancePlans")]

    public class MaintenancePlan : FullAuditedEntity<long>, IMayHaveTenant
    {
        [StringLength(1000)]
        public string Place { get; set; }

        [StringLength(1000)]
        public string Asset { get; set; }

        public int State { get; set; }
        public int? TenantId { get; set; }

        public DateTime ImplementDate { get; set; }
    }
}

