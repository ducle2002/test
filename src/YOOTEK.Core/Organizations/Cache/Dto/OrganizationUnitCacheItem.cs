using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Organizations.Cache.Dto
{
    public class OrganizationUnitCacheItem
    {
        public const string CacheName = "AppOrganizationUnitCache";
        public string DisplayName { get; set; }
        public string Code { get; set; }
        public long? ParentId { get; set; }
        public long Id { get; set; }
        public int? TenantId { get; set; }

    }

    public class OrganizationUnitChatCacheItem
    {
        public const string CacheName = "AppOrganizationUnitChatCache";
        public long UserId { get; set; }
        public long OrganizationUnitId { get; set; }
        public int? TenantId { get; set; }
        public long Id { get; set; }
        public string Name { get; set; }

    }
}
