using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Yootek.Common.Enum;
using Yootek.EntityDb;

namespace Yootek.Yootek.Services.Yootek.SmartCommunity.CitizenFee.Dto
{
    public class PayMonthlyUserBillsInput
    {
        [JsonPropertyName("userBills")]
        public List<PayUserBillDto> UserBills { get; set; }
        [JsonPropertyName("userBillDebts")]
        public List<PayUserBillDto> UserBillDebts { get; set; }
        [JsonPropertyName("prepaymentBills")]
        public List<PrepaymentBillDto> PrepaymentBills { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        [JsonPropertyName("amount")]
        public double Amount { get; set; }
        [JsonPropertyName("userBill")]
        public UserBill UserBill { get; set; }
        [Required]
        [JsonPropertyName("apartmentCode")]
        public string ApartmentCode { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [Required]
        [JsonPropertyName("period")]
        public DateTime Period { get; set; }
        [Required]
        [JsonPropertyName("method")]
        public UserBillPaymentMethod Method { get; set; }
        [JsonPropertyName("fileUrl")]
        public string FileUrl { get; set; }
        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; }
        [JsonPropertyName("status")]
        public UserBillPaymentStatus? Status { get; set; }
        [JsonPropertyName("creationTime")]
        public DateTime CreationTime { get; set; }
        public decimal BalanceAmount { get; set; }

    }

    public class ThirdPartyPayMonthlyUserBillsInput
    {
        [JsonPropertyName("userBills")]
        public List<PayUserBillDto> UserBills { get; set; }
        [JsonPropertyName("userBillDebts")]
        public List<PayUserBillDto> UserBillDebts { get; set; }
        [JsonPropertyName("prepaymentBills")]
        public List<PrepaymentBillDto> PrepaymentBills { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        [JsonPropertyName("amount")]
        public double Amount { get; set; }
        [JsonPropertyName("userBill")]
        public UserBill UserBill { get; set; }
        [Required]
        [JsonPropertyName("apartmentCode")]
        public string ApartmentCode { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [Required]
        [JsonPropertyName("period")]
        public DateTime Period { get; set; }
        [Required]
        [JsonPropertyName("method")]
        public UserBillPaymentMethod Method { get; set; }
        [JsonPropertyName("fileUrl")]
        public string FileUrl { get; set; }
        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; }
        [JsonPropertyName("status")]
        public UserBillPaymentStatus? Status { get; set; }
        [JsonPropertyName("creationTime")]
        public DateTime CreationTime { get; set; }

    }

    public class PayUserBillDto
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
        [Range(0, double.MaxValue)]
        [JsonPropertyName("payAmount")]
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
        [JsonPropertyName("billType")]
        public BillType BillType { get; set; }
        [JsonPropertyName("totalIndex")]
        public decimal? TotalIndex { get; set; }
        [JsonPropertyName("totalLastCost")]
        public decimal? TotalLastCost { get; set; }
        [JsonPropertyName("lastCost")]
        public double? LastCost { get; set; }
        [JsonPropertyName("numberPeriod")]
        public int NumberPeriod { get; set; }
        [JsonPropertyName("vehicles")]
        public string Vehicles { get; set; }
        [JsonPropertyName("citizenTempId")]
        public long? CitizenTempId { get; set; }
        [JsonPropertyName("carNumber")]
        public int? CarNumber { get; set; }
        [JsonPropertyName("motorbikeNumber")]
        public int? MotorbikeNumber { get; set; }
        [JsonPropertyName("bicycleNumber")]
        public int? BicycleNumber { get; set; }
        [JsonPropertyName("otherVehicleNumber")]
        public int? OtherVehicleNumber { get; set; }
        [JsonPropertyName("pricesType")]
        public int? PricesType { get; set; }
    }

}
