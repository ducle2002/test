using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.IdentityFramework;
using Abp.Linq.Extensions;
using Abp.Notifications;
using Abp.Runtime.Validation;
using Abp.UI;
using Yootek.Application;
using Yootek.Authorization.Roles;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Notifications;
using Yootek.Organizations;
using Yootek.Organizations.OrganizationStructure;
using Yootek.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Yootek.Authorization;
using Yootek.QueriesExtension;


namespace Yootek.Services
{
    [AbpAuthorize]
    public class AdminStaffAppService : AsyncCrudAppService<Staff, StaffDto, long, PagedResultRequestDto, CreateStaffDto, StaffDto>
    {
        private readonly IRepository<Staff, long> _staffRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly RoleManager _roleManager;
        private readonly UserAppService _userService;
        private readonly UserManager _userManager;
        private readonly INotificationSubscriptionManager _notificationSubscriptionManager;
        private readonly IAppNotifier _appNotifier;
        private readonly IRepository<Role> _roleRepository;
        private readonly IRepository<Position, long> _positionRepository;
        private readonly IRepository<AppOrganizationUnit, long> _organizationRepos;
        private readonly IRepository<UserRole, long> _userRoleRepos;
        private readonly IRepository<OrganizationStructureDept, long> _departmentRepos;
        private readonly IStaffExcelExport _staffExcelExporter;


        public AdminStaffAppService(
            IRepository<Staff, long> staffRepository,
            IRepository<User, long> userRepository,
             RoleManager roleManager,
             UserAppService userService,
             UserManager userManager,
             INotificationSubscriptionManager notificationSubscriptionManager,
             IAppNotifier appNotifier,
             IRepository<Role> roleRepository,
             IRepository<Position, long> positionRepository,
             IRepository<AppOrganizationUnit, long> organizationRepos,
             IRepository<UserRole, long> userRoleRepos,
             IRepository<OrganizationStructureDept, long> departmentRepos,
             IStaffExcelExport staffExcelExporter

            )
            : base(staffRepository)
        {
            _staffRepository = staffRepository;
            _userRepository = userRepository;
            _roleManager = roleManager;
            _userService = userService;
            _userManager = userManager;
            _notificationSubscriptionManager = notificationSubscriptionManager;
            _appNotifier = appNotifier;
            _roleRepository = roleRepository;
            _positionRepository = positionRepository;
            _organizationRepos = organizationRepos;
            _userRoleRepos = userRoleRepos;
            _departmentRepos = departmentRepos;
            _staffExcelExporter = staffExcelExporter;

        }
        public async Task<object> GetAllStaffAsync(GetStaffInput input)
        {

            try
            {
                List<long> buIds = _userManager.GetAccessibleBuildingOrUrbanIds();
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {

                    var query = (from st in _staffRepository.GetAll()
                                     //    // Bắt buộc
                                     //join us in _userRepository.GetAll() on st.UserId equals us.Id into tb_us
                                     //from us in tb_us
                                 join po in _positionRepository.GetAll() on st.PositionId equals po.Id into tb_po
                                 from po in tb_po.DefaultIfEmpty()
                                 join ou in _organizationRepos.GetAll() on st.OrganizationUnitId equals ou.Id into tb_ou
                                 from ou in tb_ou.DefaultIfEmpty()
                                 join du in _departmentRepos.GetAll() on st.DepartmentUnitId equals du.Id into tb_du
                                 from du in tb_du.DefaultIfEmpty()
                                     //join du in _departmentRepos.GetAll() on st.DepartmentUnitId equals du.Id into tb_du
                                     //from du in tb_du.DefaultIfEmpty()
                                 select new StaffInput()
                                 {
                                     CreationTime = st.CreationTime,
                                     CreatorUserId = st.CreatorUserId,
                                     Id = st.Id,
                                     Specialize = st.Specialize,
                                     OrganizationUnitId = st.OrganizationUnitId,
                                     PositionId = st.PositionId,
                                     UserId = st.UserId,
                                     AccountName = st.AccountName,
                                     Name = st.Name,
                                     Surname = st.Surname,
                                     Email = st.Email,
                                     TenantId = st.TenantId,
                                     PositionName = (po != null) ? po.DisplayName : null,
                                     OrganizationUnitName = (ou != null) ? ou.DisplayName : null,
                                     UrbanId = (ou != null) ? ou.ParentId : null,
                                     BuildingId = _organizationRepos.GetAll().Where(u => u.Id == st.OrganizationUnitId && u.ParentId != null && u.Type == 0).Select(u => u.Id).FirstOrDefault(),
                                     DepartmentUnitId = st.DepartmentUnitId,
                                     DepartmentUnitName = (du != null) ? du.DisplayName : null,
                                     Type = st.Type
                                 })
                                 .WhereByBuildingOrUrbanIf(!IsGranted(IOCPermissionNames.Data_Admin), buIds)
                                 .WhereIf(input.OrganizationUnitId != null, x => input.OrganizationUnitId == x.OrganizationUnitId)
                                 .WhereIf(input.DepartmentUnitId != null, x => input.DepartmentUnitId == x.DepartmentUnitId)
                                 .WhereIf(input.Type.HasValue, x => input.Type == x.Type)
                                 .WhereIf(input.UrbanId != null, x => input.UrbanId == x.UrbanId)
                                 .ApplySearchFilter(input.Keyword, x => x.Name, x => x.AccountName, x => x.Email)
                                 .AsQueryable();

                    var result = query
                            .ApplySort(input.OrderBy, input.SortBy)
                            .ApplySort(OrderByStaff.NAME)
                    .Skip(input.SkipCount).Take(input.MaxResultCount).ToList();

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

        //public override async Task<StaffDto> CreateAsync(CreateStaffDto input)
        //{
        //    try
        //    {
        //        CheckCreatePermission();

        //        var existingStaff = await _staffRepository.FirstOrDefaultAsync(st => st.Email == input.Email);

        //        if (existingStaff != null)
        //        {
        //            throw new AbpValidationException("Tài khoản nhân viên đã tồn tại.", new List<ValidationResult>() {
        //        new ValidationResult("Tài khoản nhân viên đã tồn tại.", new [] { "AccountName" })
        //    });
        //        }
        //        else
        //        {
        //            User user = null;
        //            Staff staff = null;

        //            if (input.UserId != 0) // Nếu UserId khác 0, sử dụng thông tin user đã có
        //            {
        //                user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == input.UserId);
        //                staff = new Staff()
        //                {
        //                    Name = input.Name,
        //                    Surname = input.Surname,
        //                    AccountName = user.UserName,
        //                    Password = user.Password,
        //                    Email = input.Email,
        //                    DateOfBirth = input.DateOfBirth,
        //                    AddressOfBirth = input.AddressOfBirth,
        //                    Specialize = input.Specialize,
        //                    TenantId = AbpSession.TenantId,
        //                    PositionId = input.PositionId,
        //                    OrganizationUnitId = input.OrganizationUnitId,
        //                    UserId = user.Id,
        //                    DepartmentUnitId = input.DepartmentUnitId,
        //                    Type = input.Type
        //                };
        //            }
        //            else // Nếu UserId == 0, tạo mới user và staff
        //            {
        //                user = new User()
        //                {
        //                    UserName = input.AccountName,
        //                    EmailAddress = input.Email,
        //                    Name = input.Name,
        //                    Surname = input.Surname,
        //                    Password = input.Password,
        //                    TenantId = AbpSession.TenantId,
        //                    IsActive = true,
        //                    IsEmailConfirmed = true
        //                };

        //                CheckErrors(await _userManager.CreateAsync(user, input.Password));

        //                if (input.RoleNames != null)
        //                {
        //                    CheckErrors(await _userManager.SetRolesAsync(user, input.RoleNames));
        //                }

        //                staff = new Staff()
        //                {
        //                    Name = input.Name,
        //                    Surname = input.Surname,
        //                    AccountName = user.UserName,
        //                    Password = user.Password, 
        //                    Email = input.Email,
        //                    DateOfBirth = input.DateOfBirth,
        //                    AddressOfBirth = input.AddressOfBirth,
        //                    Specialize = input.Specialize,
        //                    TenantId = AbpSession.TenantId,
        //                    PositionId = input.PositionId,
        //                    OrganizationUnitId = input.OrganizationUnitId,
        //                    UserId = user.Id,
        //                    DepartmentUnitId = input.DepartmentUnitId,
        //                    Type = input.Type
        //                };
        //            }

        //            var createdStaff = await _staffRepository.InsertAsync(staff);
        //            var staffDto = ObjectMapper.Map<StaffDto>(createdStaff);

        //            return staffDto;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        var data = DataResult.ResultError(e.ToString(), "Exception !");
        //        Logger.Fatal(e.Message);
        //        throw;
        //    }
        //}
        public override async Task<StaffDto> CreateAsync(CreateStaffDto input)
        {
            try
            {
                CheckCreatePermission();

                var existingStaff = await _staffRepository.FirstOrDefaultAsync(st => st.Email == input.Email);

                if (existingStaff != null)
                {
                    throw new AbpValidationException("Tài khoản nhân viên đã tồn tại.", new List<ValidationResult>() {
                new ValidationResult("Tài khoản nhân viên đã tồn tại.", new [] { "AccountName" })
            });
                }

                User user = null;

                if (input.UserId != 0)
                {
                    user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == input.UserId);
                }
                else
                {
                    user = new User()
                    {
                        UserName = input.AccountName,
                        EmailAddress = input.Email,
                        Name = input.Name,
                        Surname = input.Surname,
                        Password = input.Password,
                        TenantId = AbpSession.TenantId,
                        IsActive = true,
                        IsEmailConfirmed = true
                    };

                    CheckErrors(await _userManager.CreateAsync(user, input.Password));

                    if (input.RoleNames != null)
                    {
                        CheckErrors(await _userManager.SetRolesAsync(user, input.RoleNames));
                    }
                }

                var staff = new Staff()
                {
                    Name = input.Name,
                    Surname = input.Surname,
                    AccountName = user.UserName,
                    Password = user.Password,
                    Email = input.Email,
                    DateOfBirth = input.DateOfBirth,
                    AddressOfBirth = input.AddressOfBirth,
                    Specialize = input.Specialize,
                    TenantId = AbpSession.TenantId,
                    PositionId = input.PositionId,
                    OrganizationUnitId = input.OrganizationUnitId,
                    UserId = user.Id,
                    DepartmentUnitId = input.DepartmentUnitId,
                    Type = input.Type
                };

                var createdStaff = await _staffRepository.InsertAsync(staff);
                var staffDto = ObjectMapper.Map<StaffDto>(createdStaff);

                return staffDto;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
        public async Task DeleteStaffWithUserAsync(EntityDto<long> input, bool deleteUser = false)
        {
            var staff = await _staffRepository.GetAsync(input.Id);
            await _staffRepository.DeleteAsync(staff);
            if (deleteUser)
            {
                var user = await _userRepository.FirstOrDefaultAsync(u => u.Id == staff.UserId);
                if (user != null)
                {
                    await _userRepository.DeleteAsync(user);
                }
            }
        }
        public async Task<DataResult> DeleteStaff(long id)
        {
            try
            {

                await _staffRepository.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Delete success!");
                return data;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetStaffByIdAsync(long id)
        {
            try
            {
                var query = from st in _staffRepository.GetAll()
                                //join us in _userRepository.GetAll() on st.UserId equals us.Id into tb_us
                                //from us in tb_us.DefaultIfEmpty()
                            join ou in _organizationRepos.GetAll() on st.OrganizationUnitId equals ou.Id into tb_ou
                            from ou in tb_ou.DefaultIfEmpty()
                            join po in _positionRepository.GetAll() on st.PositionId equals po.Id into tb_po
                            from po in tb_po.DefaultIfEmpty()
                            join du in _departmentRepos.GetAll() on st.DepartmentUnitId equals du.Id into tb_du
                            from du in tb_du.DefaultIfEmpty()
                            where st.Id == id
                            select new StaffInput()
                            {
                                CreationTime = st.CreationTime,
                                CreatorUserId = st.CreatorUserId,
                                Id = st.Id,
                                Specialize = st.Specialize,
                                OrganizationUnitId = st.OrganizationUnitId,
                                DepartmentUnitId = st.DepartmentUnitId,
                                PositionId = st.PositionId,
                                UserId = st.UserId,
                                AccountName = st.AccountName,
                                Email = st.Email,
                                Name = st.Name,
                                Surname = st.Surname,
                                TenantId = st.TenantId,
                                PositionName = (po != null) ? po.DisplayName : null,
                                OrganizationUnitName = (ou != null) ? ou.DisplayName : null,
                                DepartmentUnitName = (du != null) ? du.DisplayName : null,
                                AddressOfBirth = st.AddressOfBirth,
                                DateOfBirth = st.DateOfBirth,
                                Type = st.Type,

                                RoleNames = _userRepository.GetAll()
                                .Where(u => u.Id == st.UserId)
                                .SelectMany(u => u.Roles)
                                .Join(_roleRepository.GetAll(), ur => ur.RoleId, ro => ro.Id, (ur, ro) => ro.NormalizedName)
                                .ToArray()
                            };

                var result = await query.FirstOrDefaultAsync();

                if (result == null)
                {
                    throw new EntityNotFoundException(typeof(Staff), id);
                }

                var data = DataResult.ResultSuccess(result, "Get success!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }


        public async Task<object> UpdateStaffAsync(UpdateInput input)
        {

            //update
            var updateData = await _staffRepository.GetAsync(input.Id);
            var user = await _userManager.GetUserByIdAsync(input.UserId);
            if (updateData != null)
            {
                input.MapTo(updateData);
                if (input.RoleNames != null && user != null)
                {
                    CheckErrors(await _userManager.SetRolesAsync(user, input.RoleNames));
                }

                //call back
                await _staffRepository.UpdateAsync(updateData);
            }
            // mb.statisticMetris(t1, 0, "Ud_staff");
            var data = DataResult.ResultSuccess(updateData, "Update success !");
            return data;
        }
        public Task<DataResult> DeleteListStaff([FromBody] List<long> ids)
        {
            try
            {

                if (ids.Count == 0) return Task.FromResult(DataResult.ResultError("Err", "input empty"));
                var tasks = new List<Task>();
                foreach (var id in ids)
                {
                    var tk = DeleteStaff(id);
                    tasks.Add(tk);
                }
                Task.WaitAll(tasks.ToArray());

                var data = DataResult.ResultSuccess("Delete success!");
                return Task.FromResult(data);
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                return Task.FromResult(data);
            }
        }
        public async Task<object> ExportAllStaffExcel()
        {
            try
            {
                var staffs = await _staffRepository.GetAll().ToListAsync();

                var result = _staffExcelExporter.ExportStaffToExcel(staffs);
                return DataResult.ResultSuccess(result, "Export success");
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
        public async Task<object> ExportListStaffExcel(StaffExportDto input)
        {
            try
            {
                var staffList = await _staffRepository.GetAll()
                    .WhereIf(input.Ids != null && input.Ids.Count > 0, x => input.Ids.Contains(x.Id))
                    .ToListAsync();

                var result = _staffExcelExporter.ExportStaffToExcel(staffList);
                return DataResult.ResultSuccess(result, "Export success");
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
        public async Task<object> ImportStaffExcel([FromForm] ImportStaffExcelDto input)
        {
            try
            {
                var file = input.Form;
                if (file == null || file.Length <= 0)
                {
                    return DataResult.ResultError("No file uploaded.", "Error");
                }

                var fileExt = Path.GetExtension(file.FileName);
                if (fileExt != ".xlsx" && fileExt != ".xls")
                {
                    return DataResult.ResultError("Invalid file format. Only .xlsx and .xls files are supported.", "Error");
                }

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    var package = new ExcelPackage(stream);
                    var worksheet = package.Workbook.Worksheets.First();

                    for (var row = 2; row <= worksheet.Dimension.End.Row; row++)
                    {
                        var email = worksheet.Cells[row, 4].Value.ToString(); // Get email from Excel
                        var accountName = worksheet.Cells[row, 1].Value.ToString();
                        var name = worksheet.Cells[row, 2].Value.ToString();
                        var surname = worksheet.Cells[row, 3].Value.ToString();
                        var password = worksheet.Cells[row, 5].Value.ToString();
                        var dateOfBirth = DateTime.Parse(worksheet.Cells[row, 6].Value.ToString());


                        var existingStaff = await _staffRepository.FirstOrDefaultAsync(s => s.Email == email);
                        var existingUser = await _userManager.FindByEmailAsync(email);

                        //không có email trùng của cả staff và user
                        if (existingStaff == null && existingUser == null)
                        {
                            var user = new User
                            {
                                UserName = accountName,
                                EmailAddress = email,
                                Name = name,
                                Surname = surname,
                                Password = password,
                                TenantId = AbpSession.TenantId,
                                IsActive = true,
                                IsEmailConfirmed = true
                            };

                            CheckErrors(await _userManager.CreateAsync(user, user.Password));

                            var staff = new Staff
                            {
                                Name = name,
                                Surname = surname,
                                AccountName = accountName,
                                Password = user.Password,
                                Email = email,
                                DateOfBirth = dateOfBirth,
                                TenantId = AbpSession.TenantId,
                                UserId = user.Id,
                            };
                            await _staffRepository.InsertAsync(staff);
                        }
                        //trùng email với user (có tk user), không có staff
                        else if (existingStaff == null && existingUser != null)
                        {
                            var staff = new Staff
                            {
                                Name = name,
                                Surname = surname,
                                AccountName = existingUser.UserName,
                                Password = existingUser.Password,
                                Email = email,
                                DateOfBirth = dateOfBirth,
                                TenantId = AbpSession.TenantId,
                                UserId = existingUser.Id,

                            };
                            await _staffRepository.InsertAsync(staff);
                        }
                        // Skip if staff exists
                    }

                    // Save changes to the database
                    await CurrentUnitOfWork.SaveChangesAsync();

                    return DataResult.ResultSuccess(null, "Import success");
                }
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        //public async Task<object> GetStaffByUnit(GetStaffUnitInput input)
        //{
        //    try
        //    {
        //        using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
        //        {
        //            var query = (from tp in _staffRepository.GetAll()
        //                         select new StaffDto()
        //                         {
        //                             Type = tp.Type,
        //                             CreationTime = tp.CreationTime,
        //                             CreatorUserId = tp.CreatorUserId,
        //                             Id = tp.Id,
        //                             Description = tp.Description,
        //                             ImageUrl = tp.ImageUrl,
        //                             DisplayName = tp.DisplayName,
        //                             ParentId = tp.ParentId,
        //                             ProjectCode = tp.ProjectCode,
        //                             TenantId = tp.TenantId,
        //                             IsManager = tp.IsManager,
        //                             Code = tp.Code


        //                         })
        //                         .WhereIf(input.ParentId.HasValue, x => x.ParentId == input.ParentId)
        //                         .WhereIf(input.Type.HasValue, x => x.Type == input.Type);

        //            //var result = await query.ToListAsync();

        //            var result = query.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
        //            var data = DataResult.ResultSuccess(result, "Get success!", query.Count());
        //            return data;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        var data = DataResult.ResultError(e.ToString(), "Exception !");
        //        Logger.Fatal(e.Message);
        //        throw;
        //    }


        //}
    }
}
