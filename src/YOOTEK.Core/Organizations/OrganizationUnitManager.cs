using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Abp.Domain.Uow;
using Abp.Linq;
using Abp.Runtime.Caching;
using Abp.UI;
using Abp.Zero;
using Yootek.Authorization.Users;
using Yootek.Organizations;
using Yootek.Organizations.Cache;
using Yootek.Organizations.Cache.Dto;
using Yootek.Organizations.Dto;
using Yootek.Organizations.OrganizationStructure;
using Microsoft.EntityFrameworkCore;

namespace Yootek.Organizations
{
    /// <summary>
    /// Performs domain logic for Organization Units.
    /// </summary>
    public class AppOrganizationUnitManager : DomainService
    {
        protected IRepository<AppOrganizationUnit, long> OrganizationUnitRepository { get; private set; }
        protected IRepository<UserOrganizationUnit, long> UserOrganizationUnitRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private ICacheManager _cacheManager;
        public IAsyncQueryableExecuter AsyncQueryableExecuter { get; set; }
        private readonly object _syncOrgObj = new object();
        private readonly object _syncDeptObj = new object();

        public AppOrganizationUnitManager(
            IRepository<AppOrganizationUnit, long> organizationUnitRepository,
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,
            IUnitOfWorkManager unitOfWorkManager,
            ICacheManager cacheManager
            )
        {
            OrganizationUnitRepository = organizationUnitRepository;
            UserOrganizationUnitRepository = userOrganizationUnitRepository;

            LocalizationSourceName = AbpZeroConsts.LocalizationSourceName;
            AsyncQueryableExecuter = NullAsyncQueryableExecuter.Instance;
            _unitOfWorkManager = unitOfWorkManager;
            _cacheManager = cacheManager;
        }

        public async Task<List<AppOrganizationUnit>> GetAppOrganizationUnitAsync(GetOrganizationUnitInput input)
        {
            return await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (CurrentUnitOfWork.SetTenantId(input.TenantId))
                {
                    var org = OrganizationUnitRepository.GetAll()
                    .WhereIf(input.Id > 0, x => x.Id == input.Id)
                    .WhereIf(input.Type > 0, x => x.Type == input.Type)
                    .ToList();
                    return org;
                }
            });
        }

        public async Task<AppOrganizationUnit> GetAsync(long id, int? tenantId)
        {
            return await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (CurrentUnitOfWork.SetTenantId(tenantId))
                {
                    var org = await OrganizationUnitRepository.FirstOrDefaultAsync(x => x.Id == id);
                    return org;
                }
            });
        }

        public virtual async Task CreateAsync(AppOrganizationUnit organizationUnit)
        {
            using (var uow = UnitOfWorkManager.Begin())
            {
                organizationUnit.Code = await GetNextChildCodeAsync(organizationUnit.ParentId);
                // await ValidateOrganizationUnitAsync(organizationUnit);
                await OrganizationUnitRepository.InsertAsync(organizationUnit);

                await uow.CompleteAsync();
            }
        }

        public virtual void Create(AppOrganizationUnit organizationUnit)
        {
            using (var uow = UnitOfWorkManager.Begin())
            {
                organizationUnit.Code = GetNextChildCode(organizationUnit.ParentId);
                ValidateOrganizationUnit(organizationUnit);
                OrganizationUnitRepository.Insert(organizationUnit);

                uow.Complete();
            }
        }

        public virtual async Task UpdateAsync(AppOrganizationUnit organizationUnit)
        {
            // await ValidateOrganizationUnitAsync(organizationUnit);
            await OrganizationUnitRepository.UpdateAsync(organizationUnit);
        }

        public virtual void Update(AppOrganizationUnit organizationUnit)
        {
            ValidateOrganizationUnit(organizationUnit);
            OrganizationUnitRepository.Update(organizationUnit);
        }

        public virtual async Task<string> GetNextChildCodeAsync(long? parentId)
        {
            var lastChild = await GetLastChildOrNullAsync(parentId);
            if (lastChild == null)
            {
                var parentCode = (parentId != null) ? await GetCodeAsync(parentId.Value) : null;
                return AppOrganizationUnit.AppendCode(parentCode, AppOrganizationUnit.CreateCode(1));
            }

            return AppOrganizationUnit.CalculateNextCode(lastChild.Code);
        }

        public virtual string GetNextChildCode(long? parentId)
        {
            var lastChild = GetLastChildOrNull(parentId);
            if (lastChild == null)
            {
                var parentCode = parentId != null ? GetCode(parentId.Value) : null;
                return AppOrganizationUnit.AppendCode(parentCode, AppOrganizationUnit.CreateCode(1));
            }

            return AppOrganizationUnit.CalculateNextCode(lastChild.Code);
        }

        public virtual async Task<AppOrganizationUnit> GetLastChildOrNullAsync(long? parentId)
        {
            var query = OrganizationUnitRepository.GetAll()
                .Where(ou => ou.ParentId == parentId)
                .OrderByDescending(ou => ou.Code);
            return await AsyncQueryableExecuter.FirstOrDefaultAsync(query);
        }

        public virtual AppOrganizationUnit GetLastChildOrNull(long? parentId)
        {
            var query = OrganizationUnitRepository.GetAll()
                .Where(ou => ou.ParentId == parentId)
                .OrderByDescending(ou => ou.Code);
            return query.FirstOrDefault();
        }

        public virtual async Task<string> GetCodeAsync(long id)
        {
            return (await OrganizationUnitRepository.GetAsync(id)).Code;
        }

        public virtual string GetCode(long id)
        {
            return (OrganizationUnitRepository.Get(id)).Code;
        }

        public virtual async Task DeleteAsync(long id)
        {
            try
            {
                using (var uow = UnitOfWorkManager.Begin())
                {
                    var children = await FindChildrenAsync(id, true);

                    foreach (var child in children)
                    {
                        await OrganizationUnitRepository.DeleteAsync(child);
                    }

                    await OrganizationUnitRepository.DeleteAsync(id);

                    await uow.CompleteAsync();
                }
            }
            catch (Exception e) {
                throw e;
            }
        }

        public virtual void Delete(long id)
        {
            using (var uow = UnitOfWorkManager.Begin())
            {
                var children = FindChildren(id, true);

                foreach (var child in children)
                {
                    OrganizationUnitRepository.Delete(child);
                }

                OrganizationUnitRepository.Delete(id);

                uow.Complete();
            }
        }

        public virtual async Task MoveAsync(long id, long? parentId)
        {
            using (var uow = UnitOfWorkManager.Begin())
            {
                var organizationUnit = await OrganizationUnitRepository.GetAsync(id);
                if (organizationUnit.ParentId == parentId)
                {
                    await uow.CompleteAsync();
                    return;
                }

                //Should find children before Code change
                var children = await FindChildrenAsync(id, true);

                //Store old code of OU
                var oldCode = organizationUnit.Code;

                //Move OU
                organizationUnit.Code = await GetNextChildCodeAsync(parentId);
                organizationUnit.ParentId = parentId;

                await ValidateOrganizationUnitAsync(organizationUnit);

                //Update Children Codes
                foreach (var child in children)
                {
                    child.Code = AppOrganizationUnit.AppendCode(organizationUnit.Code, AppOrganizationUnit.GetRelativeCode(child.Code, oldCode));
                }

                await uow.CompleteAsync();
            }
        }

        public virtual void Move(long id, long? parentId)
        {
            UnitOfWorkManager.WithUnitOfWork(() =>
            {
                var organizationUnit = OrganizationUnitRepository.Get(id);
                if (organizationUnit.ParentId == parentId)
                {
                    return;
                }

                //Should find children before Code change
                var children = FindChildren(id, true);

                //Store old code of OU
                var oldCode = organizationUnit.Code;

                //Move OU
                organizationUnit.Code = GetNextChildCode(parentId);
                organizationUnit.ParentId = parentId;

                ValidateOrganizationUnit(organizationUnit);

                //Update Children Codes
                foreach (var child in children)
                {
                    child.Code = AppOrganizationUnit.AppendCode(organizationUnit.Code, AppOrganizationUnit.GetRelativeCode(child.Code, oldCode));
                }
            });
        }

        public async Task<List<AppOrganizationUnit>> FindChildrenAsync(long? parentId, bool recursive = false)
        {
            if (!recursive)
            {
                return await OrganizationUnitRepository.GetAllListAsync(ou => ou.ParentId == parentId);
            }

            if (!parentId.HasValue)
            {
                return await OrganizationUnitRepository.GetAllListAsync();
            }

            var code = await GetCodeAsync(parentId.Value);

            return await OrganizationUnitRepository.GetAllListAsync(
                ou => ou.Code.StartsWith(code) && ou.Id != parentId.Value
            );
        }

        public List<AppOrganizationUnit> FindChildren(long? parentId, bool recursive = false)
        {
            if (!recursive)
            {
                return OrganizationUnitRepository.GetAllList(ou => ou.ParentId == parentId);
            }

            if (!parentId.HasValue)
            {
                return OrganizationUnitRepository.GetAllList();
            }

            var code = GetCode(parentId.Value);

            return OrganizationUnitRepository.GetAllList(
                ou => ou.Code.StartsWith(code) && ou.Id != parentId.Value
            );
        }

        protected virtual async Task ValidateOrganizationUnitAsync(AppOrganizationUnit organizationUnit)
        {
            var siblings = (await FindChildrenAsync(organizationUnit.ParentId))
                .Where(ou => ou.Id != organizationUnit.Id)
                .ToList();

            if (siblings.Any(ou => ou.DisplayName == organizationUnit.DisplayName))
            {
                throw new UserFriendlyException(L("OrganizationUnitDuplicateDisplayNameWarning", organizationUnit.DisplayName));
            }
        }

        protected virtual void ValidateOrganizationUnit(AppOrganizationUnit organizationUnit)
        {
            var siblings = (FindChildren(organizationUnit.ParentId))
                .Where(ou => ou.Id != organizationUnit.Id)
                .ToList();

            if (siblings.Any(ou => ou.DisplayName == organizationUnit.DisplayName))
            {
                throw new UserFriendlyException(L("OrganizationUnitDuplicateDisplayNameWarning", organizationUnit.DisplayName));
            }
        }

        #region OrganizationUnit Department

        public void CreateDepartmentUserCache(OrganizationStructureDeptUser data)
        {
            var items = GetCacheDepartmentUserOrNull(data.TenantId??0);
            var item = data.MapTo<DepartmentUserCacheItem>();
            if (items == null || items.Count == 0)
            {
                items = new List<DepartmentUserCacheItem> { item };
            }
            else
            {
                items.Add(item);
            }

            lock (_syncDeptObj)
            {
                UpdateDepartmentUserOnCache(data.TenantId.Value, items);
            }
        }

        public void DeleteDepartmentUserCache(long id, int tenantId)
        {
            var items = GetCacheDepartmentUserOrNull(tenantId);
            if (items == null || items.Count == 0)
            {
                return;
            }
            else
            {
                var item = items.Find(x => x.Id == id);
                items.Remove(item);
            }

            lock (_syncDeptObj)
            {
                UpdateDepartmentUserOnCache(tenantId, items);
            }
        }

        public void CreateDepartmentOrganizationUnitCache(DepartmentOrganizationUnit data)
        {
            var items = GetCacheDepartmentOrganizationUnitOrNull(data.TenantId ?? 0);
            var item = data.MapTo<DepartmentOrganizationUnitCacheItem>();
            if (items == null || items.Count == 0)
            {
                items = new List<DepartmentOrganizationUnitCacheItem> { item };
            }
            else
            {
                items.Add(item);
            }

            lock (_syncOrgObj)
            {
                UpdateDepartmentOrganizationUnitOnCache(data.TenantId.Value, items);
            }
        }

        public void DeleteDepartmentOrganizationUnitCache(long id, int tenantId)
        {
            var items = GetCacheDepartmentOrganizationUnitOrNull(tenantId);
            if (items == null || items.Count == 0)
            {
                return;
            }
            else
            {
                var item = items.Find(x => x.Id == id);
                items.Remove(item);
            }

            lock (_syncDeptObj)
            {
                UpdateDepartmentOrganizationUnitOnCache(tenantId, items);
            }
        }

        #endregion

        #region Cache department
        private void UpdateDepartmentUserOnCache(int tenantId, List<DepartmentUserCacheItem> items)
        {
            _cacheManager.GetDepartmentUserCache().Set(tenantId, items);
        }

        private void UpdateDepartmentOrganizationUnitOnCache(int tenantId, List<DepartmentOrganizationUnitCacheItem> items)
        {
            _cacheManager.GetDepaprtmentOrganizationUnitCache().Set(tenantId, items);
        }

        private List<DepartmentOrganizationUnitCacheItem> GetCacheDepartmentOrganizationUnitOrNull(int tenantId)
        {
            return _cacheManager.GetDepaprtmentOrganizationUnitCache().GetOrDefault(tenantId);
        }

        private List<DepartmentUserCacheItem> GetCacheDepartmentUserOrNull(int tenantId)
        {
            return _cacheManager.GetDepartmentUserCache().GetOrDefault(tenantId);
        }
        #endregion
    }
}
