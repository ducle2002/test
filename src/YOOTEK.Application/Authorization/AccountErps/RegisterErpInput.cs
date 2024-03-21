using Abp.Auditing;
using Abp.Authorization.Users;
using Abp.Collections.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Yootek.MultiTenancy;
using Yootek.Validation;

namespace YOOTEK.Authorization.AccountErps
{
    public class RegisterErpInput : IValidatableObject
    {
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        [Required]
        [StringLength(AbpUserBase.MaxPlainPasswordLength)]
        public string Password { get; set; }
        public string CaptchaResponse { get; set; }
        public string PhoneNumber { get; set; }
        public string CityAddress { get; set; }


        public const string PhoneNumberRegex = @"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$";

        private bool IsPhoneNbr(string number)
        {
            if (number != null) return Regex.IsMatch(number, PhoneNumberRegex);
            else return false;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!PhoneNumber.IsNullOrEmpty() && !IsPhoneNbr(PhoneNumber))
            {
                yield return new ValidationResult("Phone number is invalid!");
            }
        }
    }
}
