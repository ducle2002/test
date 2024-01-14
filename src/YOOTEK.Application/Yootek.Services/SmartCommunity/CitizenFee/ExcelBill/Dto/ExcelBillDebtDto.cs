using Yootek.EntityDb;
using Yootek.Services.Dto;
using System.Collections.Generic;

namespace Yootek.Services.SmartCommunity.ExcelBill.Dto
{
    public class ExcelBillDebtDto
    {
        public List<UserBill> UserBills { get; set; }
        public List<UserBillPayment>? UserBillPayments { get; set; }
        public List<BillDebt>? BillDebts { get; set; }
    }
    /*[AutoMap(typeof(GetStatisticBillInput))]
    public class ExportStatisticBillInput
    {
        public int Year { get; set; }
        public FormIdStatisticScope FormIdScope { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
    }*/

    public class DataStatisticScope
    {
        public string ScopeName { get; set; }
        public Dictionary<string, DataStatisticBillTenantDto> Data { get; set; }
    }
}
