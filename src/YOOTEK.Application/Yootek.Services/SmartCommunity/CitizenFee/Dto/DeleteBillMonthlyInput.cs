using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Yootek.Services.Yootek.SmartCommunity.CitizenFee.Dto
{
    public class DeleteBillMonthlyInput
    {
        public List<long> Ids { get; set; }
        public bool IsDeleteAll { get; set; }
    }
}
