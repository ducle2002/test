using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using JetBrains.Annotations;
using Yootek.Common;

namespace Yootek.App.ServiceHttpClient.Dto.Business
{
    public class CreatePaymentDto
    {
        public int? TenantId { get; set; }
        public EPaymentMethod Method { get; set; }
        public long TransactionId { get; set; } // orderId, bookingId, ...
        [MaxLength(512)]
        public string? Description { get; set; }
        public EPaymentType Type { get; set; }
        public string? Properties { get; set; }
        public long Amount { get; set; }
        [CanBeNull] public string TransactionProperties { get; set; }
        [CanBeNull] public string Unit { get; set; }
    }
    public class DeletePaymentDto : EntityDto<long>
    {
    }
    public class GetAllPaymentsDto : CommonInputDto
    {
        public long? TransactionId { get; set; }
        public EPaymentType? Type { get; set; }
    }
    public class GetPaymentDetailDto
    {
        public long? Id { get; set; }
        public string? PaymentCode { get; set; }
        public string? TransactionCode { get; set; }
    }
    public class PaymentDto : EntityDto<long>
    {
        public int? TenantId { get; set; }
        public EPaymentStatus Status { get; set; }
        public EPaymentMethod Method { get; set; }
        public long TransactionId { get; set; }
        public string TransactionCode { get; set; }
        public string? Description { get; set; }
        [MaxLength(100)]
        public string Code { get; set; }
        public EPaymentType Type { get; set; }
        public long Amount { get; set; }
        public string? Properties { get; set; }
        public long? CreatorUserId { get; set; }
        public DateTime CreationTime { get; set; }
        [CanBeNull] public string TransactionProperties { get; set; }
        [CanBeNull] public string Unit { get; set; }
    }
    public class UpdatePaymentDto : EntityDto<long>
    {
        public EPaymentMethod? Method { get; set; }
        public string? Description { get; set; }
        public EPaymentType? Type { get; set; }
    }
    public enum ETypeActionUpdateStatusPayment
    {
        FAIL = 1,
        SUCCESS = 2
    }
    public class UpdateStatusPaymentDto
    {
        public string TransactionCode { get; set; }
        public ETypeActionUpdateStatusPayment TypeAction { get; set; }
    }
    public enum EPaymentMethod
    {
        COD = 1,
        MOMO = 2,
        VNPAY = 3,
        ZALOPAY = 4,
        VIETTELPAY = 5,
    }

    public enum EPaymentStatus
    {
        WAIT_FOR_PAY = 1,
        PAID = 2,
        FAIL = 3,
    }

    public enum EPaymentType
    {
        SHOPPING = 1,
        BOOKING = 2,
        BILL = 3,
        ECOFARM = 4,
    }
}
