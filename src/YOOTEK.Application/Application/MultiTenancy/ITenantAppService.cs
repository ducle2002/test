using Abp.Application.Services;
using IMAX.MultiTenancy.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IMAX.MultiTenancy
{
    public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
    {
        Task UpdateConfigTenant(UpdateConfigTenantDto input);
    }
}

