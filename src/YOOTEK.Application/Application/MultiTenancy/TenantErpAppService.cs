using Abp.Application.Services;
using System.Threading.Tasks;
using Yootek.MultiTenancy;
using Yootek;
using Yootek.Authorization.Accounts.Dto;
using Yootek.Common.DataResult;
using Abp.Web.Models;

namespace YOOTEK.Application.MultiTenancy
{
    public interface ITenantErpAppService : IApplicationService
    {

    }

    public class TenantErpAppService : YootekAppServiceBase, ITenantErpAppService
    {
        private readonly TenantManager _tenantManager;

        public TenantErpAppService(
            TenantManager tenantManager
            )
        {
            _tenantManager = tenantManager;
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

        [DontWrapResult]
        public async Task<DataResult> GetTenantByTenancyName(string tenancyName)
        {
            if (string.IsNullOrWhiteSpace(tenancyName))
            {
                return DataResult.ResultSuccess(null, "");
            }

            var tenant = await _tenantManager.FindByTenancyErpNameAsync(tenancyName);
            if (tenant == null)
            {
                return DataResult.ResultSuccess(null, "");
            }

            if (!tenant.IsActive)
            {
                return DataResult.ResultSuccess(null, "");
            }

            return DataResult.ResultSuccess(tenant, "");
        }
    }

}
