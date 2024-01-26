using System;
using Yootek.Organizations.Interface;

namespace Yootek.Services.Dto
{
    #region input

    public class GetStatisticTotalInput : IMayHaveUrban, IMayHaveBuilding
    {
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
    }
    public class GetStatisticBillBuildingInput
    {
        public long UrbanId { get; set; }
    }

    public class GetTotalStatisticUserBillInput
    {
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public DateTime? Period { get; set; }
        public DateTime? FromDay { get; set; }
        public DateTime? ToDay { get; set; }
    }

    #endregion

    #region grouped dto
    public class StatisticBillTenantDto
    {
        public long TenantId { get; set; }
        public string DisplayName { get; set; }
    }
    public class StatisticBillUrbanDto
    {
        public long UrbanId { get; set; }
        public string DisplayName { get; set; }
    }
    public class StatisticBillBuildingDto
    {
        public long BuildingId { get; set; }
        public string DisplayName { get; set; }
    }
    #endregion

    #region response
    public class DataStatisticBillTenantDto
    {
        // Tổng tiền hóa đơn
        public double TotalCost { get; set; }
        public double TotalCostElectric { get; set; }
        public double TotalCostWater { get; set; }
        public double TotalCostManager { get; set; }
        public double TotalCostOther { get; set; }
        public double TotalCostResidence { get; set; }
        public double TotalCostParking { get; set; }

        // Tổng tiền Công nợ
        public decimal TotalDebt { get; set; }
        public decimal TotalDebtElectric { get; set; }
        public decimal TotalDebtWater { get; set; }
        public decimal TotalDebtManager { get; set; }
        public decimal TotalDebtOther { get; set; }
        public decimal TotalDebtResidence { get; set; }
        public decimal TotalDebtParking { get; set; }

        // Tổng tiền hóa đơn đã thanh toán
        public double TotalPaid { get; set; }
        public double TotalPaidElectric { get; set; }
        public double TotalPaidWater { get; set; }
        public double TotalPaidManager { get; set; }
        public double TotalPaidOther { get; set; }
        public double TotalPaidResidence { get; set; }
        public double TotalPaidParking { get; set; }


        // Tổng tiền thu 
        public double TotalPaymentIncome { get; set; }
        public double TotalPaymentWithDirect { get; set; }
        public double TotalPaymentWithBanking { get; set; }
        public double TotalPaymentWithMomo { get; set; }
        public double TotalPaymentWithVNPay { get; set; }
        public double TotalPaymentWithZaloPay { get; set; }

        // Các chỉ số khác
        public long TotalWIndex { get; set; }
        public long TotalEIndex { get; set; }
        public long CarNumber { get; set; }
        public long MotorbikeNumber { get; set; }
        public long BicycleNumber { get; set; }
    }
    public class DataStatisticBillUrbanDto
    {
        public long UrbanId { get; set; }
        public string DisplayName { get; set; }
        public double TotalCost { get; set; }
        public double TotalDebt { get; set; }
        public double TotalPaid { get; set; }
        public double TotalCostElectric { get; set; }
        public double TotalCostWater { get; set; }
        public double TotalCarFee { get; set; }
        public double TotalManager { get; set; }
        public double TotalOther { get; set; }
        public long TotalWIndex { get; set; }
        public long TotalEIndex { get; set; }
        public long CarNumber { get; set; }
        public long MotorbikeNumber { get; set; }
        public long BicycleNumber { get; set; }
    }
    public class DataStatisticBillBuildingDto
    {
        public long BuildingId { get; set; }
        public string DisplayName { get; set; }
        public double TotalCost { get; set; }
        public double TotalDebt { get; set; }
        public double TotalPaid { get; set; }
        public double TotalCostElectric { get; set; }
        public double TotalCostWater { get; set; }
        public double TotalCarFee { get; set; }
        public double TotalManager { get; set; }
        public double TotalOther { get; set; }
        public long TotalWIndex { get; set; }
        public long TotalEIndex { get; set; }
        public long CarNumber { get; set; }
        public long MotorbikeNumber { get; set; }
        public long BicycleNumber { get; set; }
    }
    #endregion
}
