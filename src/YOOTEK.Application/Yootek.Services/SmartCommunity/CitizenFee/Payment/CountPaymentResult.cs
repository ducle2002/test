using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Yootek.Services.Yootek.SmartCommunity.CitizenFee.Payment
{
    internal class CountPaymentResult
    {
        public double TotalAmount { get; set; }
        public int NumberPayment { get; set; }
        public int NumberApartment { get; set; }
        public int NumberUserBill { get; set; } 
    }

    internal class CountPaymentSocialResult
    {
        public double TotalAmount { get; set; }
        public double TotalBalanceOnePay { get; set; }
        public double TotalBalanceMomo { get; set; }
        public int NumberPayment { get; set; }
        public string TenantName { get; set; }
    }
}
