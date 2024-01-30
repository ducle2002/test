using Yootek.Common.Enum;
using Yootek.EntityDb;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Yootek.Services.SmartCommunity.ExcelBill.Dto
{
    public class ImportExcelBillInput
    {
        public IFormFile Form { get; set; }
        public BillType Type { get; set; }
        public ParkingBillType? ParkingType { get; set; }
        public List<BillConfig> Formulas { get; set; }
    }

    public enum ParkingBillType
    {
        Parking = 5,
        ParkingLevel = 6,
    }

    public class ImportExcelBillDebtInput
    {
        public IFormFile Form { get; set; }
    }

    public class BillProperites
    {
        public string customerName { get; set; }
        public long[] formulas { get; set; }
        public BillConfig[] formulaDetails { get; set; }
        public long? pricesType { get; set; }
        public BillConfig vehicleFormulaDetail { get; set; }


    }
    public class ListBillConfigProperties
    {
        public BillProperites? Properties { get; set; }
        public BillType? BillType { get; set; }

    }

    public class BillParkingProperties : BillProperites
    {
        public int pricesType { get; set; }
        public List<VehicleProperties> vehicles { get; set; }
    }

    public class BillProperty
    {
        public string customerName { get; set; }
        public List<long> formulas { get; set; }
        public List<BillConfig> formulaDetails { get; set; }
    }

    public class VehicleProperties
    {
        public string vehicleName { get; set; }
        public int vehicleType { get; set; }
        public string vehicleCode { get; set; }
        public string apartmentCode { get; set; }
        public long parkingId { get; set; }
        public string cardNumber { get; set; }
        public string ownerName { get; set; }
        public decimal cost { get; set; }
        public int level { get; set; }
    }
}
