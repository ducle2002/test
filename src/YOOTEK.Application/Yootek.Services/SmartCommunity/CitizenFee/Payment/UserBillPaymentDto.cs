using Abp.AutoMapper;
using JetBrains.Annotations;
using Yootek.App.ServiceHttpClient.Dto.Business;
using Yootek.Common.Enum;
using Yootek.Services.Dto;


namespace Yootek.Services
{
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
        public long PaymentId { get; set; }
        public EPrepaymentStatus Status { get; set; }
        public int? TenantId { get; set; }
    }

    public enum EPrepaymentStatus
    {
        PENDING = 1,
        SUCCESS = 2,
        FAILED = 3,
    }
}
