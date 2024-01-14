using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Yootek.Organizations.Interface;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("CitizenParkings")]
    public class CitizenParking : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveUrban, IMayHaveBuilding
    {
        [StringLength(1000)]
        public string ParkingName { get; set; }
        [StringLength(256)]
        public string ParkingCode { get; set; }
        [StringLength(1000)]
        public string Description { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public int? TenantId { get; set; }
    }

}
