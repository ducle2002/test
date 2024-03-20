using Abp;
using Abp.Authorization.Users;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Organizations;
using Abp.Runtime.Caching;
using Yootek.Authorization.Roles;
using Yootek.Organizations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Abp.Zero.Configuration.AbpZeroSettingNames;

namespace Yootek.Authorization.Users
{
    public class UserStore : AbpUserStore<Role, User>
    {
        private readonly IRepository<Role> _roleRepository;
        private readonly IRepository<User, long> _userRepository;
        private IRepository<UserPermissionSetting, long> _userPermissionSettingRepository;
        private ICacheManager _cacheManager;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationUnitRepository;
        private readonly IRepository<AppOrganizationUnit, long> _organizationUnitRepository;
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
            IRepository<AppOrganizationUnit, long> organizationUnitRepository,
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
            _organizationUnitRepository = organizationUnitRepository;
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
                    var roles = _roleRepository.GetAllIncluding(x => x.Permissions).Where(y => y.Permissions.Any(u => u.Name == IOCPermissionNames.Pages_Citizens)).ToList();
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

        public async Task<List<User>> GetAllChatCitizenManagerTenantAsync(long? urbanId, int? tenantId)
        {
            try
            {
                return await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
                {
                    using (_unitOfWorkManager.Current.SetTenantId(tenantId))
                    {
                        var ids = _organizationUnitRepository.GetAll().Where(x =>x.Type == APP_ORGANIZATION_TYPE.CHAT).Select(uou => uou.ParentId).ToList();
                        var uoIds = _organizationUnitRepository.GetAll().Where(x => ids.Contains(x.Id)).WhereIf(urbanId.HasValue, x => x.ParentId == urbanId).Select(x => x.Id).ToList();
                        var userIdsInOrganizationUnit = _userOrganizationUnitRepository.GetAll().Where(uou => uoIds.Contains(uou.OrganizationUnitId)).Select(uou => uou.UserId);
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
      
    }
}

