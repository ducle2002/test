using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Linq.Extensions;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Yootek.Authorization.Permissions.Dto;
using Yootek.Common.DataResult;
using Abp.Runtime.Validation;
using Microsoft.EntityFrameworkCore;
using Abp.UI;
using Yootek.MultiTenancy;

namespace Yootek.Authorization.Permissions
{
    public class PermissionAppService : YootekAppServiceBase, IApplicationService
    {
        private readonly IPermissionManager _permissionManager;
        private readonly IRepository<PermissionTenant, long> _permissionTenantRepo;
        private readonly IRepository<Tenant, int> _tenantRepository;

        public PermissionAppService(IPermissionManager permissionManager,
            IRepository<PermissionTenant, long> permissionTenantRepository,
            IRepository<Tenant, int> tenantRepository)
        {
            _permissionManager = permissionManager;
            _permissionTenantRepo = permissionTenantRepository;
            _tenantRepository = tenantRepository;
        }

        public async Task<ListResultDto<FlatPermissionWithLevelDto>> GetAllPermissions(GetAllPermissionsDto input)
        {
            using (CurrentUnitOfWork.SetTenantId(input.TenantId))
            {
                var permissions = _permissionManager.GetAllPermissions(tenancyFilter: false);
                var query = permissions.Where(p => p.Parent == null);

                var rootPermissions = query.ToList();

                var flatPermissions = new List<FlatPermissionWithLevelDto>();

                foreach (var rootPermission in rootPermissions)
                {
                    var level = 0;
                    AddPermission(rootPermission, permissions, flatPermissions, level);
                }

                if (input.TenantId.HasValue)
                {
                    var tenantPermissions = await _permissionTenantRepo.GetAll()
                        .Where(p => p.TenantId == input.TenantId.Value).ToListAsync();
                    flatPermissions = flatPermissions.FindAll(flatPermission =>
                        tenantPermissions.Any(tp => tp.Name == flatPermission.Name));
                }

                return new ListResultDto<FlatPermissionWithLevelDto>
                {
                    Items = flatPermissions
                };
            }
        }

        public async Task<object> UpdatePermissionsForTenant(UpdatePermissionsForTenantDto input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(input.TenantId))
                {
                    var qAllPermissions =
                    (await this.GetAllPermissions(new GetAllPermissionsDto { TenantId = null })).Items;
                    var allPermissions = qAllPermissions.ToList();

                    // check valid input
                    foreach (var permission in input.Permissions)
                    {
                        if (!allPermissions.Any(p => p.Name == permission))
                        {
                            throw new UserFriendlyException("Permission not found");
                        }
                    }

                    // clear data before reinsert
                    var curPermissions = await _permissionTenantRepo.GetAll().Where(p => p.TenantId == input.TenantId)
                        .ToListAsync();
                    foreach (var curPermission in curPermissions)
                    {
                        await _permissionTenantRepo.DeleteAsync(curPermission);
                    }

                    foreach (var permission in input.Permissions)
                    {
                        await _permissionTenantRepo.InsertAsync(new PermissionTenant
                        {
                            TenantId = input.TenantId,
                            Name = permission
                        });
                    }

                    var data = DataResult.ResultSuccess(input, "Update success!");
                    return data;
                }
                 
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                var data = DataResult.ResultError(e.Message, "Exception!");
                return data;
            }
        }

        private void AddPermission(Permission permission, IReadOnlyList<Permission> allPermissions,
            List<FlatPermissionWithLevelDto> result, int level)
        {
            using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
            {
                var flatPermission = ObjectMapper.Map<FlatPermissionWithLevelDto>(permission);
                flatPermission.Level = level;
                result.Add(flatPermission);

                if (permission.Children == null)
                {
                    return;
                }

                var children = allPermissions.Where(p => p.Parent != null && p.Parent.Name == permission.Name).ToList();

                foreach (var childPermission in children)
                {
                    AddPermission(childPermission, allPermissions, result, level + 1);
                }
            }
           
        }
    }
}