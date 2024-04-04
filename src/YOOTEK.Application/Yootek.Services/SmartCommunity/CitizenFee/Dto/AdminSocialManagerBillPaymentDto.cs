using JetBrains.Annotations;
using System;
using Yootek.Common.Enum;
using Yootek.Common;
using YOOTEK.EntityDb;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using Yootek.Yootek.Services.Yootek.SmartCommunity.CitizenFee.Dto;
using Yootek.MultiTenancy;

namespace YOOTEK.Yootek.Services.SmartCommunity.CitizenFee.Dto
{
    public class GetAllBillPaymentByAdminSocialDto : CommonInputDto
    {
        [CanBeNull] public DateTime? Period { get; set; }
        public UserBillPaymentStatus? Status { get; set; }
        public UserBillPaymentMethod? Method { get; set; }
        public string ApartmentCode { get; set; }
        public bool IsAdvanced { get; set; }
        public bool? IsDeleted { get; set; }
        public int? TenantId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public DateTime? FromDay { get; set; }
        public DateTime? ToDay { get; set; }
        public DateTime? InDay { get; set; }
    }

    public class GetAllPaymentInput : CommonInputDto
    {
        public DateTime? Period { get; set; }
        public int? TenantId { get; set; }
        public int? MerchantId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public DateTime? FromDay { get; set; }
        public DateTime? ToDay { get; set; }
        public int? Status { get; set; }
        public int? Method { get; set; }
    }

    public class GetAlltenantPaymentInput : CommonInputDto
    {
    }

    public class TenantPaymentDto 
    {
        public int Id { get; set; }
        public string TenantName { get; set; }
        public TenantType? TenantType { get; set; }
        public int NumberEPayment { get; set; }
        public int NumberRPayment { get; set; }
        public double TotalAmountEpay { get; set; }
        public double TotalAmountRpay { get; set; }
        public double TotalPaymentForTenant { get; set; }
        public double TotalBalance { get; set; }
    }

    public class ThirdPartyPaymentDto: EntityDto<int>
    {
        public int? TenantId { get; set; }
        public string Code { get; set; }
        public EPaymentStatus Status { get; set; }
        public EPaymentMethod Method { get; set; }
        public EPaymentType Type { get; set; }
        public string TransactionId { get; set; }
        public string TransactionProperties { get; set; }
        public PayMonthlyUserBillsInput TransactionJson { get; set; }
        public double Amount { get; set; }
        public int? MerchantId { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public string Properties { get; set; }
        public DateTime CreatedAt { get; set; }
        public long? CreatedById { get; set; }
        public string TenantName { get; set; }
        public string FullName { get; set; }
        public string MerchantName { get; set; }
        public EInternalStateChangeStatus? InternalState { get; set; }
        public bool IsAutoVerified { get; set; }
        public bool IsManuallyVerified { get; set; }
        public object ObjectProperties { get; set; }
        public List<BillPaidDto> BillList { get; set; }
        public List<BillPaidDto> BillListDebt { get; set; }
        public List<PrepaymentBillDto> BillListPrepayment { get; set; }
    }

    public class CountThirdPartyPaymentDto
    {
        public double TotalAmount { get; set; }
        public int NumberPayment { get; set; }
        public string TenantName { get; set; }
    }
}
