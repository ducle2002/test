using System.Threading.Tasks;
using Abp.Application.Services;
using IMAX.Sessions.Dto;

namespace IMAX.Sessions
{
    public interface ISessionAppService : IApplicationService
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
    }
}
