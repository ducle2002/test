using Abp.Configuration;
using Abp.Runtime.Caching;
using Yootek.Organizations.Cache.Dto;
using Yootek.Organizations.OrganizationStructure;
using System;
using System.Collections.Generic;

namespace Yootek.Organizations.Cache
{
    /// </summary>
    public static class CacheManagerUserBuildingUrbanExtension
    {
        public static ITypedCache<string, List<long>> GetBuildingUrbanCache(this ICacheManager cacheManager)
        {
            return cacheManager
                .GetCache<string, List<long>>("BuildingOrUrbanIds");
        }

        public static ITypedCache<string, List<long>> GetOperationDepartmentCache(this ICacheManager cacheManager)
        {
            return cacheManager
                .GetCache<string, List<long>>("OperationDepartmentIds");
        }

        public static ITypedCache<int, List<DepartmentOrganizationUnitCacheItem>> GetDepaprtmentOrganizationUnitCache(this ICacheManager cacheManager)
        {
            return cacheManager
                .GetCache<int, List<DepartmentOrganizationUnitCacheItem>>("DepaprtmentOrganizationUnits");
        }

        public static ITypedCache<int, List<DepartmentUserCacheItem>> GetDepartmentUserCache(this ICacheManager cacheManager)
        {
            return cacheManager
                .GetCache<int, List<DepartmentUserCacheItem>>("DepartmentUsers");
        }
    }


}
