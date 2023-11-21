using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAX.IMAX.Services.IMAX.SmartCommunity.CitizenFee.Payment
{
    internal class CountPaymentResult
    {
        public double TotalAmount { get; set; }
        public int NumberPayment { get; set; }
        public int NumberApartment { get; set; }
        public int NumberUserBill { get; set; } 
    }
}
