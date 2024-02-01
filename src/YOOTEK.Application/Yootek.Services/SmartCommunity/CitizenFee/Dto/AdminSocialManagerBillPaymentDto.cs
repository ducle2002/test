using JetBrains.Annotations;
using System;
using Yootek.Common.Enum;
using Yootek.Common;
using YOOTEK.EntityDb;
using Abp.Application.Services.Dto;

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
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public DateTime? FromDay { get; set; }
        public DateTime? ToDay { get; set; }
        public int? Status { get; set; }
        public int? Method { get; set; }
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
        public double Amount { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public string Properties { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TenantName { get; set; }
    }

    public class CountThirdPartyPaymentDto
    {
        public double TotalAmount { get; set; }
        public int NumberPayment { get; set; }
        public string TenantName { get; set; }
    }
}
