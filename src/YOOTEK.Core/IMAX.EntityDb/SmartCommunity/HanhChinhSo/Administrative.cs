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
    public enum AdministrativeState
    {
        Requesting = 1,
        Accepted = 2,
        Denied = 3,
        Cancel = 4,
        Expired = 5,
        Completed = 6
    }

    [Table("Administrative")]
    public class Administrative : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveOrganizationUnit
    {
        public int? TenantId { get; set; }
        public long? OrganizationUnitId { get; set; }
        public long ADTypeId { get; set; }
        public string Properties { get; set; }
        public AdministrativeState State { get; set; }
        public string DeniedReason { get; set; }
        public long? UserBillId { get; set; }
    }
}
