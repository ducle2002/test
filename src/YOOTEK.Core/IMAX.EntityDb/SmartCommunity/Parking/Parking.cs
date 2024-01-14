using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Organizations;
using Yootek.Organizations.Interface;
using JetBrains.Annotations;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    public enum ParkingVehicleType
    {
        Other = 0,
        Car = 1,
        Motorbike = 2,
        Bicycle = 3,
    }

    public enum ParkingStatus
    {
        Other = 0,
        Free = 1,
        Occupied = 2,
    }

    [Table("Parking")]
    public class Parking : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveOrganizationUnit, IMayHaveUrban, IMayHaveBuilding
    {
        [CanBeNull] public string Name { get; set; }
        [CanBeNull] public string Description { get; set; }
        [StringLength(512)] public string QrCode { get; set; }
        public int? TenantId { get; set; }
        public string Position { get; set; }
        public ParkingVehicleType? VehicleType { get; set; }
        public ParkingStatus? Status { get; set; }
        [CanBeNull] public string ImageUrls { get; set; }

        [NotMapped]
        [CanBeNull]
        public string[] ListImageUrl
        {
            get
            {
                if (ImageUrls != null) return JsonConvert.DeserializeObject<string[]>(ImageUrls);
                return new string[] { };
            }
            set => ImageUrls = JsonConvert.SerializeObject(value);
        }

        public long? OrganizationUnitId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public long? CitizenParkingId { get; set; }
    }
}