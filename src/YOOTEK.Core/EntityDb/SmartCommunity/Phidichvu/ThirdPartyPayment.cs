using Abp.Domain.Entities;
using System;

namespace YOOTEK.EntityDb
{
    public class ThirdPartyPayment: Entity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public string Code { get; set; }
        public EPaymentStatus Status { get; set; }
        public EPaymentMethod Method { get; set; }
        public EPaymentType Type { get;set; }
        public long TransactionId { get; set; }
        public string TransactionProperties { get; set; }
        public float Amount { get; set; }
        public string Currency {  get; set; }
        public string Description { get; set; }
        public string Properties {  get; set; }
        public DateTime CreateAt { get; set; }
        public string MerchantId { get; set; }  
    }

    public enum EPaymentStatus
    {
        Pending = 1,
        Paid = 2,
        Failed = 3,
    }

    public enum EPaymentMethod
    {
        Other = 0,
        Cod = 1,
        Momo = 2,
        Onepay = 3,
    }

    public enum EPaymentType
    {
        Shopping = 1,
        Booking = 2,
        Invoice = 3,
        Ecofarm = 4,
    }
}
