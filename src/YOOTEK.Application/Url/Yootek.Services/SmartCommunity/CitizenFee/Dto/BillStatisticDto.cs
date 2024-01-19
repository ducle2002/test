using Yootek.Common.Enum;
using Yootek.EntityDb;
using Yootek.Organizations.Interface;
using System;

namespace Yootek.Services
{
    public class BillStatisticDto
    {
    }

    public class PaymentStatisticDto : IMayHaveUrban, IMayHaveBuilding
    {
        public string ApartmentCode { get; set; }
        public DateTime Period { get; set; }
        public decimal TotalPaid { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public TypePayment? TypePayment { get; set; }
    }

    public class UserBillStatisticDto : IMayHaveUrban, IMayHaveBuilding
    {
        public string ApartmentCode { get; set; }
        public DateTime Period { get; set; }
        public double? LastCost { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public BillType BillType { get; set; }
        public UserBillStatus Status { get; set; }
        public decimal? DebtTotal { get; set; }

    }
    public class BillDebtStatisticDto : IMayHaveUrban, IMayHaveBuilding
    {
        public string ApartmentCode { get; set; }
        public DateTime Period { get; set; }
        public double? TotalPaid { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public double? TotalDebt { get; set; }
        public DebtState? State { get; set; }

    }
}
