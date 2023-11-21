using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IMAX.EntityDb
{
    public enum VehicleType
    {
        Other = 0,
        Car = 1,
        Motorbike = 2,
        Bicycle = 3,
    }

    [Table("UserVehicle")]
    public class UserVehicle : FullAuditedEntity<long>, IMayHaveTenant
    {
        [StringLength(256)]
        public string VehicleName { get; set; }
        public VehicleType VehicleType { get; set; }
        [StringLength(1000)]
        public string ImageUrl { get; set; }
        [StringLength(256)]
        public string VehicleCode { get; set; }
        [StringLength(256)]
        public string ApartmentCode { get; set; }
        [StringLength(1000)]
        public string Description { get; set; }
        public bool IsDefault { get; set; }
        public int? TenantId { get; set; }
    }
}
