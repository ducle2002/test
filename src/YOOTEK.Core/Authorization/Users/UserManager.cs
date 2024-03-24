using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Organizations;
using Abp.Runtime.Caching;
using Yootek.Authorization.Roles;
using System.Threading.Tasks;
using Abp;
using Abp.Threading;
using Yootek.MultiTenancy;
using System.Linq;
using Yootek.Organizations;
using Abp.UI;
using Abp.MultiTenancy;
using System.Security.Claims;
using Yootek.Organizations.Cache;
using Abp.Runtime.Session;
using Microsoft.EntityFrameworkCore;
using Yootek.Organizations.OrganizationStructure;
using Yootek.Organizations.Cache.Dto;
using Abp.AutoMapper;
using Abp.Json;
using Abp.Linq.Extensions;
using Org.BouncyCastle.Crypto;
using Twilio.Types;

namespace Yootek.Authorization.Users
{
    public class UserManager : AbpUserManager<Role, User>
    {
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<User, long> _userRepos;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationUnitRepository;
        private readonly IRepository<AppOrganizationUnit, long> _organizationUnitRepository;
        private readonly IRepository<DepartmentOrganizationUnit, long> _dtOuRepo;
        private readonly IRepository<OrganizationStructureDeptUser, long> _deptUserRepo;
        private readonly IRepository<Tenant> _tenantRepository;
        private readonly ITenantCache _tenantCache;
        private readonly ICacheManager _cacheManager;
        private readonly object _syncObj = new object();
        private readonly object _syncOrgObj = new object();

        public UserManager(
            RoleManager roleManager,
            UserStore store,
            IRepository<User, long> userRepos,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<User> passwordHasher,
            IEnumerable<IUserValidator<User>> userValidators,
            IEnumerable<IPasswordValidator<User>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<User>> logger,
            IPermissionManager permissionManager,
            IUnitOfWorkManager unitOfWorkManager,
            ICacheManager cacheManager,
            ITenantCache tenantCache,
            IRepository<OrganizationUnit, long> organizationUnitRepository,
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,
            IRepository<AppOrganizationUnit, long> appOrganizationUnitRepository,
            IRepository<DepartmentOrganizationUnit, long> dtOuRepo,
            IRepository<OrganizationStructureDeptUser, long> deptUserRepo,
            IOrganizationUnitSettings organizationUnitSettings,
            IRepository<Tenant> tenantRepository,
            ISettingManager settingManager,
            IRepository<UserLogin, long> userLoginRepository)
            : base(roleManager,
                   store,
                   optionsAccessor,
                   passwordHasher,
                   userValidators,
                   passwordValidators,
                   keyNormalizer,
                   errors,
                   services,
                   logger,
                   permissionManager,
                   unitOfWorkManager,
                   cacheManager,
                   organizationUnitRepository,
                   userOrganizationUnitRepository,
                   organizationUnitSettings,
                   settingManager,
                   userLoginRepository)
        {
            _unitOfWorkManager = unitOfWorkManager;
            _userRepos = userRepos;
            _userOrganizationUnitRepository = userOrganizationUnitRepository;
            _organizationUnitRepository = appOrganizationUnitRepository;
            _tenantCache = tenantCache;
            _tenantRepository = tenantRepository;
            _cacheManager = cacheManager;
            _dtOuRepo = dtOuRepo;
            _deptUserRepo = deptUserRepo;
        }

        #region Base user
        public override async Task<IdentityResult> CreateAsync(User user)
        {
            var tenantId = GetCurrentTenantId();
            if (tenantId.HasValue && !user.TenantId.HasValue)
            {
                user.TenantId = tenantId.Value;
            }

            var result = await CheckDuplicateUsernameOrEmailAddressAsync(user.Id, user.UserName, user.EmailAddress, false);
            if (!result.Succeeded)
            {
                return result;
            }
            await InitializeOptionsAsync(user.TenantId);
            return await base.CreateAsync(user);
        }

        public override async Task<IdentityResult> CheckDuplicateUsernameOrEmailAddressAsync(long? expectedUserId, string userName, string emailAddress)
        {
            var user = (await FindByNameAsync(userName));
            if (user != null && user.Id != expectedUserId)
            {
                throw new UserFriendlyException(string.Format(L("Identity.DuplicateUserName"), userName));
            }

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> CheckDuplicateUsernameOrEmailAddressAsync(long? expectedUserId, string userName, string emailAddress, bool isCheckEmail)
        {
            var user = (await FindByNameAsync(userName));
            if (user != null && user.Id != expectedUserId)
            {
                throw new UserFriendlyException(string.Format(L("Identity.DuplicateUserName"), userName));
            }

            if (isCheckEmail)
            {
                user = (await FindByEmailAsync(emailAddress));
                if (user != null && user.Id != expectedUserId)
                {
                    throw new UserFriendlyException(string.Format(L("Identity.DuplicateEmail"), emailAddress));
                }
            }

            return IdentityResult.Success;
        }

        public async Task<User> GetUserOrNullAsync(UserIdentifier userIdentifier)
        {
            return await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (_unitOfWorkManager.Current.SetTenantId(userIdentifier.TenantId))
                {
                    return await FindByIdAsync(userIdentifier.UserId.ToString());
                }
            });
        }

        public User GetUserOrNull(UserIdentifier userIdentifier)
        {
            return AsyncHelper.RunSync(() => GetUserOrNullAsync(userIdentifier));
        }

        public async Task<User> GetUserAsync(UserIdentifier userIdentifier)
        {
            var user = await GetUserOrNullAsync(userIdentifier);
            return user;
        }

        public User GetUser(UserIdentifier userIdentifier)
        {
            return AsyncHelper.RunSync(() => GetUserAsync(userIdentifier));
        }


        public async Task ChangeAvatar(string imageUrl, long userId)
        {
            try
            {
                await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
                {
                    using (_unitOfWorkManager.Current.SetTenantId(null))
                    {
                        var user = await _userRepos.GetAsync(userId);

                        if (user != null)
                        {
                            user.ImageUrl = imageUrl;
                            await _unitOfWorkManager.Current.SaveChangesAsync();
                        }
                    }
                });

            }
            catch (Exception e)
            {
                Logger.LogError("Exception", e.ToString());
            }
        }

        public async Task<List<User>> FindUserbyKeywordAsync(List<long> currentFriends, string key, int? tenantId, int skip, int take)
        {
            try
            {
                var userId = AbpSession.UserId;
                if (currentFriends == null) currentFriends = new List<long>();
                key = key.ToLower();
                using (_unitOfWorkManager.Current.SetTenantId(tenantId))
                {
                    var users = _userRepos.GetAll().Where(x => x.Id != userId && !currentFriends.Contains(x.Id) && (x.EmailAddress.ToLower().Contains(key) || x.UserName.ToLower().Contains(key)
                    || x.PhoneNumber.ToLower().Contains(key))).Skip(skip).Take(take).ToList();
                    return users;
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
                return null;
            }
        }
        private int? GetCurrentTenantId()
        {
            if (_unitOfWorkManager.Current != null)
            {
                return _unitOfWorkManager.Current.GetTenantId();
            }

            return AbpSession.TenantId;
        }
        #endregion

        #region Organization

        public async Task<List<AppOrganizationUnit>> GetAppOrganizationUnitsAsync(User user)
        {
            var result = _unitOfWorkManager.WithUnitOfWork(() =>
            {
                var query = from uou in _userOrganizationUnitRepository.GetAll()
                            join ou in _organizationUnitRepository.GetAll() on uou.OrganizationUnitId equals ou.Id
                            where uou.UserId == user.Id
                            select ou;

                return query.ToList();
            });

            return await Task.FromResult(result);
        }

        public override async Task AddToOrganizationUnitAsync(long userId, long ouId)
        {
            await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                await AddToOrganizationUnitAsync(
                    await GetUserByIdAsync(userId),
                    await _organizationUnitRepository.GetAsync(ouId)
                );
            });
        }

        public async Task AddToOrganizationUnitAsync(User user, AppOrganizationUnit ou)
        {
            await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                var currentOus = await GetAppOrganizationUnitsAsync(user);

                if (currentOus.Any(cou => cou.Id == ou.Id))
                {
                    return;
                }

                //  await CheckMaxUserOrganizationUnitMembershipCountAsync(user.TenantId, currentOus.Count + 1);

                await _userOrganizationUnitRepository.InsertAsync(new UserOrganizationUnit(user.TenantId, user.Id,
                    ou.Id));

                // add user org to cache
                var types = await CheckOrganizationUnitType(ou.Id, ou.TenantId);
                if (!types.Any()) return;
                if (types.Contains(APP_ORGANIZATION_TYPE.BUILDING) || types.Contains(APP_ORGANIZATION_TYPE.URBAN))
                {
                    AddUserBuildingUrbanId(user, ou.Id);
                }
                else
                {
                    AddUserOperationDepartmentId(user, ou.Id);
                }

            });
        }

        public async Task AddToUserBuildingAsync(User user, AppOrganizationUnit ou)
        {
            await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                var currentOus = await GetAppOrganizationUnitsAsync(user);

                if (currentOus.Any(cou => cou.Id == ou.Id))
                {
                    return;
                }
                await _userOrganizationUnitRepository.InsertAsync(new UserOrganizationUnit(user.TenantId, user.Id,
                    ou.Id));
                AddUserBuildingUrbanId(user, ou.Id);
            });
        }

        public override async Task RemoveFromOrganizationUnitAsync(long userId, long ouId)
        {
            await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                await RemoveFromOrganizationUnitAsync(
                    await GetUserByIdAsync(userId),
                    await _organizationUnitRepository.GetAsync(ouId)
                );
               
            });
        }

        public async Task RemoveFromOrganizationUnitAsync(User user, AppOrganizationUnit ou)
        {
            await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                await _userOrganizationUnitRepository.DeleteAsync(uou =>
                    uou.UserId == user.Id && uou.OrganizationUnitId == ou.Id
                );
                RemoveUserBuildingUrbanId(user, ou.Id);
                // add user org to cache
                var types = await CheckOrganizationUnitType(ou.Id, ou.TenantId);
                if (!types.Any()) return;
                if (types.Contains(APP_ORGANIZATION_TYPE.BUILDING) || types.Contains(APP_ORGANIZATION_TYPE.URBAN))
                {
                    RemoveUserBuildingUrbanId(user, ou.Id);
                }
                else
                {
                    RemoveOperationDepartmentId(user, ou.Id);
                }
            });
        }

        public override async Task SetOrganizationUnitsAsync(long userId, params long[] organizationUnitIds)
        {
            await SetOrganizationUnitsAsync(
                await GetUserByIdAsync(userId),
                organizationUnitIds
            );
        }

        public async Task<List<UserIdentifier>> GetUserOrganizationUnitByType(APP_ORGANIZATION_TYPE type, long? parentId = null)
        {
            try
            {
                using (_unitOfWorkManager.Current.SetTenantId(AbpSession.TenantId))
                {
                    var parent = await _organizationUnitRepository.FirstOrDefaultAsync(parentId?? 0);
                    var ids = await _organizationUnitRepository.GetAll()
                        .Where(x => x.Type == type)
                        .WhereIf(parentId.HasValue && parent != null, x => x.Code.StartsWith(parent.Code))
                        .Select(x => x.ParentId ?? 0)
                        .Distinct()
                        .ToListAsync();
                    var users = await _userOrganizationUnitRepository.GetAll().
                        Where(x => ids.Contains(x.OrganizationUnitId))
                        .Select(x => new UserIdentifier(x.TenantId, x.UserId))
                        .ToListAsync();
                    return users;
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToJsonString());
                return null;
            }
        }
        public async Task<List<UserIdentifier>> GetUserOrganizationUnitByUrban(long urbanId)
        {
            try
            {
                using (_unitOfWorkManager.Current.SetTenantId(AbpSession.TenantId))
                {
                    var urban = await _organizationUnitRepository.FirstOrDefaultAsync(urbanId);
                    if (urban == null) return null;
                    var ids = await _organizationUnitRepository.GetAll().Where(x => x.Code.StartsWith(urban.Code) && x.Type == APP_ORGANIZATION_TYPE.REPRESENTATIVE_NAME).Select(x => x.Id).Distinct().ToListAsync();
                    var users = await _userOrganizationUnitRepository.GetAll()
                        .Where(x => ids.Contains(x.OrganizationUnitId))
                        .Select(x => new UserIdentifier(x.TenantId, x.UserId))
                        .ToListAsync();
                    return users;
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToJsonString());
                return null;
            }
        }

        public async Task<List<UserIdentifier>> GetUserOrganizationUnitByUrbanOrNull(long? urbanId)
        {
            try
            {
                using (_unitOfWorkManager.Current.SetTenantId(AbpSession.TenantId))
                {
                    var urban = await _organizationUnitRepository.FirstOrDefaultAsync(urbanId??0);
                    if (urbanId > 0 || urban == null)
                    {
                        var users = await _userOrganizationUnitRepository.GetAll()
                           .Select(x => new UserIdentifier(x.TenantId, x.UserId))
                           .Distinct()
                           .ToListAsync();
                        return users;
                    }
                    else
                    {
                        var ids = await _organizationUnitRepository.GetAll().Where(x => x.Code.StartsWith(urban.Code) && x.Type == APP_ORGANIZATION_TYPE.REPRESENTATIVE_NAME).Select(x => x.Id).Distinct().ToListAsync();
                        var users = await _userOrganizationUnitRepository.GetAll()
                            .Where(x => ids.Contains(x.OrganizationUnitId))
                            .Select(x => new UserIdentifier(x.TenantId, x.UserId))
                            .Distinct()
                            .ToListAsync();
                        return users;
                    }
                   
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToJsonString());
                return null;
            }
        }

        private async Task<List<APP_ORGANIZATION_TYPE>> CheckOrganizationUnitType(long id, int? tenantId)
        {
            return await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using(_unitOfWorkManager.Current.SetTenantId(tenantId))
                {
                    var types = await _organizationUnitRepository.GetAll().Where(x => x.ParentId == id).Select(x => x.Type).ToListAsync();
                    return types;
                }
               
            });
        }
        #endregion

        #region Cache OrganizationUnit accessible
        public List<long> GetAccessibleOperationDepartmentIds()
        {
            var ids = new List<long>();
            try
            {
                using (_unitOfWorkManager.Current.SetTenantId(AbpSession.TenantId))
                {
                    var cacheIds = _cacheManager.GetOperationDepartmentCache().Get(AbpSession.ToUserIdentifier().ToUserIdentifierString(), f => GetUserOperationDepartmentIds());

                    if (cacheIds != null)
                    {
                        ids = cacheIds;
                    }

                    var departmentIs = GetAccessibleDepartmentIds();
                    if (departmentIs != null && departmentIs.Count() > 0)
                    {
                        ids = ids.Concat(departmentIs).Distinct().ToList();
                    }
                }
            }
            catch { }

            return ids;
        }


        private List<long> GetUserOperationDepartmentIds()
        {
            using (_unitOfWorkManager.Current.SetTenantId(AbpSession.TenantId))
            {
                var bus = _organizationUnitRepository.GetAllList(x => x.ParentId.HasValue && (x.Type != APP_ORGANIZATION_TYPE.BUILDING && x.Type != APP_ORGANIZATION_TYPE.URBAN));
                var buids = bus.Select(x => x.ParentId.Value).ToList();

                var uoIds = _userOrganizationUnitRepository.GetAllList(x => buids.Contains(x.OrganizationUnitId) && x.UserId == AbpSession.UserId.Value).Select(x => x.OrganizationUnitId).ToList();
                return uoIds;
            }

        }

        private void AddUserOperationDepartmentId(User user, long buildingId)
        {
            var ids = GetCacheOperationDepartmentIdOrNull(user.ToUserIdentifier());
            if (ids == null || ids.Count == 0)
            {
                ids = new List<long> { buildingId };
            }
            else
            {
                ids.Add(buildingId);
            }

            lock (_syncOrgObj)
            {
                UpdateOperationDepartmentIdOnCache(user.ToUserIdentifier(), ids.Distinct().ToList());
            }

        }

        private void RemoveOperationDepartmentId(User user, long buildingId)
        {
            var ids = GetCacheOperationDepartmentIdOrNull(user.ToUserIdentifier());
            if (ids == null || ids.Count == 0)
            {
                return;
            }
            else
            {
                ids.Remove(buildingId);
            }

            lock (_syncOrgObj)
            {
                UpdateOperationDepartmentIdOnCache(user.ToUserIdentifier(), ids);
            }

        }

        private void UpdateOperationDepartmentIdOnCache(UserIdentifier userIdentifier, List<long> ids)
        {
            _cacheManager.GetOperationDepartmentCache().Set(userIdentifier.ToUserIdentifierString(), ids);
        }

        private List<long> GetCacheOperationDepartmentIdOrNull(UserIdentifier userIdentifier)
        {
            return _cacheManager.GetOperationDepartmentCache().GetOrDefault(userIdentifier.ToUserIdentifierString());
        }

        #endregion

        #region Cache Building or Urban

        public List<long> GetAccessibleBuildingOrUrbanIds()
        {
            var ids = new List<long>();
            try
            {
                using(_unitOfWorkManager.Current.SetTenantId(AbpSession.TenantId))
                {
                    var cacheIds = _cacheManager.GetBuildingUrbanCache().Get(AbpSession.ToUserIdentifier().ToUserIdentifierString(), f => GetUserBuildingOrUrbanIds());

                    if (cacheIds != null)
                    {
                        ids = cacheIds;
                    }
                    var cacheOuIds = GetAccessibleOperationDepartmentIds();
                    if(cacheOuIds != null && cacheOuIds.Count() > 0)
                    {
                        var codes = _organizationUnitRepository.GetAllList(x => cacheOuIds.Contains(x.Id)).Select(x => x.Code.Split('.')[0]).Distinct().ToList();
                        var urbans = _organizationUnitRepository.GetAllList(x =>  codes.Contains(x.Code)).Select(x => x.Id).ToList();
                        ids = ids.Concat(urbans).Distinct().ToList();
                    }
                }
            }
            catch { }

            return ids;
        }

        private List<long> GetUserBuildingOrUrbanIds()
        {
            using (_unitOfWorkManager.Current.SetTenantId(AbpSession.TenantId))
            {
                var bus = _organizationUnitRepository.GetAllList(x => x.ParentId.HasValue && (x.Type == APP_ORGANIZATION_TYPE.BUILDING || x.Type == APP_ORGANIZATION_TYPE.URBAN));
                var buids = bus.Select(x => x.ParentId.Value).ToList();

                var uoIds = _userOrganizationUnitRepository.GetAllList(x => buids.Contains(x.OrganizationUnitId) && x.UserId == AbpSession.UserId.Value).Select(x => x.OrganizationUnitId).ToList();
                return uoIds;
            }
          
        }

        private void AddUserBuildingUrbanId(User user, long buildingId)
        {
            var ids = GetCacheBuildingUrbanIdOrNull(user.ToUserIdentifier());
            if(ids == null || ids.Count == 0)
            {
                ids = new List<long> { buildingId };
            }
            else
            {
                ids.Add(buildingId);
            }

            lock (_syncObj)
            {
                UpdateBuildingUrbanIdOnCache(user.ToUserIdentifier(), ids.Distinct().ToList());
            }

        }

        private void RemoveUserBuildingUrbanId(User user, long buildingId)
        {
            var ids = GetCacheBuildingUrbanIdOrNull(user.ToUserIdentifier());
            if (ids == null || ids.Count == 0)
            {
                return;
            }
            else
            {
                ids.Remove(buildingId);
            }

            lock (_syncObj)
            {
                UpdateBuildingUrbanIdOnCache(user.ToUserIdentifier(), ids);
            }

        }

        private void UpdateBuildingUrbanIdOnCache(UserIdentifier userIdentifier, List<long> ids)
        {
            _cacheManager.GetBuildingUrbanCache().Set(userIdentifier.ToUserIdentifierString(), ids);
        }

        private List<long> GetCacheBuildingUrbanIdOrNull(UserIdentifier userIdentifier)
        {
            return _cacheManager.GetBuildingUrbanCache().GetOrDefault(userIdentifier.ToUserIdentifierString());
        }

        public async Task InitUsersClaimOrganizationUnit()
        {
            await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                var tenant = _tenantRepository.GetAllList();
                foreach (var tenantItem in tenant)
                {
                    using (_unitOfWorkManager.Current.SetTenantId(tenantItem.Id))
                    {
                        try
                        {
                            var bus = await _organizationUnitRepository.GetAllListAsync(x => x.ParentId.HasValue && (x.Type == APP_ORGANIZATION_TYPE.BUILDING || x.Type == APP_ORGANIZATION_TYPE.URBAN));
                            var buids = bus.Select(x => x.ParentId.Value).ToList();

                            var userOUs = _userOrganizationUnitRepository.GetAllList(x => buids.Contains(x.OrganizationUnitId)).GroupBy(x => x.UserId)
                            .Select(groupedUsers => new
                            {
                                UserId = groupedUsers.Key,
                                Items = groupedUsers.ToList()
                            }).ToDictionary(x => x.UserId, y => y.Items);

                            foreach (var userOU in userOUs)
                            {
                                User user = await GetUserByIdAsync(userOU.Key);
                                var listIds = userOU.Value.Select(x => x.OrganizationUnitId).ToList();
                                await AddClaimAsync(user, new Claim("BUIds", string.Join(",", listIds)));
                            }
                        }
                        catch { }
                    }
                }
            });

        }
        #endregion

        #region Cache OrganizationUnit Department
        public List<long> GetAccessibleDepartmentIds()
        {
            var ids = new List<long>();
            if (AbpSession.TenantId == null) return ids;
            try
            {
                using (_unitOfWorkManager.Current.SetTenantId(AbpSession.TenantId))
                {
                    var departments = _cacheManager.GetDepartmentUserCache().Get(AbpSession.TenantId.Value, f => GetDepartmentUsers());
                    var departmentIds = departments.Where(x => x.UserId ==  AbpSession.UserId).Select(x => x.DeptId).ToList();
                    var organizationUnits = _cacheManager.GetDepaprtmentOrganizationUnitCache().Get(AbpSession.TenantId.Value, f => GetDepartmentOrganizations());
                    var organizationUnitIds = organizationUnits.Where(x => departmentIds.Contains(x.DeptId)).Select(x => x.OrganizationUnitId).ToList();
                    ids = organizationUnitIds;

                }
            }
            catch { }
              
            return ids;
        }

        private List<DepartmentOrganizationUnitCacheItem> GetDepartmentOrganizations()
        {
            using (_unitOfWorkManager.Current.SetTenantId(AbpSession.TenantId))
            {
                var departments = _dtOuRepo.GetAllList();
                return departments.MapTo<List<DepartmentOrganizationUnitCacheItem>>();
            }

        }
        private List<DepartmentUserCacheItem> GetDepartmentUsers()
        {
            using (_unitOfWorkManager.Current.SetTenantId(AbpSession.TenantId))
            {
                var departments = _deptUserRepo.GetAllList();
                return departments.MapTo<List<DepartmentUserCacheItem>>();
            }

        }

        #endregion


        #region ERP
        public async Task<User> GetErpUserOrNullByPhoneNumberAsync(string phoneNumber)
        {
            return await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                var tenant = await _tenantRepository.FirstOrDefaultAsync(x => x.TenancyName == phoneNumber);
                if (tenant == null) return null;

                using (_unitOfWorkManager.Current.SetTenantId(tenant.Id))
                {
                    return await FindByNameAsync(phoneNumber);
                }
            });
        }

        public async Task<IdentityResult> UpdateErpUserAsync(User user)
        {
            return await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (_unitOfWorkManager.Current.SetTenantId(user.TenantId))
                {
                    return await UpdateAsync(user);
                }
            });
        }

        #endregion
    }
}
