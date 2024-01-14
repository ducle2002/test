using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Organizations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Yootek.EntityDb.Yootek.Metrics
{
    [Table("HomeMeters")]
    public class HomeMeter : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveOrganizationUnit
    {
        public string NameElectricMeter { get; set; }
        public string NameWaterMeter { get; set; }
        public decimal? ElectricMeterCurrentValue { get; set; }
        public decimal? WaterMeterCurrentValue { get; set; }
        public bool WaterMeterState { get; set; }
        public bool ElectricMeterState { get; set; }
        public string ApartmentCode { get; set; }
        public DateTime? Period { get; set; }
        public int? TenantId { get; set; }
        public long? OrganizationUnitId { get; set; }
    }
}
