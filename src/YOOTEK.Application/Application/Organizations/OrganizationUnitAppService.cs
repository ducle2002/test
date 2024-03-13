using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.IdentityFramework;
using Abp.Linq.Extensions;
using Abp.Organizations;
using Abp.UI;
using Yootek.Authorization.Roles;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Organizations;
using Yootek.Organizations.Dto;
using Yootek.Organizations.OrganizationStructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Yootek.Authorization;
using Yootek.QueriesExtension;

namespace Yootek.Service
{
    public interface IOrganizationUnitAppService : IApplicationService
    {
        Task<ListResultDto<OrganizationUnitDto>> GetOrganizationUnits();

        Task<PagedResultDto<OrganizationUnitUserListDto>> GetOrganizationUnitUsers(GetOrganizationUnitUsersInput input);

        Task<OrganizationUnitDto> CreateOrganizationUnit(CreateOrganizationUnitInput input);

        Task<OrganizationUnitDto> UpdateOrganizationUnit(UpdateOrganizationUnitInput input);

        Task<OrganizationUnitDto> MoveOrganizationUnit(MoveOrganizationUnitInput input);

        Task DeleteOrganizationUnit(EntityDto<long> input);

        Task RemoveUserFromOrganizationUnit(UserToOrganizationUnitInput input);

        Task RemoveRoleFromOrganizationUnit(RoleToOrganizationUnitInput input);

        Task AddUsersToOrganizationUnit(UsersToOrganizationUnitInput input);

        Task AddRolesToOrganizationUnit(RolesToOrganizationUnitInput input);

        Task<PagedResultDto<NameValueDto>> FindUsers(FindOrganizationUnitUsersInput input);

        Task<PagedResultDto<NameValueDto>> FindRoles(FindOrganizationUnitRolesInput input);

    }

    [AbpAuthorize]
    public class OrganizationUnitAppService : YootekAppServiceBase, IOrganizationUnitAppService
    {
        private readonly AppOrganizationUnitManager _organizationUnitManager;
        private readonly IRepository<AppOrganizationUnit, long> _organizationUnitRepository;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationUnitRepository;
        private readonly IRepository<OrganizationUnitRole, long> _organizationUnitRoleRepository;
        private readonly RoleManager _roleManager;
        private readonly UserManager _userManager;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<Position, long> _positionRepository;
        private readonly IRepository<Staff, long> _staffRepository;
        private readonly IRepository<DepartmentOrganizationUnit, long> _departUnitRepository;


        public OrganizationUnitAppService(
            AppOrganizationUnitManager organizationUnitManager,
            IRepository<AppOrganizationUnit, long> organizationUnitRepository,
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,
            RoleManager roleManager,
            UserManager userManager,
            IRepository<OrganizationUnitRole, long> organizationUnitRoleRepository,
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<Position, long> positionRepository,
            IRepository<Staff, long> staffRepository,
            IRepository<DepartmentOrganizationUnit, long> departUnitRepository
            )
        {
            _organizationUnitRepository = organizationUnitRepository;
            _organizationUnitManager = organizationUnitManager;
            _userOrganizationUnitRepository = userOrganizationUnitRepository;
            _roleManager = roleManager;
            _organizationUnitRoleRepository = organizationUnitRoleRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _positionRepository = positionRepository;
            _staffRepository = staffRepository;
            _departUnitRepository = departUnitRepository;
            _userManager = userManager;


        }

        public async Task<ListResultDto<OrganizationUnitDto>> GetOrganizationUnits()
        {
            try
            {
               // List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
                var organizationUnits = await _organizationUnitRepository.GetAll()
                    .Where(x => x.Type == APP_ORGANIZATION_TYPE.REPRESENTATIVE_NAME)
                    .ToListAsync();

                var organizationUnitMemberCounts = await _userOrganizationUnitRepository.GetAll()
                   .GroupBy(x => x.OrganizationUnitId)
                   .Select(groupedUsers => new
                   {
                       organizationUnitId = groupedUsers.Key,
                       count = groupedUsers.Count()
                   }).ToDictionaryAsync(x => x.organizationUnitId, y => y.count);
                var ouDepartmentCounts = await _departUnitRepository.GetAll().GroupBy(x => x.OrganizationUnitId).Select(groupedDepartment => new
                {
                    organizationUnitId = groupedDepartment.Key,
                    count = groupedDepartment.Count()

                }).ToDictionaryAsync(x => x.organizationUnitId, y => y.count);

                var ouChargeCounts = await _organizationUnitRepository.GetAll().Where(ou => ou.ParentId != null && ou.Type != APP_ORGANIZATION_TYPE.REPRESENTATIVE_NAME && ou.Type != 0 && ou.Type != APP_ORGANIZATION_TYPE.BUILDING && ou.Type != APP_ORGANIZATION_TYPE.URBAN && ou.Type != APP_ORGANIZATION_TYPE.NONE)
                    .GroupBy(x => x.ParentId)
                    .Select(groupedCharge => new
                    {
                        organizationUnitId = groupedCharge.Key,
                        count = groupedCharge.Count()
                    }).ToDictionaryAsync(x => x.organizationUnitId, y => y.count);

                var organizationUnitRoleCounts = await _organizationUnitRoleRepository.GetAll()
                    .GroupBy(x => x.OrganizationUnitId)
                    .Select(groupedRoles => new
                    {
                        organizationUnitId = groupedRoles.Key,
                        count = groupedRoles.Count()
                    }).ToDictionaryAsync(x => x.organizationUnitId, y => y.count);

                var organizationUnitTypes = _organizationUnitRepository.GetAllList(x => x.ParentId != null && x.Type != APP_ORGANIZATION_TYPE.REPRESENTATIVE_NAME)
                   .GroupBy(x => x.ParentId)
                   .Select(ou => new
                   {
                       parentId = ou.Key,
                       types = ou.Select(x => x.Type).Distinct().ToArray()
                   }).ToDictionary(x => x.parentId, y => y.types);

                return new ListResultDto<OrganizationUnitDto>(
                                   organizationUnits.Select(ou =>
                                   {
                                       var organizationUnitDto = ObjectMapper.Map<OrganizationUnitDto>(ou);
                                       organizationUnitDto.MemberCount = organizationUnitMemberCounts.ContainsKey(ou.Id) ? organizationUnitMemberCounts[ou.Id] : 0;
                                       organizationUnitDto.RoleCount = organizationUnitRoleCounts.ContainsKey(ou.Id) ? organizationUnitRoleCounts[ou.Id] : 0;
                                       organizationUnitDto.DepartmentCount = ouDepartmentCounts.ContainsKey(ou.Id) ? ouDepartmentCounts[ou.Id] : 0;
                                       organizationUnitDto.UnitChargeCount = ouChargeCounts.ContainsKey(ou.Id) ? ouChargeCounts[ou.Id] : 0;
                                       organizationUnitDto.Types = organizationUnitTypes.ContainsKey(ou.Id) ? organizationUnitTypes[ou.Id] : null;
                                       return organizationUnitDto;
                                   }).ToList());
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<PagedResultDto<OrganizationUnitUserListDto>> GetOrganizationUnitUsers(GetOrganizationUnitUsersInput input)
        {
            try
            {
                var query = from ouUser in _userOrganizationUnitRepository.GetAll()
                            join ou in _organizationUnitRepository.GetAll() on ouUser.OrganizationUnitId equals ou.Id
                            join user in UserManager.Users on ouUser.UserId equals user.Id
                            join staff in _staffRepository.GetAll() on user.Id equals staff.UserId into tb_staff
                            from staff in tb_staff.DefaultIfEmpty()
                            join position in _positionRepository.GetAll() on staff.PositionId equals position.Id into tb_position
                            from position in tb_position.DefaultIfEmpty()
                            where ouUser.OrganizationUnitId == input.Id
                            select new
                            {
                                ouUser,
                                user,
                                position
                            };

                var totalCount = await query.CountAsync();
                var items = await query.OrderBy(input.Sorting).PageBy(input).ToListAsync();

                return new PagedResultDto<OrganizationUnitUserListDto>(
                   totalCount,
                   items.Select(item =>
                   {
                       var organizationUnitUserDto = ObjectMapper.Map<OrganizationUnitUserListDto>(item.user);
                       organizationUnitUserDto.AddedTime = item.ouUser.CreationTime;
                       organizationUnitUserDto.PositionName = item.position?.DisplayName; // gán giá trị null nếu position không tồn tại
                       return organizationUnitUserDto;
                   }).ToList());
            }
            catch (Exception e)
            {
                throw;
            }
        }

        //public async Task<PagedResultDto<OrganizationUnitUserListDto>> GetOrganizationUnitUsers(GetOrganizationUnitUsersInput input)
        //{
        //    try
        //    {
        //        var query = from ouUser in _userOrganizationUnitRepository.GetAll()
        //                    join ou in _organizationUnitRepository.GetAll() on ouUser.OrganizationUnitId equals ou.Id
        //                    join staff in _staffRepository.GetAll() on ouUser.UserId equals staff.UserId
        //                    join position in _positionRepository.GetAll() on staff.PositionId equals position.Id
        //                    join user in UserManager.Users on ouUser.UserId equals user.Id
        //                    where ouUser.OrganizationUnitId == input.Id
        //                    select new
        //                    {
        //                        ouUser,
        //                        user,
        //                        staff,
        //                        position
        //                    };

        //        var totalCount = await query.CountAsync();
        //        var items = await query.OrderBy(input.Sorting).PageBy(input).ToListAsync();

        //        return new PagedResultDto<OrganizationUnitUserListDto>(
        //            totalCount,
        //            items.Select(item =>
        //            {
        //                var organizationUnitUserDto = ObjectMapper.Map<OrganizationUnitUserListDto>(item.user);
        //                organizationUnitUserDto.AddedTime = item.ouUser.CreationTime;
        //                organizationUnitUserDto.PositionName = item.position?.DisplayName;
        //                return organizationUnitUserDto;
        //            }).ToList());
        //    }
        //    catch (Exception e)
        //    {
        //        throw;
        //    }
        //}

        public async Task<PagedResultDto<object>> GetOrganizationUnitIdByUser()
        {
            var query = from ouUser in _userOrganizationUnitRepository.GetAll()
                        join urb in _organizationUnitRepository.GetAll() on ouUser.OrganizationUnitId equals urb.Id
                        where ouUser.UserId == AbpSession.UserId
                        select new
                        {
                            OrganizationUnitId = ouUser.OrganizationUnitId,
                            Type = urb.Type,
                            TenantCode = urb.ProjectCode,
                            DisplayName = urb.DisplayName,
                            ImageUrl = urb.ImageUrl,
                            IsManager = urb.IsManager,
                            Types = (from ou in _organizationUnitRepository.GetAll()
                                     where ou.ParentId == ouUser.OrganizationUnitId
                                     select ou.Type).ToArray(),
                            ParentId = urb.ParentId,
                        };

            var totalCount = await query.CountAsync();
            var items = await query.ToListAsync();
            return new PagedResultDto<object>(
                totalCount,
                items.ToList());
        }

        public async Task<PagedResultDto<OrganizationUnitRoleListDto>> GetOrganizationUnitRoles(GetOrganizationUnitRolesInput input)
        {
            var query = from ouRole in _organizationUnitRoleRepository.GetAll()
                        join ou in _organizationUnitRepository.GetAll() on ouRole.OrganizationUnitId equals ou.Id
                        join role in _roleManager.Roles on ouRole.RoleId equals role.Id
                        where ouRole.OrganizationUnitId == input.Id
                        select new
                        {
                            ouRole,
                            role
                        };

            var totalCount = await query.CountAsync();
            var items = await query.OrderBy(input.Sorting).PageBy(input).ToListAsync();

            return new PagedResultDto<OrganizationUnitRoleListDto>(
                totalCount,
                items.Select(item =>
                {
                    var organizationUnitRoleDto = ObjectMapper.Map<OrganizationUnitRoleListDto>(item.role);
                    organizationUnitRoleDto.AddedTime = item.ouRole.CreationTime;
                    return organizationUnitRoleDto;
                }).ToList());
        }

        public async Task<OrganizationUnitDto> CreateOrganizationUnit(CreateOrganizationUnitInput input)
        {
            try
            {
                var organizationUnitName = new AppOrganizationUnit(AbpSession.TenantId, input.DisplayName, input.ParentId, APP_ORGANIZATION_TYPE.REPRESENTATIVE_NAME, input.IsManager);
                organizationUnitName.ImageUrl = input.ImageUrl;
                organizationUnitName.Description = input.Description;
                organizationUnitName.ProjectCode = input.ProjectCode;
                organizationUnitName.Code = await _organizationUnitManager.GetNextChildCodeAsync(input.ParentId);
                var id = await _organizationUnitRepository.InsertAndGetIdAsync(organizationUnitName);
                if (input.Types != null && input.Types.Length > 0)
                {
                    foreach (var type in input.Types)
                    {
                        var organizationUnit = new AppOrganizationUnit(AbpSession.TenantId, input.DisplayName, id, type, input.IsManager);
                        organizationUnit.ImageUrl = input.ImageUrl;
                        organizationUnit.ProjectCode = input.ProjectCode;
                        await _organizationUnitManager.CreateAsync(organizationUnit);
                    }
                }
                else
                {
                    var organizationUnit = new AppOrganizationUnit(AbpSession.TenantId, input.DisplayName, id, APP_ORGANIZATION_TYPE.NONE, input.IsManager);
                    organizationUnit.ImageUrl = input.ImageUrl;
                    organizationUnit.ProjectCode = input.ProjectCode;
                    await _organizationUnitManager.CreateAsync(organizationUnit);
                }
                await CurrentUnitOfWork.SaveChangesAsync();
                var result = ObjectMapper.Map<OrganizationUnitDto>(input);
                result.Id = id;
                return result;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<OrganizationUnitDto> UpdateOrganizationUnit(UpdateOrganizationUnitInput input)
        {
            var organizationUnit = await _organizationUnitRepository.GetAsync(input.Id);
            organizationUnit.DisplayName = input.DisplayName;
            organizationUnit.Description = input.Description;
            organizationUnit.ImageUrl = input.ImageUrl;
            organizationUnit.IsManager = input.IsManager;
            organizationUnit.ProjectCode = input.ProjectCode;
            await _organizationUnitManager.UpdateAsync(organizationUnit);
            if (input.Types == null && input.Types.Length == 0)
            {
                await _organizationUnitRepository.DeleteAsync(x => x.ParentId == input.Id && x.Type != APP_ORGANIZATION_TYPE.REPRESENTATIVE_NAME);

            }

            if (input.Types != null)
            {
                var childs = _organizationUnitRepository.GetAll().Where(x => x.ParentId == input.Id && input.Types.Contains(x.Type)).ToList();
                var adds = new List<APP_ORGANIZATION_TYPE>();
                var deletes = new List<AppOrganizationUnit>();
                // var tuples = CompareArrays(input.Types, types);
                if (childs != null && childs.Count() > 0)
                {
                    var childTypes = childs.Select(x => x.Type).ToArray();
                    adds = input.Types.Where(x => !childTypes.Contains(x)).ToList();
                    deletes = childs.Where(x => !input.Types.Contains(x.Type)).ToList();
                    foreach (var child in childs)
                    {
                        child.DisplayName = input.DisplayName;
                        child.Description = input.Description;
                        child.ImageUrl = input.ImageUrl;
                        child.IsManager = input.IsManager;
                        child.ProjectCode = input.ProjectCode;
                        await _organizationUnitManager.UpdateAsync(child);

                    }
                }
                else
                {
                    adds = input.Types.ToList();
                }

                foreach (var add in adds)
                {
                    var child = new AppOrganizationUnit(AbpSession.TenantId, input.DisplayName, organizationUnit.Id, add, input.IsManager);
                    child.ImageUrl = input.ImageUrl;
                    child.ProjectCode = input.ProjectCode;
                    await _organizationUnitManager.CreateAsync(child);
                }

                foreach (var delete in deletes)
                {
                    await _organizationUnitRepository.DeleteAsync(delete.Id);

                }
                await CurrentUnitOfWork.SaveChangesAsync();

            }
            return await CreateOrganizationUnitDto(organizationUnit);
        }


        public async Task<OrganizationUnitDto> MoveOrganizationUnit(MoveOrganizationUnitInput input)
        {
            await _organizationUnitManager.MoveAsync(input.Id, input.NewParentId);

            return await CreateOrganizationUnitDto(
                await _organizationUnitRepository.GetAsync(input.Id)
                );
        }


        public async Task DeleteFeature(EntityDto<long> input)
        {
            await _organizationUnitRepository.DeleteAsync(input.Id);
        }

        public async Task DeleteOrganizationUnit(EntityDto<long> input)
        {
            await _userOrganizationUnitRepository.DeleteAsync(x => x.OrganizationUnitId == input.Id);
            await _organizationUnitRoleRepository.DeleteAsync(x => x.OrganizationUnitId == input.Id);
            await _organizationUnitManager.DeleteAsync(input.Id);
        }




        public async Task RemoveUserFromOrganizationUnit(UserToOrganizationUnitInput input)
        {
            await UserManager.RemoveFromOrganizationUnitAsync(input.UserId, input.OrganizationUnitId);
        }


        public async Task RemoveRoleFromOrganizationUnit(RoleToOrganizationUnitInput input)
        {
            await _roleManager.RemoveFromOrganizationUnitAsync(input.RoleId, input.OrganizationUnitId);
            var organizationUnit = _organizationUnitRepository.FirstOrDefault(x => x.Id == input.OrganizationUnitId);
            var role = _roleManager.Roles.FirstOrDefault(x => x.Id == input.RoleId);

            var users = await _userManager.GetUsersInOrganizationUnitAsync(organizationUnit, true);
            if (users.Count > 0)
            {
                foreach (var us in users)
                {
                    await _userManager.RemoveFromRoleAsync(us, role.NormalizedName);
                }
            }

        }

        public async Task AddUsersToOrganizationUnit(UsersToOrganizationUnitInput input)
        {
            var organizationUnit = _organizationUnitRepository.FirstOrDefault(x => x.Id == input.OrganizationUnitId);

            var roles = await _roleManager.GetRolesInOrganizationUnit(organizationUnit, true);

            var roleNames = roles
                .SelectMany(role => _roleManager.Roles.Where(r => r.Id == role.Id))
                .Select(role => role.NormalizedName)
                .ToArray();

            foreach (var userId in input.UserIds)
            {
                await UserManager.AddToOrganizationUnitAsync(userId, input.OrganizationUnitId);
                var user = UserManager.GetUserById(userId);
                await _userManager.AddToRolesAsync(user, roleNames);
            }
        }

        public async Task AddRolesToOrganizationUnit(RolesToOrganizationUnitInput input)
        {
            var users = (
                from ouUser in _userOrganizationUnitRepository.GetAll()
                join ou in _organizationUnitRepository.GetAll() on ouUser.OrganizationUnitId equals ou.Id
                join user in UserManager.Users on ouUser.UserId equals user.Id
                join staff in _staffRepository.GetAll() on user.Id equals staff.UserId into tb_staff
                from staff in tb_staff.DefaultIfEmpty()
                where ouUser.OrganizationUnitId == input.OrganizationUnitId
                select new
                {
                    user
                }
            ).ToList();

            var roleIds = input.RoleIds;
            var roleNames = _roleManager.Roles
                .Where(role => roleIds.Contains(role.Id))
                .Select(role => role.NormalizedName)
                .ToArray();

            if (users != null)
            {
                foreach (var us in users)
                {
                    var user = us.user;
                    await _userManager.AddToRolesAsync(user, roleNames);
                }
            }
            foreach (var roleId in input.RoleIds)
            {
                await _roleManager.AddToOrganizationUnitAsync(roleId, input.OrganizationUnitId, AbpSession.TenantId);
            }
        }

        public async Task<PagedResultDto<NameValueDto>> FindUsers(FindOrganizationUnitUsersInput input)
        {
            try
            {
                var userIdsInOrganizationUnit = _userOrganizationUnitRepository.GetAll()
              .Where(uou => uou.OrganizationUnitId == input.OrganizationUnitId)
              .Select(uou => uou.UserId);

                var query = UserManager.Users
                    .Where(u => !userIdsInOrganizationUnit.Contains(u.Id))
                    .WhereIf(
                        !input.Filter.IsNullOrWhiteSpace(),
                        u =>
                            (!string.IsNullOrEmpty(u.Name) && u.Name.Contains(input.Filter)) ||
                            (!string.IsNullOrEmpty(u.Surname) && u.Surname.Contains(input.Filter)) ||
                          (!string.IsNullOrEmpty(u.UserName) && u.UserName.Contains(input.Filter)) ||
                            (!string.IsNullOrEmpty(u.EmailAddress) && !string.IsNullOrWhiteSpace(u.EmailAddress) && u.EmailAddress.Contains(input.Filter))
                    );

                var userCount = await query.CountAsync();
                var users = await query
                    .OrderBy(u => u.UserName)
                    .ThenBy(u => u.UserName)
                    .PageBy(input)
                    .ToListAsync();

                return new PagedResultDto<NameValueDto>(
                    userCount,
                    users.Select(u =>
                        new NameValueDto(
                            u.FullName + " (" + u.EmailAddress + ")",
                            u.Id.ToString()
                        )
                    ).ToList()
                );
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        //public async Task<PagedResultDto<NameValueDto>> FindStaffForOunit(FindOrganizationUnitUsersInput input)
        //{
        //    try
        //    {
        //        var staffIdsInOrganizationUnit = _userOrganizationUnitRepository.GetAll()
        //      .Where(uou => uou.OrganizationUnitId == input.OrganizationUnitId)
        //      .Select(uou => uou.UserId);

        //        var query = UserManager.Users
        //            .Where(u => !staffIdsInOrganizationUnit.Contains(u.))
        //            .WhereIf(
        //                !input.Filter.IsNullOrWhiteSpace(),
        //                u =>
        //                    (!string.IsNullOrEmpty(u.Name) && u.Name.Contains(input.Filter)) ||
        //                    (!string.IsNullOrEmpty(u.Surname) && u.Surname.Contains(input.Filter)) ||
        //                  (!string.IsNullOrEmpty(u.UserName) && u.UserName.Contains(input.Filter)) ||
        //                    (!string.IsNullOrEmpty(u.EmailAddress) && !string.IsNullOrWhiteSpace(u.EmailAddress) && u.EmailAddress.Contains(input.Filter))
        //            );

        //        var userCount = await query.CountAsync();
        //        var users = await query
        //            .OrderBy(u => u.UserName)
        //            .ThenBy(u => u.UserName)
        //            .PageBy(input)
        //            .ToListAsync();

        //        return new PagedResultDto<NameValueDto>(
        //            userCount,
        //            users.Select(u =>
        //                new NameValueDto(
        //                    u.FullName + " (" + u.EmailAddress + ")",
        //                    u.Id.ToString()
        //                )
        //            ).ToList()
        //        );
        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }
        //}

        //public async Task<PagedResultDto<NameValueDto>> FindUsers(FindOrganizationUnitUsersInput input)
        //{
        //    try
        //    {
        //        var userIdsInOrganizationUnit = _userOrganizationUnitRepository.GetAll()
        //            .Where(uou => uou.OrganizationUnitId == input.OrganizationUnitId)
        //            .Select(uou => uou.UserId);

        //        var staffUserIds = _staffRepository.GetAll()
        //            .Where(st => st.OrganizationUnitId == input.OrganizationUnitId)
        //            .Select(st => st.UserId);

        //        var query = UserManager.Users
        //            .Where(u => !userIdsInOrganizationUnit.Contains(u.Id) && staffUserIds.Contains(u.Id))
        //            .WhereIf(
        //                !input.Filter.IsNullOrWhiteSpace(),
        //                u =>
        //                    (!string.IsNullOrEmpty(u.Name) && u.Name.Contains(input.Filter)) ||
        //                    (!string.IsNullOrEmpty(u.Surname) && u.Surname.Contains(input.Filter)) ||
        //                    (!string.IsNullOrEmpty(u.UserName) && u.UserName.Contains(input.Filter)) ||
        //                    (!string.IsNullOrEmpty(u.EmailAddress) && !string.IsNullOrWhiteSpace(u.EmailAddress) && u.EmailAddress.Contains(input.Filter))
        //            );

        //        var userCount = await query.CountAsync();
        //        var users = await query
        //            .OrderBy(u => u.UserName)
        //            .ThenBy(u => u.UserName)
        //            .PageBy(input)
        //            .ToListAsync();

        //        return new PagedResultDto<NameValueDto>(
        //            userCount,
        //            users.Select(u =>
        //                new NameValueDto(
        //                    u.FullName + " (" + u.EmailAddress + ")",
        //                    u.Id.ToString()
        //                )
        //            ).ToList()
        //        );
        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }
        //}


        public async Task<PagedResultDto<NameValueDto>> FindRoles(FindOrganizationUnitRolesInput input)
        {
            var roleIdsInOrganizationUnit = _organizationUnitRoleRepository.GetAll()
                .Where(uou => uou.OrganizationUnitId == input.OrganizationUnitId)
                .Select(uou => uou.RoleId);

            var query = _roleManager.Roles
                .Where(u => !roleIdsInOrganizationUnit.Contains(u.Id))
                .WhereIf(
                    !input.Filter.IsNullOrWhiteSpace(),
                    u =>
                        u.DisplayName.Contains(input.Filter) ||
                        u.Name.Contains(input.Filter)
                );

            var roleCount = await query.CountAsync();
            var users = await query
                .OrderBy(u => u.DisplayName)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<NameValueDto>(
                roleCount,
                users.Select(u =>
                    new NameValueDto(
                        u.DisplayName,
                        u.Id.ToString()
                    )
                ).ToList()
            );
        }

        private async Task<OrganizationUnitDto> CreateOrganizationUnitDto(OrganizationUnit organizationUnit)
        {
            var dto = ObjectMapper.Map<OrganizationUnitDto>(organizationUnit);
            dto.MemberCount = await _userOrganizationUnitRepository.CountAsync(uou => uou.OrganizationUnitId == organizationUnit.Id);
            return dto;
        }


        public async Task<object> GetOrganizationUnitByType(OrganizationUnitInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var query = (from tp in _organizationUnitRepository.GetAll()
                                 where tp.Type == input.Type
                                 /* join bd in _buildingRepos.GetAll() on tp.Type equals bd.Id into tb_bd
                                  from bd in tb_bd.DefaultIfEmpty()*/
                                 select new OrganizationUnitDto()
                                 {
                                     Type = tp.Type,
                                     CreationTime = tp.CreationTime,
                                     CreatorUserId = tp.CreatorUserId,
                                     Id = tp.Id,
                                     Description = tp.Description,
                                     ImageUrl = tp.ImageUrl,
                                     DisplayName = tp.DisplayName,
                                     ParentId = tp.ParentId,
                                     ProjectCode = tp.ProjectCode,
                                     TenantId = tp.TenantId,
                                     IsManager = tp.IsManager


                                 })
                                 .WhereIf(input.Keyword != null, x => (x.DisplayName != null && x.DisplayName.ToLower().Contains(input.Keyword.ToLower())) || (x.ProjectCode != null && x.ProjectCode.ToLower() == input.Keyword.ToLower()));

                    var result = await query.PageBy(input).ToListAsync();
                    var data = DataResult.ResultSuccess(result, "Get success!", query.Count());
                    return data;
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }

        }

        public async Task<object> GetOrganizationUnitByParentId(OrganizationUnitByParent input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var query = (from organizationUnit in _organizationUnitRepository.GetAll()
                                 select new OrganizationUnitDto()
                                 {
                                     Type = organizationUnit.Type,
                                     CreationTime = organizationUnit.CreationTime,
                                     CreatorUserId = organizationUnit.CreatorUserId,
                                     Id = organizationUnit.Id,
                                     Description = organizationUnit.Description,
                                     ImageUrl = organizationUnit.ImageUrl,
                                     DisplayName = organizationUnit.DisplayName,
                                     ParentId = organizationUnit.ParentId,
                                     ProjectCode = organizationUnit.ProjectCode,
                                     TenantId = organizationUnit.TenantId,
                                     IsManager = organizationUnit.IsManager,
                                     Code = organizationUnit.Code
                                 })
                                 .Where(x => x.Type != APP_ORGANIZATION_TYPE.NONE)
                                 .WhereIf(input.ParentId.HasValue, x => x.ParentId == input.ParentId)
                                 .WhereIf(input.Type.HasValue, x => x.Type == input.Type);

                    var result = query.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
                    return DataResult.ResultSuccess(result, "Get success!", query.Count());

                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public DataResult GetOrganizationUnitByParentIdNotPaging(OrganizationUnitByParentNotPagingInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    IQueryable<OrganizationUnitDto> query = (from organizationUnit in _organizationUnitRepository.GetAll()
                                                             select new OrganizationUnitDto()
                                                             {
                                                                 Id = organizationUnit.Id,
                                                                 Type = organizationUnit.Type,
                                                                 CreationTime = organizationUnit.CreationTime,
                                                                 CreatorUserId = organizationUnit.CreatorUserId,
                                                                 Description = organizationUnit.Description,
                                                                 ImageUrl = organizationUnit.ImageUrl,
                                                                 DisplayName = organizationUnit.DisplayName,
                                                                 ParentId = organizationUnit.ParentId,
                                                                 ProjectCode = organizationUnit.ProjectCode,
                                                                 TenantId = organizationUnit.TenantId,
                                                                 IsManager = organizationUnit.IsManager,
                                                                 Code = organizationUnit.Code
                                                             })
                                 .WhereIf(input.ParentId.HasValue, x => x.ParentId == input.ParentId)
                                 .Where(x => x.Type == APP_ORGANIZATION_TYPE.REPRESENTATIVE_NAME);
                    return DataResult.ResultSuccess(query.ToList(), "Get success!", query.Count());
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<OrganizationUnitDto> CreateUnitCharge(CreateOrganizationUnitInput input)
        {
            try
            {
                var organizationUnitName = new AppOrganizationUnit(AbpSession.TenantId, input.DisplayName, input.ParentId, APP_ORGANIZATION_TYPE.REPRESENTATIVE_NAME, input.IsManager);
                organizationUnitName.ImageUrl = input.ImageUrl;
                organizationUnitName.Description = input.Description;
                organizationUnitName.ProjectCode = input.ProjectCode;
                organizationUnitName.Code = await _organizationUnitManager.GetNextChildCodeAsync(input.ParentId);
                await CurrentUnitOfWork.SaveChangesAsync();

                var result = ObjectMapper.Map<OrganizationUnitDto>(input);
                result.Id = await _organizationUnitRepository.InsertAndGetIdAsync(organizationUnitName);

                return result;
            }
            catch (Exception e)
            {
                throw;
            }
        }


        private Tuple<List<APP_ORGANIZATION_TYPE>, List<APP_ORGANIZATION_TYPE>> CompareArrays(APP_ORGANIZATION_TYPE[] news, APP_ORGANIZATION_TYPE[] olds)
        {
            var adds = new List<APP_ORGANIZATION_TYPE>();
            var removes = new List<APP_ORGANIZATION_TYPE>();
            var dirs = new Dictionary<APP_ORGANIZATION_TYPE, bool>();
            int count = 1;
            foreach (var typenew in news)
            {

                var check = false;
                foreach (var typeold in olds)
                {
                    if (count == 1) dirs.Add(typeold, false);
                    if (typenew == typeold)
                    {
                        check = true;
                        dirs[typeold] = true;
                    }
                }
                count++;
                if (!check) adds.Add(typenew);
            }

            foreach (var dir in dirs)
            {
                if (!dir.Value) removes.Add(dir.Key);
            }
            return new Tuple<List<APP_ORGANIZATION_TYPE>, List<APP_ORGANIZATION_TYPE>>(adds, removes);
        }
    }
}
