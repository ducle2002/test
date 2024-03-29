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

    public class ReSendVerificationOtpInput
    {
        public string FullName { get; set; }
        public string Email { get; set; }

        public string PhoneNumber { get; set; }
    }

    public class SendForgotPasswordOtpInput
    {
        public string PhoneNumber { get; set; }
    }

    public class ConfirmResetPasswordInput
    {
        public string OtpCode { get; set; }
        public string PhoneNumber { get; set; }
        public string NewPassword { get; set; }
    }
}
