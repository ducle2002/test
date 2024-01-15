using Abp.AutoMapper;
using Yootek.Common;
using Yootek.EntityDb;
using Yootek.Organizations.Interface;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using static Yootek.YootekServiceBase;

namespace Yootek.Services
{
    [AutoMap(typeof(CitizenParking))]
    public class CitizenParkingDto : CitizenParking
    {
    }


    public class GetAllCitizenParkingInput : CommonInputDto
    {
        public OrderByCitizenParking? OrderBy { get; set; }
    }

    public enum OrderByCitizenParking
    {
        [FieldName("ParkingName")]
        PARKING_NAME = 1,
        [FieldName("ParkingCode")]
        PARKING_CODE = 2
    }
    public class ExportToExcelInput
    {
        [CanBeNull] public List<long> Ids { get; set; }
    }

    public class ExportToExcelOutputDto : CitizenParking
    {
        public string BuildingCode { get; set; }
        public string UrbanCode { get; set; }
    }

    public class ImportExcelInput : IMayHaveUrban, IMayHaveBuilding
    {
        public IFormFile Form { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
    }

}
