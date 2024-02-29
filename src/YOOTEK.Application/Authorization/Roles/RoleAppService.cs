using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.IdentityFramework;
using Abp.Linq.Extensions;
using Yootek.Authorization;
using Yootek.Authorization.Roles;
using Yootek.Authorization.Users;
using Yootek.Roles.Dto;
using Yootek.Authorization.Permissions;
using Yootek.Authorization.Permissions.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Yootek.Roles
{
    // [AbpAuthorize(PermissionNames.Pages_Roles, PermissionNames.Pages_Admin, PermissionNames.Pages_SystemAdministration_Roles)]
    public class RoleAppService :
        AsyncCrudAppService<Role, RoleDto, int, PagedRoleResultRequestDto, CreateRoleDto, RoleDto>, IRoleAppService
    {
        private readonly RoleManager _roleManager;
        private readonly UserManager _userManager;
        private readonly UserRegistrationManager _userRegistrationManager;
        private readonly PermissionAppService _permissionAppService;
        private readonly IRepository<Role> _repository;

        public RoleAppService(
            IRepository<Role> repository,
            UserRegistrationManager userRegistrationManager,
            RoleManager roleManager,
            UserManager userManager,
            PermissionAppService permissionAppService)
            : base(repository)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _userRegistrationManager = userRegistrationManager;
            _repository = repository;
            _permissionAppService = permissionAppService;
        }

        public override async Task<RoleDto> CreateAsync(CreateRoleDto input)
        {
            CheckCreatePermission();
            var role = ObjectMapper.Map<Role>(input);

            //Create Account rocket chat
            string thirdAcc = null;
            //if (input.GrantedPermissions.Contains(PermissionNames.Pages_SmartCommunity_Citizens_ChatCitizen))
            //{

            //    var passRC = _userRegistrationManager.GeneratePassThirdAccount();
            //    var usernameRC = input.Name;
            //    usernameRC = usernameRC + AbpSession.TenantId != null ? AbpSession.TenantId.ToString() : "" + _userRegistrationManager.GenerateString();
            //    var mailRC = usernameRC + "@gmail.com";
            //    var check = await _userRegistrationManager.RegisterRocketChatAsync(input.DisplayName, mailRC, usernameRC, passRC);
            //    if (check)
            //    {
            //        thirdAcc = System.Text.Json.JsonSerializer.Serialize(new
            //        {
            //            rocketChat = new
            //            {
            //                username = usernameRC,
            //                password = passRC,
            //                email = mailRC
            //            }
            //        });
            //    }
            //    role.IsChatActive = true;
            //}
            //else
            //{
            //    role.IsChatActive = false;
            //}
            //

            role.ThirdAccounts = thirdAcc;
            role.SetNormalizedName();
            CheckErrors(await _roleManager.CreateAsync(role));

            var grantedPermissions = PermissionManager
                .GetAllPermissions()
                .Where(p => input.GrantedPermissions.Contains(p.Name))
                .ToList();

            await _roleManager.SetGrantedPermissionsAsync(role, grantedPermissions);
            return MapToEntityDto(role);
        }

        public async Task<ListResultDto<RoleListDto>> GetRolesAsync(GetRolesInput input)
        {
            var roles = await _roleManager
                .Roles
                .WhereIf(
                    !input.Permission.IsNullOrWhiteSpace(),
                    r => r.Permissions.Any(rp => rp.Name == input.Permission && rp.IsGranted)
                )
                .ToListAsync();

            return new ListResultDto<RoleListDto>(ObjectMapper.Map<List<RoleListDto>>(roles));
        }

        public override async Task<RoleDto> UpdateAsync(RoleDto input)
        {
            CheckUpdatePermission();

            var role = await _roleManager.GetRoleByIdAsync(input.Id);

            ObjectMapper.Map(input, role);
            if (input.GrantedPermissions.Contains(IOCPermissionNames.Pages_Digitals_Communications) ||
                input.GrantedPermissions.Contains(IOCPermissionNames.Pages_Government_ChatCitizen))
            {
                role.IsChatActive = true;
            }
            else
            {
                role.IsChatActive = false;
            }

            CheckErrors(await _roleManager.UpdateAsync(role));

            var grantedPermissions = PermissionManager
                .GetAllPermissions()
                .Where(p => input.GrantedPermissions.Contains(p.Name))
                .ToList();

            await _roleManager.SetGrantedPermissionsAsync(role, grantedPermissions);

            return MapToEntityDto(role);
        }

        public override async Task DeleteAsync(EntityDto<int> input)
        {
            CheckDeletePermission();

            var role = await _roleManager.FindByIdAsync(input.Id.ToString());
            var users = await _userManager.GetUsersInRoleAsync(role.NormalizedName);

            foreach (var user in users)
            {
                CheckErrors(await _userManager.RemoveFromRoleAsync(user, role.NormalizedName));
            }

            CheckErrors(await _roleManager.DeleteAsync(role));
        }

        public async Task<ListResultDto<FlatPermissionDto>> GetAllPermissions()
        {
            var qPermissions =
                await _permissionAppService.GetAllPermissions(new GetAllPermissionsDto(AbpSession.TenantId));
            var permissions = qPermissions.Items;

            return new ListResultDto<FlatPermissionDto>(
                ObjectMapper.Map<List<FlatPermissionDto>>(permissions).OrderBy(p => p.Name).ToList()
            );
        }

        [AbpAllowAnonymous]
        public async Task<List<RoleDto>> GetAllRoleChatUserAsync()
        {
            try
            {
                var roles = await Repository
                    .GetAllIncluding(x => x.Users)
                    .Where(y => y.IsChatActive == true && y.Users.Any(m => m.UserId == AbpSession.UserId))
                    .ToListAsync();
                return ObjectMapper.Map<List<RoleDto>>(roles);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        protected override IQueryable<Role> CreateFilteredQuery(PagedRoleResultRequestDto input)
        {
            return Repository.GetAllIncluding(x => x.Permissions)
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Keyword)
                                                                   || x.DisplayName.Contains(input.Keyword)
                                                                   || x.Description.Contains(input.Keyword));
        }

        protected override async Task<Role> GetEntityByIdAsync(int id)
        {
            return await Repository.GetAllIncluding(x => x.Permissions).FirstOrDefaultAsync(x => x.Id == id);
        }

        protected override IQueryable<Role> ApplySorting(IQueryable<Role> query, PagedRoleResultRequestDto input)
        {
            return query.OrderBy(r => r.DisplayName);
        }

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }

        public async Task<GetRoleForEditOutput> GetRoleForEdit(EntityDto input)
        {
            var qPermissions =
                await _permissionAppService.GetAllPermissions(new GetAllPermissionsDto(AbpSession.TenantId));
            var permissions = qPermissions.Items;

            var role = await _roleManager.GetRoleByIdAsync(input.Id);
            var grantedPermissions = (await _roleManager.GetGrantedPermissionsAsync(role)).ToArray();
            var roleEditDto = ObjectMapper.Map<RoleEditDto>(role);

            return new GetRoleForEditOutput
            {
                Role = roleEditDto,
                Permissions = ObjectMapper.Map<List<FlatPermissionDto>>(permissions).OrderBy(p => p.DisplayName)
                    .ToList(),
                GrantedPermissionNames = grantedPermissions.Select(p => p.Name).ToList()
                    .FindAll(x => permissions.Any(y => y.Name == x))
            };
        }
    }
}