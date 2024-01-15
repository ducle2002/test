using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services
{
    [Serializable]
    public class SendEmailUserBillJobArgs
    {
        public List<string> ApartmentCodes { get; set; }
        public DateTime? Period { get; set; }
        public int? TenantId { get; set; }

        public SendEmailUserBillJobArgs(List<string> apartmentCodes, DateTime? period, int? tenantId)
        {
            ApartmentCodes = apartmentCodes;
            Period = period;
            TenantId = tenantId;
        }
    }
}
