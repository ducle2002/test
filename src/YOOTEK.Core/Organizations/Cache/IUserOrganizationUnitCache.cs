using Yootek.Organizations.Cache.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Organizations.Cache
{
    public interface IUserOrganizationUnitCache
    {
     
        Task<List<OrganizationUnitChatCacheItem>> GetOrganizationUnitChatById(long organizationUnitId, int? tenantId = null);
        Task<List<FriendChatOrganizationUnitCacheItem>> GetFriendChatOrganizationUnit(long organizationUnitId, int? tenantId = null);

    }
}
