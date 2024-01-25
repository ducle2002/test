using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Yootek.Organizations.Interface;

namespace Yootek.EntityDb
{
    [Table("CarCard")]
    public class CarCard : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveUrban, IMayHaveBuilding
    {
        public int? TenantId { get; set; }
        [StringLength(1000)]
        public string VehicleCardCode { get; set; }
        public CarCardType Status { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
    }
    public enum CarCardType
    {
        WaitingActivation = 1,
        Activated = 2,
        Cancelled = 3,
    }
}
