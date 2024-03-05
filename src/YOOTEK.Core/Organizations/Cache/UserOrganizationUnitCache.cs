using Abp;
using Abp.Domain.Repositories;
using Abp.Runtime.Caching;
using System.Linq;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.MultiTenancy;
using Yootek.Authorization.Users;
using Yootek.Chat;
using Yootek.Friendships;
using Abp.Collections.Extensions;
using Abp.Organizations;
using Abp.Authorization.Users;
using System.Threading.Tasks;
using Yootek.Organizations.Cache.Dto;
using Yootek.EntityDb;
using Yootek.Authorization.Roles;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;
using Yootek.Authorization;
using Abp.Authorization;

namespace Yootek.Organizations.Cache
{
    public class UserOrganizationUnitCache : IUserOrganizationUnitCache, ISingletonDependency
    {
        private readonly ICacheManager _cacheManager;
        private readonly IRepository<OrganizationUnit, long> _organizationUnitRepository;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationUnitRepository;
        private readonly IRepository<UserPermissionSetting, long> _userPermissionSettingRepos;
        private readonly IRepository<OrganizationUnitRole, long> _organizationUnitRoleRepos;
        private readonly ITenantCache _tenantCache;
        private readonly IRepository<User, long> _userRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<Role> _roleRepository;
        private readonly IPermissionManager _permissionManager;
        private readonly IRepository<Friendship, long> _friendshipRepos;
        private readonly object _syncObj = new object();

        public UserOrganizationUnitCache(
            ICacheManager cacheManager,
            IRepository<OrganizationUnit, long> organizationUnitRepository,
            IRepository<UserPermissionSetting, long> userPermissionSettingRepos,
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,
            IRepository<OrganizationUnitRole, long> organizationUnitRoleRepos,
            IRepository<Friendship, long> friendshipRepos,
            IRepository<Role> roleRepository,
            ITenantCache tenantCache,
            IRepository<User, long> userRepository,
            IUnitOfWorkManager unitOfWorkManager,
            IPermissionManager permissionManager)
        {
            _friendshipRepos = friendshipRepos;
            _cacheManager = cacheManager;
            _tenantCache = tenantCache;
            _userRepository = userRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _organizationUnitRepository = organizationUnitRepository;
            _userOrganizationUnitRepository = userOrganizationUnitRepository;
            _userPermissionSettingRepos = userPermissionSettingRepos;
            _roleRepository = roleRepository;
            _permissionManager = permissionManager;
            _organizationUnitRoleRepos = organizationUnitRoleRepos;
        }



        public async Task<List<OrganizationUnitChatCacheItem>> GetOrganizationUnitChatById(long organizationUnitId, int? tenantId = null)
        {
            return _cacheManager.GetCache<string, List<OrganizationUnitChatCacheItem>>(OrganizationUnitChatCacheItem.CacheName + "Id").Get("OrganizationUnit" + organizationUnitId, f => GetOrganizationUnitChatInternal(organizationUnitId, tenantId));
        }

        public async Task<List<FriendChatOrganizationUnitCacheItem>> GetFriendChatOrganizationUnit(long organizationUnitId, int? tenantId = null)
        {
            return await _cacheManager.GetCache<string, List<FriendChatOrganizationUnitCacheItem>>(FriendChatOrganizationUnitCacheItem.CacheName)
                .GetAsync("FriendChatOrganizationUnit" + organizationUnitId, async f => await FriendChatOrganizationUnitInternal(organizationUnitId, tenantId));
        }

        protected List<OrganizationUnitChatCacheItem> GetOrganizationUnitChatInternal(long organizationUnitId, int? tenantId)
        {
            try
            {
                var data = GetOrganizationUnitInternal(organizationUnitId, tenantId);
                if (data == null)
                {
                    return null;
                }
                return _unitOfWorkManager.WithUnitOfWork(() =>
                {
                    using (_unitOfWorkManager.Current.SetTenantId(tenantId))
                    {
                        //var test = _permissionManager.GetAllPermissions();
                        var role = _roleRepository.GetAllIncluding(x => x.Permissions).Where(m => m.Permissions != null && m.Permissions.Where(p => p.Name == IOCPermissionNames.Pages_Digitals_Communications || p.Name == IOCPermissionNames.Pages_Government_ChatCitizen).FirstOrDefault() != null).Select(r => r.Id).ToList();
                        var ouIds = _organizationUnitRoleRepos.GetAllList(x => role.Contains(x.RoleId)).Select(m => m.OrganizationUnitId).ToList();
                        var result = data.Where(x => ouIds.Contains(x.OrganizationUnitId)).ToList();

                        return result;
                    }
                });
            }
            catch (Exception e)
            {
                return null;
            }

        }

        private List<OrganizationUnitChatCacheItem> GetOrganizationUnitInternal(long organizationUnitId, int? tenantId)
        {
            return _unitOfWorkManager.WithUnitOfWork(() =>
            {
                using (_unitOfWorkManager.Current.SetTenantId(tenantId))
                {
                    var result = new List<OrganizationUnitChatCacheItem>();
                    var organizationUnit = _organizationUnitRepository.GetAllIncluding(x => x.Children).FirstOrDefault(m => m.Id == organizationUnitId);

                    if (organizationUnit == null)
                    {
                        return null;
                    }
                    var listOrgs = _organizationUnitRepository.GetAllList(x => x.Code.StartsWith(organizationUnit.Code));
                    foreach (var uo in listOrgs)
                    {
                        var dt = new OrganizationUnitChatCacheItem()
                        {
                            OrganizationUnitId = uo.Id,
                            Name = uo.DisplayName,
                            TenantId = uo.TenantId
                        };
                        result.Add(dt);
                    }
                    return result;
                }

            });

        }

        protected async Task<List<FriendChatOrganizationUnitCacheItem>> FriendChatOrganizationUnitInternal(long organizationUnitId, int? tenantId)
        {
            return await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (_unitOfWorkManager.Current.SetTenantId(tenantId))
                {
                    try
                    {
                        var query =
                   (from friendship in _friendshipRepos.GetAll()
                    where friendship.UserId == organizationUnitId
                    select new FriendChatOrganizationUnitCacheItem
                    {
                        FriendUserId = friendship.FriendUserId,
                        FriendTenantId = friendship.FriendTenantId,
                        State = friendship.State,
                        FollowState = friendship.FollowState,
                        FriendUserName = friendship.FriendUserName,
                        FriendTenancyName = friendship.FriendTenancyName,
                        FriendImageUrl = friendship.FriendImageUrl,
                        IsOrganizationUnit = friendship.IsOrganizationUnit,
                        LastMessageDate = friendship.CreationTime
                    })
                    .Where(x => x.IsOrganizationUnit == true)
                    .AsQueryable();

                        var friendCacheItems = await query.ToListAsync();

                        return friendCacheItems;
                    }
                    catch (Exception e)
                    {
                        return null;
                    }
                }

            });
        }

        public async Task AddOrganizationUnitChatByProject(string projectCode, OrganizationUnitChatCacheItem item, int? tenantId = null)
        {

        }

        public async Task RemoveOrganizationUnitChatByProject(string projectCode, long id, int? tenantId = null)
        {

        }

    }
}