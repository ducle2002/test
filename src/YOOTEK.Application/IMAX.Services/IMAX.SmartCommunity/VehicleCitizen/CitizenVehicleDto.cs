﻿
using Abp.AutoMapper;
using IMAX.Common;
using IMAX.EntityDb;
using IMAX.Organizations.Interface;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using static IMAX.IMAXServiceBase;

namespace IMAX.Services
{
    [AutoMap(typeof(CitizenVehicle))]
    public class CitizenVehicleDto : CitizenVehicle
    {
        public string CitizenName { get; set; }
    }

    public class CitizenVehicleExcelOutputDto : CitizenVehicleDto
    {
        public string UrbanCode { get; set; }
        public string BuildingCode { get; set; }
        public string ParkingName { get; set; }
    }

    public class GetAllCitizenVehicleInput : CommonInputDto
    {
        public VehicleType VehicleType { get; set; }

        public string ApartmentCode { get; set; }
    }

    [AutoMap(typeof(CitizenVehicle))]
    public class CitizenVehicleByApartmentDto
    {
        public string VehicleName { get; set; }
        public VehicleType VehicleType { get; set; }
        public string VehicleCode { get; set; }
        public string ApartmentCode { get; set; }
        public long? ParkingId { get; set; }
        public string CardNumber { get; set; }
        public string OwnerName { get; set; }
    }

    public class GetAllVehicleByApartmentInput : CommonInputDto, IMayHaveUrban, IMayHaveBuilding
    {
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
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

}
