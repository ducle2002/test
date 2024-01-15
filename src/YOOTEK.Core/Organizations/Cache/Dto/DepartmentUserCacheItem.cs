using Abp.AutoMapper;
using Yootek.Organizations.OrganizationStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Organizations.Cache.Dto
{
    [AutoMap(typeof(OrganizationStructureDeptUser))]
    public class DepartmentUserCacheItem
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public long UserId { get; set; }
        public long DeptId { get; set; }
    }
}
