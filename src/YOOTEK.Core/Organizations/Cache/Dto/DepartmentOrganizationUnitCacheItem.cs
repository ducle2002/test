using Abp.AutoMapper;
using IMAX.Organizations.OrganizationStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAX.Organizations.Cache.Dto
{
    [AutoMap(typeof(DepartmentOrganizationUnit))]
    public class DepartmentOrganizationUnitCacheItem
    {
        public long Id { get; set; }
        public long DeptId { get; set; }
        public long OrganizationUnitId { get; set; }
        public int? TenantId { get; set; }
    }
}
