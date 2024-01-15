namespace Yootek.Authorization.Accounts.Dto
{
    public class SendVerificationSmsInputDto
    {
        public string PhoneNumber { get; set; }
    }

    public class VerifySmsCodeInputDto
    {
        public string Code { get; set; }

        public string PhoneNumber { get; set; }
    }
}