﻿using System.ComponentModel.DataAnnotations;
using Abp.Authorization.Users;

namespace IMAX.Authorization.Accounts.Dto
{
    public class SendPasswordResetCodeInput
    {
        [Required]
        [MaxLength(AbpUserBase.MaxEmailAddressLength)]
        public string EmailAddress { get; set; }
        public int? TenantId { get; set; }
    }
}