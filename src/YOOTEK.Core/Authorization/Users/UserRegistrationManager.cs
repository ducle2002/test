using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Abp.Authorization.Users;
using Abp.Domain.Services;
using Abp.IdentityFramework;
using Abp.Runtime.Session;
using Abp.UI;
using Yootek.Authorization.Roles;
using Yootek.MultiTenancy;
using Yootek.EntityDb;
using Abp.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Yootek.Configuration;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json;
using System.Text;
using System.Net;
using System.Security.Cryptography;

namespace Yootek.Authorization.Users
{
    public class UserRegistrationManager : DomainService
    {
        public IAbpSession AbpSession { get; set; }
        private readonly TenantManager _tenantManager;
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IRepository<Citizen, long> _citizenRepos;
        private readonly IConfigurationRoot _appConfiguration;

        public UserRegistrationManager(
            TenantManager tenantManager,
            UserManager userManager,
            RoleManager roleManager,
            IPasswordHasher<User> passwordHasher,
            IRepository<Citizen, long> citizenRepos,
            IAppConfigurationAccessor configurationAccessor
            )
        {
            _tenantManager = tenantManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _passwordHasher = passwordHasher;
            _citizenRepos = citizenRepos;
            AbpSession = NullAbpSession.Instance;
            _appConfiguration = configurationAccessor.Configuration;
        }

        public async Task<User> RegisterAsync(string name, string surname, string emailAddress, string userName, string plainPassword, bool isEmailConfirmed, bool isCitizen = false, string phoneNumber = "", string address = "", string gender = "", DateTime? dateOfBirth = null)
        {
            //CheckForTenant();

            //var tenant = await GetActiveTenantAsync();
            string thirdAcc = null ;
            if (!string.IsNullOrEmpty(emailAddress))
            {
                thirdAcc = await GetThirdAccountUserMultiTenantByMail(emailAddress);
            }
            else emailAddress = "";
            var user = new User
            {
                Name = name,
                Surname = surname,
                EmailAddress = emailAddress,
                IsActive = true,
                UserName = userName,
                IsEmailConfirmed = isEmailConfirmed,
                ThirdAccounts = thirdAcc,
                Roles = new List<UserRole>(),
                PhoneNumber = phoneNumber
            };

            user.SetNormalizedNames();

            await _userManager.InitializeOptionsAsync(null);

            CheckErrors(await _userManager.CreateAsync(user, plainPassword));

            await CurrentUnitOfWork.SaveChangesAsync();

            return user;
        }

        public async Task<User> RegisterErpAsync(string name, string surname, string emailAddress, string userName, string plainPassword, bool isEmailConfirmed, string phoneNumber = "")
        {
            var user = new User
            {
                Name = name,
                Surname = surname,
                EmailAddress = emailAddress,
                IsActive = true,
                UserName = userName,
                IsEmailConfirmed = isEmailConfirmed,
                Roles = new List<UserRole>(),
                PhoneNumber = phoneNumber
            };

            user.SetNormalizedNames();

            await _userManager.InitializeOptionsAsync(null);

            CheckErrors(await _userManager.CreateAsync(user, plainPassword));

            await CurrentUnitOfWork.SaveChangesAsync();

            return user;
        }



        public async Task<List<User>> RegisterListAccoutAsync(List<AccoutRegister> accoutRegisters)
        {

            List<User> result = new List<User>();
            if(accoutRegisters != null)
            {
                foreach(var us in accoutRegisters)
                {
                    var user = new User
                    {
                        Name = us.Name,
                        Surname = us.Surname,
                        EmailAddress = us.EmailAddress,
                        IsActive = true,
                        UserName = us.UserName,
                        IsEmailConfirmed = true,
                        ThirdAccounts = null,
                        Roles = new List<UserRole>(),
                        PhoneNumber = us.PhoneNumber
                    };


                    user.SetNormalizedNames();

                    await _userManager.InitializeOptionsAsync(null);
                    if (string.IsNullOrWhiteSpace(us.Password) || us.Password.Length < 4) us.Password = "Password01@";
                    user.Password = "Password01@";
                    CheckErrors(await _userManager.CreateAsync(user, us.Password));
                  
                    await CurrentUnitOfWork.SaveChangesAsync();

                    result.Add(user);
                }
            }
            return result;
        }



        public async Task<string> GetThirdAccountUserMultiTenantByMail(string email)
        {
            try
            {
                var tenants = await _tenantManager.GetAllTenantAsync();
                string thirdAccount = null;
                var currentuser = await _userManager.FindByNameOrEmailAsync(email);
                if (currentuser != null)
                {
                    if (currentuser.EmailAddress == email && currentuser.UserName != email)
                    {
                        thirdAccount = currentuser.ThirdAccounts;
                    }
                    else if (currentuser.EmailAddress != email && currentuser.UserName == email)
                    {
                        var useremail = await _userManager.FindByNameOrEmailAsync(currentuser.EmailAddress);
                        if (useremail != null) thirdAccount = useremail.ThirdAccounts;
                    }
                }
                else if (tenants != null)
                {
                    foreach (var tenant in tenants)
                    {
                        using (CurrentUnitOfWork.SetTenantId(tenant.Id))
                        {
                            var user = await _userManager.FindByNameOrEmailAsync(email);
                            if (user != null && user.ThirdAccounts != null)
                            {
                                thirdAccount = user.ThirdAccounts;
                                break;
                            }
                        }
                    }
                }
                return thirdAccount;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private void CheckForTenant()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                throw new InvalidOperationException("Can not register host users!");
            }
        }

        private async Task<Tenant> GetActiveTenantAsync()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                return null;
            }

            return await GetActiveTenantAsync(AbpSession.TenantId.Value);
        }

        private async Task<Tenant> GetActiveTenantAsync(int tenantId)
        {
            var tenant = await _tenantManager.FindByIdAsync(tenantId);
            if (tenant == null)
            {
                throw new UserFriendlyException(L("UnknownTenantId{0}", tenantId));
            }

            if (!tenant.IsActive)
            {
                throw new UserFriendlyException(L("TenantIdIsNotActive{0}", tenantId));
            }

            return tenant;
        }

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            try
            {
                identityResult.CheckErrors(LocalizationManager);
            }catch(Exception e)
            {

            }
        }

        public string GeneratePassThirdAccount()
        {
            var Length = 9;
            char[] Chars = new char[] {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
            };
            char[] arrSpec = new char[] { '!', '@', '#' };
            string pass = string.Empty;
            Random Random = new Random();

            for (byte a = 0; a < Length; a++)
            {
                pass += Chars[Random.Next(0, 61)];
            };
            pass += arrSpec[Random.Next(0, 3)];
            return pass;
        }


        public string GenerateString()
        {
            int maxSize = 5;
            char[] chars = new char[36];
            string a;
            a = "abcdefghiknmlopquwvxz1234567890";
            chars = a.ToCharArray();
            int size = maxSize;
            byte[] data = new byte[1];
            RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
            crypto.GetNonZeroBytes(data);
            size = maxSize;
            data = new byte[size];
            crypto.GetNonZeroBytes(data);
            StringBuilder result = new StringBuilder(size);
            foreach (byte b in data)
            { result.Append(chars[b % (chars.Length - 1)]); }
            return result.ToString();
        }
    }

}
