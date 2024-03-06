using Abp.Application.Services.Dto;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Localization;
using Abp.MultiTenancy;
using Abp.Runtime.Security;
using AutoMapper.Internal.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yootek.Authorization.Roles;
using Yootek.Authorization.Users;
using Yootek.Authorization;
using Yootek.Configuration;
using Yootek.Editions;
using Yootek.MultiTenancy.Dto;
using Yootek.MultiTenancy;
using Yootek;
using Yootek.Authorization.Accounts.Dto;

namespace YOOTEK.Application.MultiTenancy
{
    public interface ITenantErpAppService : IApplicationService
    {

    }

    public class TenantErpAppService : YootekAppServiceBase, ITenantErpAppService
    {
        private readonly IRepository<Tenant, int> _tenantRepository;

        public TenantErpAppService(
            IRepository<Tenant, int> repository
            )
        {
            _tenantRepository = repository;
        }

        public async Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input)
        {
            if (string.IsNullOrWhiteSpace(input.TenancyName))
            {
                return new IsTenantAvailableOutput(TenantAvailabilityState.NotFound);
            }

            var tenant = await _tenantRepository.FirstOrDefaultAsync(t => (t.TenancyName.ToLower() == input.TenancyName.ToLower()
            || t.SubName.ToLower() == input.TenancyName.ToLower())
            && (t.TenantType == TenantType.RETAIL || t.TenantType == TenantType.FNB));
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
    }
}
