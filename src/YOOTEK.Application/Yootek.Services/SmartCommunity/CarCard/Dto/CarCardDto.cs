using System;
using System.ComponentModel.DataAnnotations;
using Abp.AutoMapper;
using Abp.Domain.Entities;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Yootek.Common;
using Yootek.EntityDb;
using Yootek.Organizations.Interface;

namespace Yootek.Services
{
    [AutoMap(typeof(CarCard))]
    public class CarCardDto
    {
        public CitizenVehicleState? State { get; set; }
        public string VehicleCardCode { get; set; }
        public string BuildingName { get; set; }
        public string UrbanName { get; set; }
        public string ApartmentCode { get; set; }
        public string VehicleName { get; set; }
        public string VehicleCode { get; set; }
        public string OwnerName { get; set; }
        public VehicleType? VehicleType { get; set; }
        public string ParkingName { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public double? Cost { get; set; }
        public string Description { get; set; }
        public DateTime CreationTime { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public long? ParkingId { get; set; }
    }
    public class ImportCarCardExcelInput
    {
        public IFormFile File { get; set; }
    }
    [AutoMap(typeof(CarCard))]
    public class CreateCarCardDto
    {
        public string VehicleCardCode { get; set; }
        public long? ParkingId { get; set; }
    }
    public class GetAllCarCard : Entity<long>, IMayHaveUrban, IMayHaveBuilding
    {
        public DateTime CreationTime { get; set; }
        public CitizenVehicleState? State { get; set; }
        public string VehicleCardCode { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public string BuildingName { get; set; }
        public string UrbanName { get; set; }
        public string ApartmentCode { get; set; }
        public string VehicleName { get; set; }
        public string VehicleCode { get; set; }
        public string OwnerName { get; set; }
        public VehicleType? VehicleType { get; set; }
        public long? ParkingId { get; set; }
        public string ParkingName { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public double? Cost { get; set; }

    }
    [AutoMap(typeof(CarCard))]
    public class UpdateCarCardInput
    {
        public long Id { get; set; }
        [StringLength(1000)][CanBeNull] public string VehicleCardCode { get; set; }
        public long? ParkingId { get; set; }

    }
    public class CarCardInput : CommonInputDto
    {
        public DateTime? ToDay { get; set; }
        public CitizenVehicleState? State { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public int? Status { get; set; }
        public long? ParkingId { get; set; }

    }

    public class UpdateStatusCarCardInput
    {
        public string VehicleCardCode { get; set; }
        public CitizenVehicleState? State { get; set; }
    }

}
