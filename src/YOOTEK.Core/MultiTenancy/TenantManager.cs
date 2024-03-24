using Abp.Application.Features;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.MultiTenancy;
using Yootek.Authorization.Users;
using Yootek.Editions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.UI;
using System.Text.RegularExpressions;

namespace Yootek.MultiTenancy
{
    public class TenantManager : AbpTenantManager<Tenant, User>
    {
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        public TenantManager(
            IRepository<Tenant> tenantRepository,
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<TenantFeatureSetting, long> tenantFeatureRepository,
            EditionManager editionManager,
            IAbpZeroFeatureValueStore featureValueStore)
            : base(
                tenantRepository,
                tenantFeatureRepository,
                editionManager,
                featureValueStore)
        {
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task<List<Tenant>> GetAllTenantAsync()
        {
            try
            {
                var tenants = await TenantRepository.GetAllListAsync();

                return tenants;

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public override Tenant FindByTenancyName(string tenancyName)
        {
            return TenantRepository.FirstOrDefault(t => t.TenancyName.ToLower() == tenancyName.ToLower() || t.SubName.ToLower() == tenancyName.ToLower());
        }

        public override Task<Tenant> FindByTenancyNameAsync(string tenancyName)
        {
            return TenantRepository.FirstOrDefaultAsync(t => t.TenancyName.ToLower() == tenancyName.ToLower() || t.SubName.ToLower() == tenancyName.ToLower());
        }

        public Task<Tenant> FindByTenancyErpNameAsync(string tenancyName)
        {
            return TenantRepository.FirstOrDefaultAsync(t => (t.TenancyName.ToLower() == tenancyName.ToLower() || t.SubName.ToLower() == tenancyName.ToLower()) && t.TenantType == TenantType.ERP);
        }

        protected override Task ValidateTenancyNameAsync(string tenancyName)
        {
            if (!Regex.IsMatch(tenancyName, Tenant.NTenancyNameRegex))
            {
                throw new UserFriendlyException(L("InvalidTenancyName"));
            }

            return Task.FromResult(0);
        }

        protected override void ValidateTenancyName(string tenancyName)
        {
            if (!Regex.IsMatch(tenancyName, Tenant.NTenancyNameRegex))
            {
                throw new UserFriendlyException(L("InvalidTenancyName"));
            }
        }
    }
}
