using System.ComponentModel.DataAnnotations;

namespace IMAX.Authorization.Accounts.Dto
{
    public class SendEmailActivationLinkInput
    {
        [Required]
        public string EmailAddress { get; set; }
    }
}