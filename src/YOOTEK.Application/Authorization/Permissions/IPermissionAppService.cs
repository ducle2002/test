using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using IMAX.Authorization.Permissions.Dto;

namespace IMAX.Authorization.Permissions
{
    public interface IPermissionAppService : IApplicationService
    {
        Task<ListResultDto<FlatPermissionWithLevelDto>> GetAllPermissions(GetAllPermissionsDto input);
        Task<object> AddPermissionForTenant(AddPermissionForTenantDto input);
    }
}
