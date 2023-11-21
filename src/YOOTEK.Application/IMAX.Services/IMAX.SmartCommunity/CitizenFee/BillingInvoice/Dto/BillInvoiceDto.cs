using IMAX.Common.Enum;
using IMAX.EntityDb;
using System.Collections.Generic;

namespace IMAX.Services.SmartCommunity.BillingInvoice.Dto
{
    public class PrintBillInvoiceInput
    {
        public string ApartmentCode { get; set; }
        public int PeriodMonth { get; set; }
        public int PeriodYear { get; set; }
        public List<long> UserBillIds { get; set; }
        public long? PaymentId { get; set; }
        public UserBillPaymentMethod Method { get; set; }
        public UserBillPayment Payment { get; set; }
    }
}
