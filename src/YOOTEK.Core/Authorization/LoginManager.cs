﻿using Microsoft.AspNetCore.Identity;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Configuration;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Zero.Configuration;
using Yootek.Authorization.Roles;
using Yootek.Authorization.Users;
using Yootek.MultiTenancy;
using System.Threading.Tasks;
using System;
using Abp.Extensions;

namespace Yootek.Authorization
{
    public class LogInManager : AbpLogInManager<Tenant, Role, User>
    {
        public LogInManager(
            UserManager userManager,
            IMultiTenancyConfig multiTenancyConfig,
            IRepository<Tenant> tenantRepository,
            IUnitOfWorkManager unitOfWorkManager,
            ISettingManager settingManager,
            IRepository<UserLoginAttempt, long> userLoginAttemptRepository,
            IUserManagementConfig userManagementConfig,
            IIocResolver iocResolver,
            IPasswordHasher<User> passwordHasher,
            RoleManager roleManager,
            UserClaimsPrincipalFactory claimsPrincipalFactory)
            : base(
                  userManager,
                  multiTenancyConfig,
                  tenantRepository,
                  unitOfWorkManager,
                  settingManager,
                  userLoginAttemptRepository,
                  userManagementConfig,
                  iocResolver,
                  passwordHasher,
                  roleManager,
                  claimsPrincipalFactory)
        {
        }


        [UnitOfWork]
        public override async Task<AbpLoginResult<Tenant, User>> LoginAsync(string userNameOrEmailAddress, string plainPassword, string tenancyName = null, bool shouldLockout = true)
        {
            var result = await LoginAsyncInternal(userNameOrEmailAddress, plainPassword, tenancyName, shouldLockout);
            await SaveLoginAttemptAsync(result, tenancyName, userNameOrEmailAddress);
            return result;
        }


        protected override async Task<AbpLoginResult<Tenant, User>> LoginAsyncInternal(string userNameOrEmailAddress, string plainPassword, string tenancyName, bool shouldLockout)
        {
            try
            {
                if (userNameOrEmailAddress.IsNullOrEmpty())
                {
                    throw new ArgumentNullException(nameof(userNameOrEmailAddress));
                }

                if (plainPassword.IsNullOrEmpty())
                {
                    throw new ArgumentNullException(nameof(plainPassword));
                }

                //Get and check tenant
                Tenant tenant = null;
                using (UnitOfWorkManager.Current.SetTenantId(null))
                {
                    if (!MultiTenancyConfig.IsEnabled)
                    {
                        tenant = await GetDefaultTenantAsync();
                    }
                    else if (!string.IsNullOrWhiteSpace(tenancyName))
                    {
                        tenant = await TenantRepository.FirstOrDefaultAsync(t => t.TenancyName.ToLower() == tenancyName.ToLower());
                        if (tenant == null)
                        {
                            return new AbpLoginResult<Tenant, User>(AbpLoginResultType.InvalidTenancyName);
                        }

                        if (!tenant.IsActive)
                        {
                            return new AbpLoginResult<Tenant, User>(AbpLoginResultType.TenantIsNotActive, tenant);
                        }
                    }
                }

                var tenantId = tenant == null ? (int?)null : tenant.Id;
                using (UnitOfWorkManager.Current.SetTenantId(tenantId))
                {
                    await UserManager.InitializeOptionsAsync(tenantId);

                    //TryLoginFromExternalAuthenticationSources method may create the user, that's why we are calling it before AbpUserStore.FindByNameOrEmailAsync
                    var loggedInFromExternalSource = await TryLoginFromExternalAuthenticationSourcesAsync(userNameOrEmailAddress, plainPassword, tenant);

                    var user = await UserManager.FindByNameOrEmailAsync(tenantId, userNameOrEmailAddress);
                    if (user == null)
                    {
                        return new AbpLoginResult<Tenant, User>(AbpLoginResultType.InvalidUserNameOrEmailAddress, tenant);
                    }

                    if (await UserManager.IsLockedOutAsync(user))
                    {
                        return new AbpLoginResult<Tenant, User>(AbpLoginResultType.LockedOut, tenant, user);
                    }

                    if (!loggedInFromExternalSource)
                    {
                        if (!await UserManager.CheckPasswordAsync(user, plainPassword))
                        {
                            if (shouldLockout)
                            {
                                if (await TryLockOutAsync(tenantId, user.Id))
                                {
                                    return new AbpLoginResult<Tenant, User>(AbpLoginResultType.LockedOut, tenant, user);
                                }
                            }

                            return new AbpLoginResult<Tenant, User>(AbpLoginResultType.InvalidPassword, tenant, user);
                        }

                        await UserManager.ResetAccessFailedCountAsync(user);
                    }

                    return await CreateLoginResultAsync(user, tenant);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
