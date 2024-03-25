using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Runtime.Caching;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yootek.Authorization.Accounts.Dto;
using Yootek.Authorization.Accounts;
using Yootek.Authorization.Users;
using Yootek.Net.Sms;
using Yootek.Notifications;
using Yootek.Organizations;
using Yootek.Url;
using Yootek;
using Yootek.MultiTenancy;
using YOOTEK.Authorization.AccountErps;
using Abp.UI;
using Yootek.Authorization.Roles;
using Abp.Runtime.Session;
using Yootek.Account.Cache;
using Abp;
using Yootek.Common.DataResult;
using Abp.Json;
using Microsoft.AspNetCore.Mvc;

namespace YOOTEK.Authorization.Accounts
{
    public class AccountErpAppService : YootekAppServiceBase
    {
        public IAppUrlService AppUrlService { get; set; }

        // from: http://regexlib.com/REDetails.aspx?regexp_id=1923
        public const string PasswordRegex =
            "(?=^.{8,}$)(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?!.*\\s)[0-9a-zA-Z!@#$%^&*()]*$";

        private readonly UserRegistrationManager _userRegistrationManager;
        private readonly UserManager _userManager;
        private readonly TenantManager _tenantManager;
        private readonly IAppNotifier _appNotifier;
        private readonly IUserEmailer _userEmailer;
        private readonly ICacheManager _cacheManager;
        private readonly ISmsSender _smsSender;

        // private readonly IUnitOfWordManager
        public AccountErpAppService(
            UserManager userManager,
            TenantManager tenantManager,
            IRepository<Tenant, int> tenantRepos,
            IAppNotifier appNotifier,
            UserRegistrationManager userRegistrationManager,
            IUserEmailer userEmailer,
            ICacheManager cacheManager,
            ISmsSender smsSender
        )
        {
            _cacheManager = cacheManager;
            _tenantManager = tenantManager;
            _userRegistrationManager = userRegistrationManager;
            _userManager = userManager;
            _appNotifier = appNotifier;
            _userEmailer = userEmailer;
            AppUrlService = NullAppUrlService.Instance;
            _smsSender = smsSender;
        }

        public async Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input)
        {
            if (string.IsNullOrWhiteSpace(input.TenancyName))
            {
                return new IsTenantAvailableOutput(TenantAvailabilityState.NotFound);
            }

            var tenant = await _tenantManager.FindByTenancyErpNameAsync(input.TenancyName);
            if (tenant == null)
            {
                return new IsTenantAvailableOutput(TenantAvailabilityState.NotFound);
            }

            if (!tenant.IsActive)
            {
                return new IsTenantAvailableOutput(TenantAvailabilityState.InActive);
            }

            return new IsTenantAvailableOutput(TenantAvailabilityState.Available, tenant.Id, tenant.MobileConfig,
                tenant.AdminPageConfig);
        }

        public async Task<DataResult> Register(RegisterErpInput input)
        {

            var tenant = await _tenantManager.FindByTenancyErpNameAsync(input.PhoneNumber);
            if (tenant != null) throw new UserFriendlyException(500101, "Phone number is exist !");

            //Create tenant by phone number
            tenant = new Tenant()
            {
                TenancyName = input.PhoneNumber,
                TenantType = TenantType.ERP,
                Name = input.PhoneNumber,
                IsActive = true
            };
           
            await _tenantManager.CreateAsync(tenant);
            await CurrentUnitOfWork.SaveChangesAsync(); // To get new tenant's id.

       
            // We are working entities of new tenant, so changing tenant filter
            using (CurrentUnitOfWork.SetTenantId(tenant.Id))
            {
                string name = "";
                string surname = "";
                if (!string.IsNullOrWhiteSpace(input.FullName))
                {
                    var names = input.FullName.Trim().Split(" ");
                    if (names.Length == 1)
                    {
                        name = names[0];
                        surname = "";
                    }

                    if (names.Length > 1)
                    {
                        surname = names[0];
                        name = input.FullName.Trim().Split(surname)[1].Trim();
                    }
                }

                User user;
                user = await _userRegistrationManager.RegisterErpAsync(
                    name,
                    surname,
                    input.EmailAddress,
                    input.PhoneNumber,
                    input.Password,
                    false,
                    input.PhoneNumber);
                user.IsActive = false;
                var timeCodeExpire = TimeSpan.FromMinutes(5);
                await SendVerificationOtp(input.PhoneNumber, input.EmailAddress, input.FullName, timeCodeExpire);
                return DataResult.ResultSuccess(new RegisterErpOutput
                {
                    CanLogin = user.IsActive && user.IsEmailConfirmed,
                    TimeCodeExpire = timeCodeExpire
                }, "Success");
            }

        }

        public async Task<DataResult> SendForgotPasswordOtp([FromBody] string phoneNumber)
        {
            try
            {
                var tenant = await _tenantManager.FindByTenancyErpNameAsync(phoneNumber);
                if (tenant == null) throw new UserFriendlyException(404, "Phone number is not exist !");

                using (CurrentUnitOfWork.SetTenantId(tenant.Id))
                {
                    var user = await UserManager.FindByNameAsync(phoneNumber);
                    if (user == null)
                    {
                        throw new UserFriendlyException(404, "Phone number is not exist !");
                    }

                    user.SetNewPasswordResetCode();
                    await _userEmailer.SendPasswordResetLinkAsync(user);
                    return DataResult.ResultSuccess("");
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal("pass + Email: " + ex.ToJsonString());
                throw;
            }
        }

        public async Task<DataResult> ReSendVerificationOtp([FromBody] ReSendVerificationOtpInput input)
        {
           
            var timeCodeExpire = TimeSpan.FromMinutes(5);

            await SendVerificationOtp(input.PhoneNumber, input.Email, input.FullName, timeCodeExpire);
            return DataResult.ResultSuccess("Send success !");
        }

        protected async Task SendVerificationOtp(string phoneNumber, string email, string fullName, TimeSpan timeCodeExpire)
        {
            var code = RandomHelper.GetRandom(100000, 999999).ToString();
            var cacheItem = new OtpErpVerificationCodeCacheItem { Code = code };

            await _cacheManager.GetOtpErpVerificationCodeCache().SetAsync(
                phoneNumber,
                cacheItem,
                timeCodeExpire
            );

           await _userEmailer.SendOtpUserRegisterync(fullName, email, code);
        }

        public async Task<DataResult> VerifyOtpCode(VerifyErpOtpInputDto input)
        {
            var cash = await _cacheManager.GetOtpErpVerificationCodeCache().GetOrDefaultAsync(input.PhoneNumber);

            if (cash == null)
            {
                throw new UserFriendlyException(500102, "OTP code is expired !");
            }

            if (input.Code != cash.Code)
            {
                throw new UserFriendlyException(500103, "OTP code is not matched !");
            }

            var user = await UserManager.GetErpUserOrNullByPhoneNumberAsync(input.PhoneNumber);
            user.IsPhoneNumberConfirmed = true;
            user.IsEmailConfirmed = true;
            user.IsActive = true;
            await UserManager.UpdateErpUserAsync(user);
            return DataResult.ResultSuccess("Send success !");
        }
    }
}
