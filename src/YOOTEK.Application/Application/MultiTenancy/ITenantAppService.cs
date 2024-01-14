using Abp.Application.Services;
using Yootek.MultiTenancy.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Yootek.MultiTenancy
{
    public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
    {
        Task UpdateConfigTenant(UpdateConfigTenantDto input);
    }
}

