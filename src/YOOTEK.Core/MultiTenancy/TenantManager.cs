using Abp.Application.Features;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.MultiTenancy;
using IMAX.Authorization.Users;
using IMAX.Editions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IMAX.MultiTenancy
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
    }
}
