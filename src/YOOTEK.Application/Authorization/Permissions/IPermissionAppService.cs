using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Yootek.Authorization.Permissions.Dto;

namespace Yootek.Authorization.Permissions
{
    public interface IPermissionAppService : IApplicationService
    {
        Task<ListResultDto<FlatPermissionWithLevelDto>> GetAllPermissions(GetAllPermissionsDto input);
        Task<object> AddPermissionForTenant(AddPermissionForTenantDto input);
    }
}
