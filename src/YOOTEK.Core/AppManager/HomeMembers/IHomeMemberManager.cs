using Abp.Domain.Services;
using IMAX.EntityDb;
using System.Threading.Tasks;

namespace IMAX.AppManager.HomeMembers
{
    public interface IHomeMemberManager : IDomainService
    {
        Task UpdateCitizenInHomeMember(Citizen user, bool isActive, int? tenantId);
        Task<string> GetApartmentCodeOfUser(long userId, int? tenantId);
    }
}