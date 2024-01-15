using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Abp.Auditing;
using Abp.Authorization.Users;
using Newtonsoft.Json;

namespace Yootek.Models.TokenAuth
{
    public class AuthenticateModel
    {
        [Required]
        [StringLength(AbpUserBase.MaxEmailAddressLength)]
        public string UserNameOrEmailAddress { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxPlainPasswordLength)]
        [DisableAuditing]
        public string Password { get; set; }

        public bool RememberClient { get; set; }
    }

    public class AuthenticateModelTenant
    {
        [Required]
        [StringLength(AbpUserBase.MaxEmailAddressLength)]
        public string UserNameOrEmailAddress { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxPlainPasswordLength)]
        [DisableAuditing]
        public string Password { get; set; }

        public string TenancyName { get; set; }

        public bool RememberClient { get; set; }
    }

    public class ExternalAuthDto
    {
        public string Provider { get; set; }
        public string IdToken { get; set; }
    }


}
