using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAX.IMAX.Services.IMAX.SmartCommunity.CitizenFee.Dto
{
    public class DeleteBillMonthlyInput
    {
        public List<long> Ids { get; set; }
        public bool IsDeleteAll { get; set; }
    }
}
