using Abp.AutoMapper;
using Yootek.Common;
using Yootek.EntityDb;
using JetBrains.Annotations;

namespace Yootek.Services
{
    public class CreateOwnerUserParkingDto
    {
        public long ParkingId { get; set; }
        public long? VehicleId { get; set; }
        [CanBeNull] public string Properties { get; set; }
        [CanBeNull] public string[] ListImageUrl { get; set; }
    }

    [AutoMap(typeof(UserParking))]
    public class UpdateOwnerUserParkingDto
    {
        public long Id { get; set; }
        public long? VehicleId { get; set; }
        [CanBeNull] public string Properties { get; set; }
        [CanBeNull] public string[] ListImageUrl { get; set; }
    }

    [AutoMap(typeof(UserParking))]
    public class OwnerUserParkingDto : UserParking
    {
        public Parking ParkingInfo { get; set; }
        public string VehicleName { get; set; }
        public string VehicleCode { get; set; }
        public string ApartmentCode { get; set; }
    }

    public class GetListOwnerParkingInput: CommonInputDto
    {
        public string ApartmentCode { get; set; }
    }
}
