using Yootek.Common.Enum;
using Yootek.EntityDb;

namespace Yootek.Yootek.Services.Yootek.DichVu.Payment
{


    public class PaymentExtraDataDto
    {
        public long UserId { get; set; }
        public int TenantId { get; set; }
        public long[] UserBillIds { get; set; }
        public long PaymentId { get; set; }
        public TypePayment TypePayment { get; set; }
        public long[] BillDebtIds { get; set; }
    }
}