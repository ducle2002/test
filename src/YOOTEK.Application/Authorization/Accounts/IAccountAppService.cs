using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Yootek.Authorization.Accounts.Dto;

namespace Yootek.Authorization.Accounts
{
    public interface IAccountAppService : IApplicationService
    {
        Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

        Task<RegisterOutput> Register(RegisterInput input);
        Task<object> GetAllTenantName();
        Task SyncAccount(SyncAccountInput input);
    }
}
