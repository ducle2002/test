using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("Meters")]
    public class Meter : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        
        public string? ApartmentCode { get; set; }
        [StringLength(1000)]
        public string Name { get; set; }
        [StringLength(2000)]
        public string QrCode { get; set; }
        [StringLength(2000)]
        public string Code { get; set; }
        public long? MeterTypeId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
    }
}