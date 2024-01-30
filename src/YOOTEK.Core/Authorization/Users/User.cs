using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Abp.Authorization.Users;
using Abp.Extensions;
using Abp.Timing;
using Yootek.EntityDb;
using Yootek.GroupChats;
using Newtonsoft.Json;

namespace Yootek.Authorization.Users
{

    public class User : AbpUser<User>
    {
        public const string DefaultPassword = "123qwe";

        public static string CreateRandomPassword()
        {
            return Guid.NewGuid().ToString("N").Truncate(16);
        }

        public void SetSignInToken()
        {
            SignInToken = Guid.NewGuid().ToString();
            SignInTokenExpireTimeUtc = Clock.Now.AddMinutes(1).ToUniversalTime();
        }

        public override void SetNewPasswordResetCode()
        {
            /* This reset code is intentionally kept short.
             * It should be short and easy to enter in a mobile application, where user can not click a link.
             */
            PasswordResetCode = Guid.NewGuid().ToString("N").Truncate(8).ToUpperInvariant();
        }

        public void Unlock()
        {
            AccessFailedCount = 0;
            LockoutEndDateUtc = null;
        }
        //public List<RoomUserChat> RoomUserChats { get; set; }

        public static User CreateTenantAdminUser(int tenantId, string emailAddress)
        {
            var user = new User
            {
                TenantId = tenantId,
                UserName = AdminUserName,
                Name = AdminUserName,
                Surname = AdminUserName,
                EmailAddress = emailAddress,
                Roles = new List<UserRole>()
            };

            user.SetNormalizedNames();

            return user;
        }

        public override void SetNormalizedNames()
        {
            NormalizedUserName = UserName.ToUpperInvariant();
            if(!string.IsNullOrEmpty(EmailAddress)) NormalizedEmailAddress = EmailAddress.ToUpperInvariant();

        }

        [StringLength(MaxUserNameLength)]
        [AllowNull]
        public override string NormalizedUserName { get; set; }
        [StringLength(MaxEmailAddressLength)]
        [AllowNull]
        public override string NormalizedEmailAddress { get; set; }
        [StringLength(MaxEmailAddressLength)]
        [AllowNull]
        public override string EmailAddress { get; set; }
        [StringLength(MaxNameLength)]
        [AllowNull]
        public override string Name { get; set; }
        [StringLength(MaxSurnameLength)]
        [AllowNull]
        public override string Surname { get; set; }
        [NotMapped]
        public override string FullName { get { return this.Surname + " " + this.Name; } }
        public string GoogleAuthenticatorKey { get; set; }
        public DateTime? SignInTokenExpireTimeUtc { get; set; }
        public string SignInToken { get; set; }
        [StringLength(1000)]
        public string HomeAddress { get; set; }
        [StringLength(1000)]
        public string AddressOfBirth { get; set; }
        public DateTime? DateOfBirth { get; set; }
        [StringLength(128)]
        public string Gender { get; set; }
        [StringLength(256)]
        public string Nationality { get; set; }
        public virtual Guid? ProfilePictureId { get; set; }
        [StringLength(256)]
        public string ImageUrl { get; set; }
        public string CoverImageUrl { get; set; }
        public string ThirdAccounts { get; set; }
        public DateTime? WillBeDeletedDate { get; set; }
    }
}
