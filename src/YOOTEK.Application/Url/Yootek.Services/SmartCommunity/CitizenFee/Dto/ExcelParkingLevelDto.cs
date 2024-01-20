using Yootek.EntityDb;
using Yootek.Organizations.Interface;
using System;

namespace Yootek.Yootek.Services.Yootek.SmartCommunity.CitizenFee.Dto
{
    public class ExcelParkingLevelDto : IMayHaveUrban, IMayHaveBuilding
    {
        public DateTime Period { get; set; }
        public string ApartmentCode { get; set; }
        public DateTime? DueDate { get; set; }
        public double? LastCost { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public string VehicleName { get; set; }
        public int Level { get; set; }
        public VehicleType VehicleType { get; set; }
        public string VehicleCode { get; set; }
        public string Description { get; set; }
        public long? ParkingId { get; set; }
        public string CardNumber { get; set; }
        public string CustomerName { get; set; }
        public int? MonthNumber { get; set; }
    }
}
