using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Organizations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Yootek.EntityDb.Yootek.DichVu.BusinessReg
{
    [Table("BusinessUnit")]
    public class BusinessUnit : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveOrganizationUnit
    {
        public int? TenantId { get; set; }
        public long? OrganizationUnitId { get; set; }
        public string BusinessName { get; set; }
        public string BusinessAddress { get; set; }
        public string DistrictId { get; set; }
        public string ProvinceId { get; set; }
        public string WardId { get; set; }
        public int BusinessType { get; set; }
        public string BusinessPhone { get; set; }
        public string BusinessEmail { get; set; }
        public string BusinessWebsite { get; set; }
        public string BusinessOwnerName { get; set; }
        public string BusinessDesc { get; set; }
        public string BusinessIMG { get; set; }
    }
}
