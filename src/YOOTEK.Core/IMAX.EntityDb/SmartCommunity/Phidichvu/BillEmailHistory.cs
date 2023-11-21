using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using Abp.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IMAX.EntityDb
{
    [Table("BillEmailHistories")]
    public class BillEmailHistory: FullAuditedEntity<long>, IMayHaveTenant, IMayHaveOrganizationUnit
    {
        public long? CitizenTempId { get; set; }
        [StringLength(1000)]
        public string ApartmentCode { get; set; }
        public int? TenantId { get; set; }
        public DateTime Period { get; set; }
        public long? OrganizationUnitId { get; set; }
        public string EmailTemplate { get; set; }
    }
}
