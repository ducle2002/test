using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Abp;
using Abp.Auditing;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Runtime.Caching;
using Abp.Runtime.Session;
using Abp.UI;
using Abp.Zero.Configuration;
using Yootek.Account.Cache;
using Yootek.Authorization.Accounts.Dto;
using Yootek.Authorization.Users;
using Yootek.Common;
using Yootek.EntityDb;
using Yootek.MultiTenancy;
using Yootek.Net.Sms;
using Yootek.Notifications;
using Yootek.Organizations;
using Yootek.Url;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Abp.Json;

namespace Yootek.Authorization.Accounts
{
    [Audited]
    public class AccountAppService : YootekAppServiceBase, IAccountAppService
    {
        public IAppUrlService AppUrlService { get; set; }

        // from: http://regexlib.com/REDetails.aspx?regexp_id=1923
        public const string PasswordRegex =
            "(?=^.{8,}$)(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?!.*\\s)[0-9a-zA-Z!@#$%^&*()]*$";

        private readonly UserRegistrationManager _userRegistrationManager;
        private readonly UserManager _userManager;
        private readonly IAppNotifier _appNotifier;
        private readonly IRepository<Tenant, int> _tenantRepos;

        private readonly IUserEmailer _userEmailer;

        //  private readonly IRepository<TenantProject, long> _tenantProjectRepos;
        private readonly IRepository<AppOrganizationUnit, long> _tenantProjectRepos;
        private readonly ICacheManager _cacheManager;

        private readonly ISmsSender _smsSender;

        // private readonly IUnitOfWordManager
        public AccountAppService(
            UserManager userManager,
            IRepository<Tenant, int> tenantRepos,
            IAppNotifier appNotifier,
            UserRegistrationManager userRegistrationManager,
            IUserEmailer userEmailer,
            IRepository<AppOrganizationUnit, long> tenantProjectRepos,
            ICacheManager cacheManager,
            ISmsSender smsSender
        )
        {
            _cacheManager = cacheManager;
            _userRegistrationManager = userRegistrationManager;
            _userManager = userManager;
            _appNotifier = appNotifier;
            _tenantRepos = tenantRepos;
            _userEmailer = userEmailer;
            _tenantProjectRepos = tenantProjectRepos;
            AppUrlService = NullAppUrlService.Instance;
            _smsSender = smsSender;
        }

        [AllowAnonymous]
        public async Task<object> GetAllTenantName()
        {
            try
            {
                var tenants = (from tn in _tenantRepos.GetAll()
                    select new
                    {
                        DisplayName = tn.Name,
                        TenancyName = tn.TenancyName
                    }).ToList();

                return tenants;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<object> GetAllTenantProjectName()
        {
            try
            {
                var tenants = (from tn in _tenantProjectRepos.GetAll()
                    where tn.Type == APP_ORGANIZATION_TYPE.URBAN
                    select new
                    {
                        Id = tn.Id,
                        Name = tn.DisplayName,
                        ProjectCode = tn.ProjectCode,
                        OrganizationUnitId = tn.ParentId
                    }).ToList();

                return tenants;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input)
        {
            if (string.IsNullOrWhiteSpace(input.TenancyName))
            {
                return new IsTenantAvailableOutput(TenantAvailabilityState.NotFound);
            }

            var tenant = await TenantManager.FindByTenancyNameAsync(input.TenancyName);
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

        public async Task<RegisterOutput> Register(RegisterInput input)
        {
            if (string.IsNullOrWhiteSpace(input.Name + input.Surname) && !string.IsNullOrWhiteSpace(input.FullName))
            {
                var names = input.FullName.Trim().Split(" ");
                if (names.Length == 1)
                {
                    input.Name = names[0];
                    input.Surname = "";
                }

                if (names.Length > 1)
                {
                    input.Surname = names[0];
                    input.Name = input.FullName.Trim().Split(input.Surname)[1].Trim();
                }
            }

            User user;
            user = await _userRegistrationManager.RegisterAsync(
                input.Name,
                input.Surname,
                input.EmailAddress,
                input.UserName,
                input.Password,
                true,
                false,
                input.PhoneNumber);

            var isEmailConfirmationRequiredForLogin =
                await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement
                    .IsEmailConfirmationRequiredForLogin);

            await _appNotifier.WelcomeToTheApplicationAsync(user);   
            return new RegisterOutput
            {
                CanLogin = user.IsActive && (user.IsEmailConfirmed || !isEmailConfirmationRequiredForLogin),
                ThirdAccounts = user.ThirdAccounts
            };
        }

        public async Task<object> SendPasswordResetCode(SendPasswordResetCodeInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(input.TenantId))
                {
                    var user = await UserManager.FindByEmailAsync(input.EmailAddress);
                    if (user == null)
                    {
                        return null;
                    }

                    user.SetNewPasswordResetCode();
                    await _userEmailer.SendPasswordResetLinkAsync(user);
                    return new
                    {
                        UserId = user.Id
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal("pass + Email: " + ex.ToJsonString());
                throw;
            }
        }

        public async Task<ResetPasswordOutput> ResetPassword(ResetPasswordInput input)
        {
            using (CurrentUnitOfWork.SetTenantId(input.TenantId))
            {
                var user = await UserManager.GetUserByIdAsync(input.UserId);
                if (user == null || user.PasswordResetCode.IsNullOrEmpty() || user.PasswordResetCode != input.ResetCode)
                {
                    throw new UserFriendlyException(L("InvalidPasswordResetCode"),
                        L("InvalidPasswordResetCode_Detail"));
                }

                await UserManager.InitializeOptionsAsync(AbpSession.TenantId);
                CheckErrors(await UserManager.ChangePasswordAsync(user, input.Password));
                user.PasswordResetCode = null;
                user.IsEmailConfirmed = true;
                //user.ShouldChangePasswordOnNextLogin = false;

                await UserManager.UpdateAsync(user);

                return new ResetPasswordOutput
                {
                    CanLogin = user.IsActive,
                    UserName = user.UserName
                };
            }
        }

        public async Task SyncAccount(SyncAccountInput input)
        {
            //var tenants = await TenantManager.GetAllTenantAsync();
            var user = await _userManager.GetUserByIdAsync(AbpSession.UserId.Value);
            //if (tenants != null && user != null)
            //{
            //    foreach (var tenant in tenants)
            //    {

            //        using (CurrentUnitOfWork.SetTenantId(tenant.Id))
            //        {
            //            var userth = await _userManager.FindByNameOrEmailAsync(user.EmailAddress);
            //            if (userth != null)
            //            {
            //                userth.ThirdAccounts = input.ThirdAccount;
            //                await _userManager.UpdateAsync(userth);
            //            }
            //        }
            //    }
            //}

            if (user != null)
            {
                user.ThirdAccounts = input.ThirdAccount;
                await _userManager.UpdateAsync(user);
            }

            CurrentUnitOfWork.SaveChanges();
        }

        public async Task SendEmailActivationLink(SendEmailActivationLinkInput input)
        {
            var user = await UserManager.FindByEmailAsync(input.EmailAddress);
            if (user == null)
            {
                return;
            }

            user.SetNewEmailConfirmationCode();
            await _userEmailer.SendEmailActivationLinkAsync(
                user,
                AppUrlService.CreateEmailActivationUrlFormat(AbpSession.TenantId)
            );
        }

        public async Task GetActivateEmail(ActivateEmailInput input)
        {
            var user = await UserManager.FindByIdAsync(input.UserId.ToString());
            if (user != null && user.IsEmailConfirmed)
            {
                return;
            }

            if (user == null || user.EmailConfirmationCode.IsNullOrEmpty() ||
                user.EmailConfirmationCode != input.ConfirmationCode)
            {
                throw new UserFriendlyException(L("InvalidEmailConfirmationCode"),
                    L("InvalidEmailConfirmationCode_Detail"));
            }

            user.IsEmailConfirmed = true;
            user.EmailConfirmationCode = null;

            await UserManager.UpdateAsync(user);
        }

        public async Task SendVerificationSms(SendVerificationSmsInputDto input)
        {
            var code = RandomHelper.GetRandom(100000, 999999).ToString();
            var cacheKey = AbpSession.ToUserIdentifier().ToString();
            var cacheItem = new SmsVerificationCodeCacheItem { Code = code };

            await _cacheManager.GetSmsVerificationCodeCache().SetAsync(
                cacheKey,
                cacheItem
            );

            await _smsSender.SendAsync(input.PhoneNumber, L("SmsVerificationMessage", code));
        }

        public async Task VerifySmsCode(VerifySmsCodeInputDto input)
        {
            var cacheKey = AbpSession.ToUserIdentifier().ToString();
            var cash = await _cacheManager.GetSmsVerificationCodeCache().GetOrDefaultAsync(cacheKey);

            if (cash == null)
            {
                throw new Exception("Phone number confirmation code is not found in cache !");
            }

            if (input.Code != cash.Code)
            {
                throw new UserFriendlyException(L("WrongSmsVerificationCode"));
            }

            var user = await UserManager.GetUserAsync(AbpSession.ToUserIdentifier());
            user.IsPhoneNumberConfirmed = true;
            user.PhoneNumber = input.PhoneNumber;
            await UserManager.UpdateAsync(user);
        }
    }
}