using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAX.Services
{
    public class VerifyPaymentBillDebtInput
    {
        public long BillDebtId { get; set; }
        public int Amount { get; set; }
        public string ApartmentCode { get; set; }
        public DateTime? Period { get; set; }
        public string Description { get; set; }

    }
}
