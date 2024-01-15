using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Yootek.Common.Enum;
using Yootek.EntityDb;

namespace Yootek.Yootek.Services.Yootek.SmartCommunity.CitizenFee.Dto
{
    public class PayMonthlyUserBillsInput
    {
        public List<PayUserBillDto> UserBills { get; set; }
        public List<PayUserBillDto> UserBillDebts { get; set; }
        public List<PrepaymentBillDto> PrepaymentBills { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public double Amount { get; set; }
        public UserBill UserBill { get; set; }
        [Required]
        public string ApartmentCode { get; set; }
        public string Description { get; set; }
        [Required]
        public DateTime Period { get; set; }
        [Required]
        public UserBillPaymentMethod Method { get; set; }
        public string FileUrl { get; set; }
        public string ImageUrl { get; set; }
        public UserBillPaymentStatus? Status { get; set; }
        public DateTime CreationTime { get; set; }

    }

    public class PayUserBillDto
    {
        public long Id { get; set; }
        [Range(0, double.MaxValue)]
        public double PayAmount { get; set; }
    }

    public class VerifyPaymentUserBill
    {
        public long[] UserBillIds { get; set; }
        public double? Amount { get; set; }
        public DateTime? Period { get; set; }
        public string ApartmentCode { get; set; }
        public string Description { get; set; }
        public UserBillPaymentMethod? Method { get; set; }
        public string FileUrl { get; set; }
        public string ImageUrl { get; set; }
        public List<PrepaymentBillDto> PrepaymentBills { get; set; }
        public long[] BillDebtIds { get; set; }
        public UserBill UserBill { get; set; }
    }

    public class PrepaymentBillDto
    {
        public BillType BillType { get; set; }
        public decimal? TotalIndex { get; set; }
        public decimal? TotalLastCost { get; set; }
        public double? LastCost { get; set; }
        public int NumberPeriod { get; set; }
        public string Vehicles { get; set; }
        public long? CitizenTempId { get; set; }
        public int? CarNumber { get; set; }
        public int? MotorbikeNumber { get; set; }
        public int? BicycleNumber { get; set; }
        public int? OtherVehicleNumber { get; set; }
        public int? PricesType { get; set; }
    }

}
