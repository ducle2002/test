
using System;
using System.Collections.Generic;
using Abp.AutoMapper;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Yootek.Common;
using Yootek.EntityDb;
using Yootek.Organizations.Interface;
using static Yootek.YootekServiceBase;

namespace Yootek.Services
{
    [AutoMap(typeof(CitizenVehicle))]
    public class CitizenVehicleDto : CitizenVehicle
    {
        public string CitizenName { get; set; }

        //public string VehicleCode { get; set; }
    }

    public class CitizenVehicleExcelOutputDto : CitizenVehicleDto
    {
        public string UrbanCode { get; set; }
        public string BuildingCode { get; set; }
        public string ParkingCode { get; set; }
        public string BillConfigCode { get; set; }

    }

    public class GetAllCitizenVehicleInput : CommonInputDto
    {
        public VehicleType? VehicleType { get; set; }

        public string ApartmentCode { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public CitizenVehicleState? State { get; set; }

    }

    [AutoMap(typeof(CitizenVehicle))]
    public class CitizenVehicleByApartmentDto
    {
        public long Id { get; set; }
        public string VehicleName { get; set; }
        public VehicleType VehicleType { get; set; }
        public string VehicleCode { get; set; }
        public string ApartmentCode { get; set; }
        public long? ParkingId { get; set; }
        public string CardNumber { get; set; }
        public string OwnerName { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public long? BillConfigId { get; set; }
        public double? Cost { get; set; }
    }

    public class GetAllVehicleByApartmentInput : CommonInputDto, IMayHaveUrban, IMayHaveBuilding
    {
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public string? ApartmentCode { get; set; }
        public CitizenVehicleState? State { get; set; }
        public VehicleType? VehicleType { get; set; }
        public OrderByVehicleByApartment? OrderBy { get; set; }
    }
    public enum OrderByVehicleByApartment
    {
        [FieldName("ApartmentCode")]
        APARTMENT_CODE = 1,
        [FieldName("VehicleCode")]
        VEHICLE_CODE = 2,
        [FieldName("VehicleName")]
        VEHICLE_NAME = 3
    }
    public class GetVehicleByApartmentCode : CommonInputDto
    {
        public string ApartmentCode { get; set; }
    }
    public class RegisterCitizenVehicleInput : IMayHaveUrban, IMayHaveBuilding
    {
        public List<CitizenVehicleDto> ListVehicle { get; set; }
        public string ApartmentCode { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public long? CitizenTempId { get; set; }
        public string Description { get; set; }
    }
    public class ApproveCitizenVehicleInsertDto
    {
        public long Id { get; set; }
        public CitizenVehicleState State { get; set; }
    }

    public class CreateOrUpdateVehicleByApartmentDto : IMayHaveUrban, IMayHaveBuilding
    {
        public string ApartmentCode { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public string Description { get; set; }
        public string OwnerName { get; set; }
        public List<CitizenVehicleDto> Value { get; set; }
        public List<long> DeleteList { get; set; }
        //public DateTime? RegistrationDate { get; set; }
        //public DateTime? ExpirationDate { get; set; }
    }

    public class ExportVehicleToExcelInput
    {
        [CanBeNull] public List<string> ApartmentCodes { get; set; }
    }

    public class ImportVehicleInput
    {
        public IFormFile Form { get; set; }
    }
    public class ImportVehicleExcelInput
    {
        public IFormFile File { get; set; }

    }

    public class UpdateCostVehicleDto
    {
        public string ApartmentCode { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public long? BillConfigId { get; set; }
    }
    public class UpdateVehicleApproval
    {
        public string CardNumber { get; set; }
        public long? ParkingId { get; set; }
        public long? Id { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public double? Cost { get; set; }
    }

    public class TotalVehiclesApartment
    {
        public string ApartmentCode { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public long? BillConfigId { get; set; }
        public VehicleType? VehicleType { get; set; }
    }
    public class GetAllParkingPrices
    {
        public long? ParkingId { get; set; }
    }
}
