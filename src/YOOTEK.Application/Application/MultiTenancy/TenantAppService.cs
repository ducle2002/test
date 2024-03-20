using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Events.Bus;
using Abp.Extensions;
using Abp.IdentityFramework;
using Abp.Linq.Extensions;
using Abp.MultiTenancy;
using Abp.Runtime.Security;
using Yootek.Authorization;
using Yootek.Authorization.Roles;
using Yootek.Authorization.Users;
using Yootek.Configuration;
using Yootek.Editions;
using Yootek.MultiTenancy.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Yootek.MultiTenancy
{
    /// <summary>
    /// [AbpAuthorize(PermissionNames.Pages_Tenants)]
    /// </summary>
    public class TenantAppService :
        AsyncCrudAppService<Tenant, TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>,
        ITenantAppService
    {
        private readonly TenantManager _tenantManager;
        private readonly EditionManager _editionManager;
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;
        private readonly IPermissionManager _permissionManager;
        private readonly IAbpZeroDbMigrator _abpZeroDbMigrator;
        private readonly IConfigurationRoot _appConfiguration;

        public TenantAppService(
            IRepository<Tenant, int> repository,
            TenantManager tenantManager,
            EditionManager editionManager,
            UserManager userManager,
            RoleManager roleManager,
            IPermissionManager permissionManager,
            IAppConfigurationAccessor configurationAccessor,
            IAbpZeroDbMigrator abpZeroDbMigrator)
            : base(repository)
        {
            _tenantManager = tenantManager;
            _editionManager = editionManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _abpZeroDbMigrator = abpZeroDbMigrator;
            _permissionManager = permissionManager;
            _appConfiguration = configurationAccessor.Configuration;
        }


        public override async Task<TenantDto> CreateAsync(CreateTenantDto input)
        {
            try
            {
                CheckCreatePermission();

                // Create tenant
                var tenant = ObjectMapper.Map<Tenant>(input);
                tenant.ConnectionString = input.ConnectionString.IsNullOrEmpty()
                    ? null
                    : SimpleStringCipher.Instance.Encrypt(input.ConnectionString);

                var defaultEdition = await _editionManager.FindByNameAsync(EditionManager.DefaultEditionName);
                if (defaultEdition != null)
                {
                    tenant.EditionId = defaultEdition.Id;
                }

                await _tenantManager.CreateAsync(tenant);
                await CurrentUnitOfWork.SaveChangesAsync(); // To get new tenant's id.

                // Create tenant database
                _abpZeroDbMigrator.CreateOrMigrateForTenant(tenant);

                // We are working entities of new tenant, so changing tenant filter
                using (CurrentUnitOfWork.SetTenantId(tenant.Id))
                {
                    // Create static roles for new tenant
                    CheckErrors(await _roleManager.CreateStaticRoles(tenant.Id));

                    await CurrentUnitOfWork.SaveChangesAsync(); // To get static role ids
                    //role user default


                    // Grant all permissions to admin role
                    var adminRole = _roleManager.Roles.Single(r => r.Name == StaticRoleNames.Tenants.Admin);
                    await _roleManager.GrantAllPermissionsAsync(adminRole);

                    // Create admin user for the tenant
                    var adminUser = User.CreateTenantAdminUser(tenant.Id, input.AdminEmailAddress);
                    await _userManager.InitializeOptionsAsync(tenant.Id);
                    CheckErrors(await _userManager.CreateAsync(adminUser, User.DefaultPassword));
                    await CurrentUnitOfWork.SaveChangesAsync(); // To get admin user's id

                    // Assign admin user to role!
                    CheckErrors(await _userManager.AddToRoleAsync(adminUser, adminRole.Name));
                    await CurrentUnitOfWork.SaveChangesAsync();
                }

                // await _appNotifier.NewTenantRegisteredAsync(tenant);
                return MapToEntityDto(tenant);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        //[AbpAuthorize(PermissionNames.Pages_Tenants_Edit)]
        //public async Task UpdateTenant(TenantEditDto input)
        //{
        //    await _tenantManager.CheckEditionAsync(input.EditionId, input.IsInTrialPeriod);

        //    input.ConnectionString = SimpleStringCipher.Instance.Encrypt(input.ConnectionString);
        //    var tenant = await _tenantManager.GetByIdAsync(input.Id);

        //    if (tenant.EditionId != input.EditionId)
        //    {
        //        await EventBus.TriggerAsync(new TenantEditionChangedEventData
        //        {
        //            TenantId = input.Id,
        //            OldEditionId = tenant.EditionId,
        //            NewEditionId = input.EditionId
        //        });
        //    }

        //    ObjectMapper.Map(input, tenant);
        //    tenant.SubscriptionEndDateUtc = tenant.SubscriptionEndDateUtc?.ToUniversalTime();

        //    await _tenantManager.UpdateAsync(tenant);
        //}

        [AbpAuthorize(IOCPermissionNames.Pages_Tenants)]
        public async Task UpdateConfigTenant(UpdateConfigTenantDto input)
        {
            try
            {
                var tenant = await _tenantManager.GetByIdAsync(input.Id);
                if (tenant != null)
                {
                    tenant.AdminPageConfig = input.AdminPageConfig;
                    tenant.MobileConfig = input.MobileConfig;
                    //if(input.MobileConfig != null)
                    //{
                    //    if(input.MobileConfig.MobileVersion == null)
                    //    {
                    //        input.MobileConfig.MobileVersion = Mobile_Version;
                    //    }
                    //    tenant.MobileConfig = JsonSerializer.Serialize(input.MobileConfig);
                    //}
                    //else
                    //{
                    //    input.MobileConfig = new MobileConfigDto();
                    //    input.MobileConfig.MobileVersion = Mobile_Version;
                    //    tenant.MobileConfig = JsonSerializer.Serialize(input.MobileConfig);
                    //}
                    await _tenantManager.UpdateAsync(tenant);
                }
            }
            catch (Exception e)
            {
            }
        }

        protected override IQueryable<Tenant> CreateFilteredQuery(PagedTenantResultRequestDto input)
        {
            return Repository.GetAll()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(),
                    x => x.TenancyName.Contains(input.Keyword) || x.Name.Contains(input.Keyword))
                .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive);
        }

        protected override void MapToEntity(TenantDto updateInput, Tenant entity)
        {
            // Manually mapped since TenantDto contains non-editable properties too.
            entity.Name = updateInput.Name;
            entity.TenancyName = updateInput.TenancyName;
            entity.IsActive = updateInput.IsActive;
            entity.TenantType = updateInput.TenantType;
        }

        public override async Task DeleteAsync(EntityDto<int> input)
        {
            CheckDeletePermission();

            var tenant = await _tenantManager.GetByIdAsync(input.Id);
            await _tenantManager.DeleteAsync(tenant);
        }

        private void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}