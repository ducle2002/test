using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Yootek.Organizations.Interface;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("CitizenVehicles")]
    public class CitizenVehicle : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveUrban, IMayHaveBuilding
    {   
        //public long Id { get; set; }
      
        [StringLength(256)]
        public string VehicleName { get; set; }
        public VehicleType VehicleType { get; set; }
        [StringLength(256)]
        public string? VehicleCode { get; set; }
        [StringLength(256)]
        public string ApartmentCode { get; set; }
        [StringLength(1000)]
        public string Description { get; set; }
        public long? CitizenTempId { get; set; }
        public int? TenantId { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public CitizenVehicleState? State { get; set; }
        public long? ParkingId { get; set; }
        public string CardNumber { get; set; }
        public string OwnerName { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int? Level { get; set; }
        public double? Cost { get; set; }
        public long? BillConfigId { get; set; }
        public string? ImageUrl { get; set; }

    }

    public enum CitizenVehicleState
    {
        WAITING = 1,
        ACCEPTED = 2,
        REJECTED = 3,
        OVERDUE = 4
    }
}
