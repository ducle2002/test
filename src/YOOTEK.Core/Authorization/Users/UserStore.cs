using Abp;
using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Organizations;
using Abp.Runtime.Caching;
using IMAX.Authorization.Roles;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Abp.Zero.Configuration.AbpZeroSettingNames;

namespace IMAX.Authorization.Users
{
    public class UserStore : AbpUserStore<Role, User>
    {
        private readonly IRepository<Role> _roleRepository;
        private readonly IRepository<User, long> _userRepository;
        private IRepository<UserPermissionSetting, long> _userPermissionSettingRepository;
        private ICacheManager _cacheManager;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationUnitRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        public UserStore(
            IUnitOfWorkManager unitOfWorkManager,
            ICacheManager cacheManager,
            IRepository<User, long> userRepository,
            IRepository<Role> roleRepository,
            IRepository<UserRole, long> userRoleRepository,
            IRepository<UserLogin, long> userLoginRepository,
            IRepository<UserClaim, long> userClaimRepository,
            IRepository<UserPermissionSetting, long> userPermissionSettingRepository,
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,
            IRepository<OrganizationUnitRole, long> organizationUnitRoleRepository,
            IRepository<UserToken, long> userTokenRepository)
            : base(unitOfWorkManager,
                  userRepository,
                  roleRepository,
                  userRoleRepository,
                  userLoginRepository,
                  userClaimRepository,
                  userPermissionSettingRepository,
                  userOrganizationUnitRepository,
                  organizationUnitRoleRepository,
                  userTokenRepository
                  )
        {
            _roleRepository = roleRepository;
            _userRepository = userRepository;
            _userPermissionSettingRepository = userPermissionSettingRepository;
            _userOrganizationUnitRepository = userOrganizationUnitRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _cacheManager = cacheManager;
        }

        public async Task<List<User>> GetAllUserTenantAsync()
        {
            var users = await UserRepository.GetAllListAsync();
            return users;
        }

        public async Task<List<UserIdentifier>> GetAllUserIdentifierTenantAsync()
        {
            var users = await (from us in _userRepository.GetAll()
                               select new UserIdentifier(us.TenantId, us.Id)).ToListAsync();
            return users;
        }

        public async Task<UserIdentifier> GetUserIdentifierTenantAsync(long UserId)
        {
            var user = await _userRepository.FirstOrDefaultAsync(x => x.Id == UserId);
            if (user != null)
            {
                var userIdentifier = new UserIdentifier(user.TenantId, user.Id);
                return userIdentifier;
            }
            else return null;

        }

        public async Task<List<UserIdentifier>> GetAllCitizenManagerTenantAsync(int? tenantId)
        {
            return await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (_unitOfWorkManager.Current.SetTenantId(tenantId))
                {
                    var users = new List<UserIdentifier>();
                    var roles = _roleRepository.GetAllIncluding(x => x.Permissions).Where(y => y.Permissions.Any(u => u.Name == PermissionNames.Pages_Citizens)).ToList();
                    if (roles != null)
                    {
                        foreach (var role in roles)
                        {
                            var users1 = await UserRepository.GetAllIncluding(x => x.Roles).Where(y => y.Roles.Any(m => m.RoleId == role.Id))
                            .Select(x => new UserIdentifier(x.TenantId, x.Id)).ToListAsync();
                            users = users.Concat(users1).Distinct().ToList();
                        }
                    }
                    return users;

                }
            });
        
        }

        public async Task<List<User>> GetAllChatCitizenManagerTenantAsync()
        {
            var userPermiss = await (from x in _userPermissionSettingRepository.GetAll()
                                     where x.Name == PermissionNames.Pages_Digitals_Communications || x.Name == PermissionNames.Pages_Government_ChatCitizen
                                     select x.UserId).ToListAsync();
            // var roleCitizenManager = await _roleRepository.FirstOrDefaultAsync(x => x.Name == StaticRoleNames.Tenants.CitizenManager);
            if (userPermiss != null)
            {
                var users = await UserRepository.GetAllListAsync(x => userPermiss.Contains(x.Id));
                return users;
            }
            else
            {
                return null;
            }

        }

        public async Task<List<User>> GetUserByOrganizationUnitIdAsync(long organizationUnitId, int? tenantId)
        {
            try
            {
                return await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
                {
                    using (_unitOfWorkManager.Current.SetTenantId(tenantId))
                    {
                        var userIdsInOrganizationUnit = _userOrganizationUnitRepository.GetAll().Where(uou => uou.OrganizationUnitId == organizationUnitId).Select(uou => uou.UserId);
                        var query = _userRepository.GetAll()
                            .Where(u => userIdsInOrganizationUnit.Contains(u.Id));
                        return await query.ToListAsync();
                    }
                });

            }
            catch (Exception e)
            {
                return null;
            }

        }

        public async Task<List<User>> GetAllRoleChatFeedbackHBAsync(int type)
        {
            var permissionChat = PermissionConst.HongBangRoleMap[type];
            if (permissionChat != null)
            {
                var users = new List<User>();
                var roles = _roleRepository.GetAllIncluding(x => x.Permissions).Where(y => y.Permissions.Any(u => u.Name == permissionChat)).ToList();
                if (roles != null)
                {
                    foreach (var role in roles)
                    {
                        var users1 = await UserRepository.GetAllIncluding(x => x.Roles).Where(y => y.Roles.Any(m => m.RoleId == role.Id)).ToListAsync();
                        users = users.Concat(users1).Distinct().ToList();
                    }
                }
                return users;
            }
            else
            {
                return null;
            }

        }

        public async Task<Dictionary<int, int>> GetAllTypeRoleHBAsync()
        {
            var types = new Dictionary<int, int>();
            for (var i = 1; i < 14; i++)
            {
                types.Add(i, 999);
            }

            //var permissionChat = PermissionConst.HongBangRoleMap[type];
            var roles = await _roleRepository.GetAllIncluding(x => x.Permissions, y => y.Users).Where(m => m.Users.Any(m => m.UserId == AbpSession.UserId)).ToListAsync();

            if (roles != null)
            {
                foreach (var permission in PermissionConst.HongBangRoleMap)
                {
                    //var per = role.
                    foreach (var role in roles)
                    {
                        var check = role.Permissions.Where(x => x.Name == permission.Value).ToList();
                        if (check.Count() > 0)
                        {
                            types[permission.Key] = permission.Key;
                        }
                    }
                }
            }
            return types;
        }
    }
}

