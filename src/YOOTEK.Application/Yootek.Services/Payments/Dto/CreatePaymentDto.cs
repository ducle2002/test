using JetBrains.Annotations;

namespace Yootek.Yootek.Services.Yootek.Payments.Dto
{
    public class CreatePaymentDto
    {
        public int? TenantId { get; set; }
        public int Method { get; set; }
        public int Type { get; set; }
        public int TransactionId { get; set; }
        [CanBeNull] public string TransactionProperties { get; set; }
        [CanBeNull] public string Description { get; set; }
        [CanBeNull] public string Properties { get; set; }
        public int Amount { get; set; }
        [CanBeNull] public string Unit { get; set; }
    }
}