using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace Yootek.EntityDb
{
    [Table("CarCard")]
    public class CarCard : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        [StringLength(1000)]
        public string VehicleCardCode { get; set; }
        public long? ParkingId { get; set; }
    }
}
