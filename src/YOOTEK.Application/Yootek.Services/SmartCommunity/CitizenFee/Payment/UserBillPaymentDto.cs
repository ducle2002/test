using Abp.AutoMapper;
using JetBrains.Annotations;
using System.ComponentModel.DataAnnotations;
using Yootek.App.ServiceHttpClient.Dto.Business;
using Yootek.Common.Enum;
using Yootek.Services.Dto;
using Yootek.Yootek.Services.Yootek.SmartCommunity.CitizenFee.Dto;


namespace Yootek.Services
{
    public class RequestValidationPaymentDto
    {
        public int? TenantId { get; set; }
        public EPaymentMethod Method { get; set; }
        public long TransactionId { get; set; } // orderId, bookingId, ...
        [MaxLength(512)]
        public string Description { get; set; }
        public EPaymentType Type { get; set; }
        public string Properties { get; set; }
        public long Amount { get; set; }
        public PayMonthlyUserBillsInput TransactionProperties { get; set; }
        public string Unit { get; set; }
    }

    internal class UserBillPaymentDto
    {
    }

    [AutoMap(typeof(MappingRequestPaymentDto))]
    public class RequestUserBillPaymentInputDto
    {
        public int? TenantId { get; set; }
        public int Method { get; set; }
        public EPaymentType Type { get; set; }
        public int TransactionId { get; set; }
        [CanBeNull] public string TransactionProperties { get; set; }
        [CanBeNull] public string Description { get; set; }
        [CanBeNull] public string Properties { get; set; }
        public int Amount { get; set; }
        [CanBeNull] public string Unit { get; set; }
    }

    public class HandPaymentForThirdPartyInput
    {
        public int Id { get; set; }
        public EPrepaymentStatus Status { get; set; }
        public int? TenantId { get; set; }
    }

    public class RequestValidationInput
    {
        public long TransactionId { get; set; }
        public string ReturnUrl { get; set; }
    }

    public enum EPrepaymentStatus
    {
        PENDING = 1,
        SUCCESS = 2,
        FAILED = 3,
    }
}
