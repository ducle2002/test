using Abp.AutoMapper;
using Yootek.App.ServiceHttpClient.Dto.Yootek.SmartCommunity;
using Yootek.Common;
using Yootek.EntityDb;
using Yootek.Organizations.Interface;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using static Yootek.YootekServiceBase;

namespace Yootek.Services
{
    [AutoMap(typeof(Parking))]
    public class ParkingDto : Parking
    {
        public QRObjectDto QRObject { get; set; }
        public string QRAction { get; set; }
    }

    [AutoMap(typeof(Parking))]
    public class CreateParkingDto : IMayHaveUrban, IMayHaveBuilding
    {
        [CanBeNull] public string Name { get; set; }
        [CanBeNull] public string Description { get; set; }
        public string Position { get; set; }
        public ParkingVehicleType? VehicleType { get; set; }
        public ParkingStatus? Status { get; set; }
        [CanBeNull] public string[] ListImageUrl { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public long? CitizenParkingId { get; set; }
    }

    [AutoMap(typeof(Parking))]
    public class UpdateParkingDto : CreateParkingDto
    {
        public long Id { get; set; }
    }

    public class GetListParkingDto : CommonInputDto, IMayHaveUrban, IMayHaveBuilding
    {
        [CanBeNull] public string Search { get; set; }
        public ParkingVehicleType? VehicleType { get; set; }
        public ParkingStatus? Status { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public long? CitizenParkingId { get; set; }

        public OrderByListParking OrderBy { get; set; }
    }
    public enum OrderByListParking
    {
        [FieldName("Name")]
        NAME = 1,
        [FieldName("Position")]
        POSITION = 2
    }
    public class ParkingExcelExportInput
    {
        [CanBeNull] public List<long> Ids { get; set; }
    }

    public class UploadParkingExcel
    {
        public IFormFile File { get; set; }
    }
}