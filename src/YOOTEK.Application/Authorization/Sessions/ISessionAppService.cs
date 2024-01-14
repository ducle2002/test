using System.Threading.Tasks;
using Abp.Application.Services;
using Yootek.Sessions.Dto;

namespace Yootek.Sessions
{
    public interface ISessionAppService : IApplicationService
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
    }
}
