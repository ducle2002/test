using Abp.Domain.Services;
using Yootek.EntityDb;
using System.Threading.Tasks;

namespace Yootek.AppManager.HomeMembers
{
    public interface IHomeMemberManager : IDomainService
    {
        Task UpdateCitizenInHomeMember(Citizen user, bool isActive, int? tenantId);
        Task<string> GetApartmentCodeOfUser(long userId, int? tenantId);
    }
}