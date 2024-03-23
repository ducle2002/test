using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YOOTEK.Authorization.AccountErps
{
    public class VerifyErpOtpInputDto
    {
        public string Code { get; set; }

        public string PhoneNumber { get; set; }
    }
}
