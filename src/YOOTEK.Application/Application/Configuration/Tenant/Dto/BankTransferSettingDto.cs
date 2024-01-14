using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Application.Configuration.Tenant.Dto
{
    public class BankTransferSettingDto
    {
        public string BankCode { get; set; }
        public string BankNumber { get; set;}
        public string QRCode { get; set; }
        public string BankInfo { get; set; }

    }
}
