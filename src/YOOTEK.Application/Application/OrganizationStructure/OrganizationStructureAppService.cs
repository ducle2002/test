using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Application.OrganizationStructure.Dto;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Organizations;
using Yootek.Organizations.OrganizationStructure;
using Yootek.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yootek.Application.OrganizationStructure
{
    public interface IOrganizationStructureAppService : IApplicationService
    {
        DataResult GetDepartmentStructureAsync();
        Task<DataResult> GetAllAccountAsync(OrganizationStructureDeptUsersDto input);
    }
    public class OrganizationStructureAppService : YootekAppServiceBase, IOrganizationStructureAppService
    {
        private readonly IRepository<OrganizationStructureDept, long> _deptRepo;
        private readonly IRepository<OrganizationStructureDeptUser, long> _deptUserRepo;
        private readonly IRepository<OrganizationStructureUnit, long> _unitRepo;
        private readonly IRepository<DeptToUnit, long> _dtuRepo;
        private readonly IRepository<UnitToUnit, long> _utuRepo;
        private readonly IRepository<Staff, long> _staffRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Position, long> _positionRepository;
        private readonly IRepository<AppOrganizationUnit, long> _organizationRepos;
        private readonly IRepository<DepartmentOrganizationUnit, long> _dtOuRepo;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationUnitRepository;
        private readonly AppOrganizationUnitManager _organizationUnitManager;

        public OrganizationStructureAppService(
            IRepository<OrganizationStructureDept, long> deptRepo,
            IRepository<OrganizationStructureDeptUser, long> deptUserRepo,
            IRepository<OrganizationStructureUnit, long> unitRepo,
            IRepository<DeptToUnit, long> dtuRepo,
            IRepository<UnitToUnit, long> utuRepo,
            IRepository<Staff, long> staffRepository,
            IRepository<User, long> userRepository,
            IRepository<Position, long> positionRepository,
            IRepository<AppOrganizationUnit, long> organizationRepos,
            IRepository<DepartmentOrganizationUnit, long> dtOuRepo,
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,
            AppOrganizationUnitManager organizationUnitManager
        )
        {
            _deptRepo = deptRepo;
            _deptUserRepo = deptUserRepo;
            _unitRepo = unitRepo;
            _dtuRepo = dtuRepo;
            _utuRepo = utuRepo;
            _staffRepository = staffRepository;
            _userRepository = userRepository;
            _positionRepository = positionRepository;
            _organizationRepos = organizationRepos;
            _dtOuRepo = dtOuRepo;
            _userOrganizationUnitRepository = userOrganizationUnitRepository;
            _organizationUnitManager = organizationUnitManager;
        }

        public async Task<object> GetAllUnitsAsync(OrganizationStructureInputDto input)
        {
            try
            {
                var query = (from u1 in _unitRepo.GetAll()
                             select new OrganizationStructureUnitOutputDto()
                             {
                                 Id = u1.Id,
                                 DisplayName = u1.DisplayName,
                                 ParentUnitIds = (from u2 in _utuRepo.GetAll()
                                                  where u2.ChildId == u1.Id
                                                  select u2.ParentId).ToList()
                             })
                             .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword), x => x.DisplayName.ToLower().Contains(input.Keyword.ToLower()));
                var result = await query.PageBy(input).ToListAsync();
                var data = DataResult.ResultSuccess(result, "Get success", query.Count());
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "ResultFail");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> GetUnitByIdAsync(long id)
        {
            try
            {
                var raw = await _unitRepo.GetAsync(id);
                var data = new OrganizationStructureUnitOutputDto()
                {
                    Id = raw.Id,
                    DisplayName = raw.DisplayName,
                    ParentUnitIds = (from u2 in _utuRepo.GetAll()
                                     where u2.ChildId == raw.Id
                                     select u2.ParentId).ToList()
                };
                return DataResult.ResultSuccess(data, "Success");
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "ResultFail");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public DataResult GetDepartmentStructureAsync()
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    List<DepartmentDto> departmentDtos = _deptRepo.GetAll()
                        .Where(x => x.ParentId == null)
                        .Select(x => new DepartmentDto()
                        {
                            Id = x.Id,
                            Childs = _deptRepo.GetAll().Where(d => d.ParentId == x.Id)
                                .Select(d => new DepartmentChildDto()
                                {
                                    Id = d.Id,
                                    Description = d.Description,
                                    DisplayName = d.DisplayName,
                                    ImageUrl = d.ImageUrl,
                                    Staffs = _staffRepository.GetAll()
                                        .Where(s => s.DepartmentUnitId == d.Id)
                                        .Select(s => new DepartmentStaffDto
                                        {
                                            Id = s.UserId,
                                            Name = s.Name,
                                            Surname = s.Surname,
                                            Username = s.AccountName
                                        }).ToList(),
                                }).ToList(),
                            ParentId = x.ParentId,
                            Description = x.Description,
                            DisplayName = x.DisplayName,
                            ImageUrl = x.ImageUrl,
                        }).ToList();
                    return DataResult.ResultSuccess(departmentDtos, "Get success", departmentDtos.Count);
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
        public async Task<object> GetAllDepartmentsAsync(OrganizationStructureInputDto input)
        {
            try
            {
                var query = (from d in _deptRepo.GetAll()
                             join unit in _unitRepo.GetAll() on d.UnitId equals unit.Id into tbl_dept
                             from dpt in tbl_dept.DefaultIfEmpty()
                                 //join user in _deptUserRepo.GetAll() on d.Id equals user.DeptId
                             select new OrganizationStructureDeptOutputDto()
                             {
                                 Id = d.Id,
                                 DisplayName = d.DisplayName,
                                 UnitId = dpt.Id,
                                 UnitName = dpt.DisplayName,
                                 Description = d.Description,
                                 ImageUrl = d.ImageUrl,
                                 UserCount = _deptUserRepo.GetAll().Where(x => x.DeptId == d.Id).Count(),
                                 ParentId = d.ParentId,
                                 UnitIds = (from dt in _dtuRepo.GetAll()
                                            where dt.DeptId == d.Id
                                            select dt.UnitId).ToList(),
                             })
                             .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword), x => x.DisplayName.ToLower().Contains(input.Keyword.ToLower()))
                             .AsQueryable();
                var result = await query.PageBy(input).ToListAsync();
                var data = DataResult.ResultSuccess(result, "Get success", query.Count());
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "ResultFail");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
        public async Task<object> GetDeptByIdAsync(long id)
        {
            try
            {
                var raw = await _deptRepo.GetAsync(id);
                var data = new OrganizationStructureDeptOutputDto()
                {
                    Id = raw.Id,
                    DisplayName = raw.DisplayName,
                    Description = raw.Description,
                    ImageUrl = raw.ImageUrl,
                    UserCount = _deptUserRepo.GetAll().Where(x => x.DeptId == raw.Id).Count(),
                    ParentId = raw.ParentId,
                    UnitIds = (from dt in _dtuRepo.GetAll()
                               where dt.DeptId == raw.Id
                               select dt.UnitId).ToList(),
                };
                return DataResult.ResultSuccess(data, "Success!");
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "ResultFail");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        [Obsolete]
        public async Task<object> CreateOrUpdateUnitAsync(OrganizationStructureUnitInputDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    var remove_ids = new List<long>();
                    var add_ids = new List<long>();
                    var utu_ids = _utuRepo.GetAll().Where(x => x.ChildId == input.Id);
                    if (await utu_ids.CountAsync() > 0)
                    {
                        remove_ids = await utu_ids.Where(x => input.ParentUnitIds.All(y => y != x.ParentId)).Select(x => x.Id).ToListAsync();
                        add_ids = input.ParentUnitIds.Where(x => utu_ids.All(y => y.ParentId != x)).ToList();
                    }
                    else
                    {
                        add_ids = input.ParentUnitIds;
                    }
                    var updateData = await _unitRepo.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        if (remove_ids.Count > 0)
                        {
                            foreach (long i in remove_ids)
                            {
                                await _utuRepo.DeleteAsync(i);
                            }
                        }
                        input.MapTo(updateData);
                        await _unitRepo.UpdateAsync(updateData);
                        if (add_ids.Count > 0)
                        {
                            foreach (var i in add_ids)
                            {
                                await _utuRepo.InsertAsync(new UnitToUnit
                                {
                                    TenantId = AbpSession.TenantId,
                                    ChildId = updateData.Id,
                                    ParentId = i,
                                });
                            }
                        }
                    }
                    mb.statisticMetris(t1, 0, "Ud_unit");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    var insertData = input.MapTo<OrganizationStructureUnit>();
                    var id = await _unitRepo.InsertAndGetIdAsync(insertData);
                    if (input.ParentUnitIds != null)
                    {
                        foreach (var i in input.ParentUnitIds)
                        {
                            await _utuRepo.InsertAsync(new UnitToUnit
                            {
                                TenantId = AbpSession.TenantId,
                                ChildId = id,
                                ParentId = i,
                            });
                        }
                    }
                    mb.statisticMetris(t1, 0, "Insert_unit");
                    var data = DataResult.ResultSuccess(insertData, "Insert success!");
                    return data;
                }
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "ResultFail");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        [Obsolete]
        public async Task<object> CreateOrUpdateDepartmentAsync(OrganizationStructureDeptInputDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    var remove_ids = new List<long>();
                    var add_ids = new List<long>();
                    var dtu_ids = _dtuRepo.GetAll().Where(x => x.DeptId == input.Id);
                    if (await dtu_ids.CountAsync() > 0)
                    {
                        remove_ids = await dtu_ids.Where(x => input.UnitIds.All(y => y != x.UnitId)).Select(x => x.Id).ToListAsync();
                        add_ids = input.UnitIds.Where(x => dtu_ids.All(y => y.UnitId != x)).ToList();
                    }
                    else
                    {
                        add_ids = input.UnitIds;
                    }
                    var updateData = await _deptRepo.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        if (remove_ids.Count > 0)
                        {
                            foreach (long i in remove_ids)
                            {
                                await _dtuRepo.DeleteAsync(i);
                            }
                        }
                        input.MapTo(updateData);
                        await _deptRepo.UpdateAsync(updateData);
                        if (add_ids.Count > 0)
                        {
                            foreach (var i in add_ids)
                            {
                                await _dtuRepo.InsertAsync(new DeptToUnit()
                                {
                                    TenantId = AbpSession.TenantId,
                                    DeptId = updateData.Id,
                                    UnitId = i,
                                });
                            }
                        }
                    }
                    mb.statisticMetris(t1, 0, "Ud_dept");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    var insertData = input.MapTo<OrganizationStructureDept>();
                    var id = await _deptRepo.InsertAndGetIdAsync(insertData);
                    if (input.UnitIds != null)
                    {
                        foreach (var i in input.UnitIds)
                        {
                            await _dtuRepo.InsertAsync(new DeptToUnit()
                            {
                                TenantId = AbpSession.TenantId,
                                DeptId = id,
                                UnitId = i,
                            });
                        }
                    }
                    mb.statisticMetris(t1, 0, "Insert_unit");
                    var data = DataResult.ResultSuccess(insertData, "Insert success!");
                    return data;
                }
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "ResultFail");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> DeleteUnitAsync(long id)
        {
            try
            {
                await _unitRepo.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Deleted!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> DeleteDepartmentAsync(long id)
        {
            try
            {
                await _deptRepo.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Deleted!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public object DeleteMultipleUnits([FromBody] List<long> ids)
        {
            try
            {
                if (ids.Count == 0) return DataResult.ResultError("Error", "Empty input!");
                var tasks = new List<Task>();
                foreach (var id in ids)
                {
                    var task = DeleteUnitAsync(id);
                    tasks.Add(task);
                }
                Task.WaitAll(tasks.ToArray());
                var data = DataResult.ResultSuccess("Deleted successfully!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public object DeleteMultipleDepartments([FromBody] List<long> ids)
        {
            try
            {
                if (ids.Count == 0) return DataResult.ResultError("Error", "Empty input!");
                var tasks = new List<Task>();
                foreach (var id in ids)
                {
                    var task = DeleteDepartmentAsync(id);
                    tasks.Add(task);
                }
                Task.WaitAll(tasks.ToArray());
                var data = DataResult.ResultSuccess("Deleted successfully!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> GetAllAccountAsync(OrganizationStructureDeptUsersDto input)
        {
            try
            {
                IQueryable<AccountDepartmentDto> query = (from userDepartment in _deptUserRepo.GetAll()
                                                          join user in UserManager.Users on userDepartment.UserId equals user.Id
                                                          where userDepartment.DeptId == input.Id
                                                          select new AccountDepartmentDto
                                                          {
                                                              Id = userDepartment.UserId,
                                                              UserName = user.UserName,
                                                              DisplayName = user.FullName,
                                                              AddedTime = userDepartment.CreationTime,
                                                          });
                List<AccountDepartmentDto> result = await query.PageBy(input).ToListAsync();
                return DataResult.ResultSuccess(result, "Get success", query.Count());
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> GetAvailableUsers(OrganizationStructureFindUsersDto input)
        {
            try
            {
                var addedIds = await _deptUserRepo.GetAll().Where(x => x.DeptId == input.Id).Select(x => x.UserId).ToListAsync();
                var query = (from st in _staffRepository.GetAll()
                             join us in _userRepository.GetAll() on st.UserId equals us.Id into tb_us
                             from us in tb_us.DefaultIfEmpty()
                             join po in _positionRepository.GetAll() on st.PositionId equals po.Id into tb_po
                             from po in tb_po.DefaultIfEmpty()
                             join ou in _organizationRepos.GetAll() on st.OrganizationUnitId equals ou.Id into tb_ou
                             from ou in tb_ou.DefaultIfEmpty()
                             select new StaffInput()
                             {
                                 CreationTime = st.CreationTime,
                                 CreatorUserId = st.CreatorUserId,
                                 Id = st.Id,
                                 Specialize = st.Specialize,
                                 OrganizationUnitId = st.OrganizationUnitId,
                                 PositionId = st.PositionId,
                                 UserId = st.UserId,
                                 UserName = us.UserName,
                                 Name = us.Name,
                                 Surname = us.Surname,
                                 EmailAddress = us.EmailAddress,
                                 IsActive = us.IsActive,
                                 TenantId = st.TenantId,
                                 PositionName = po.DisplayName,
                                 OrganizationUnitName = ou.DisplayName,
                                 UrbanId = ou.ParentId

                             }).Where(x => !addedIds.Contains(x.UserId))
                               .WhereIf(!String.IsNullOrWhiteSpace(input.Keyword), u =>
                                    (!string.IsNullOrEmpty(u.Name) && u.Name.Contains(input.Keyword)) ||
                                    (!string.IsNullOrEmpty(u.Surname) && u.Surname.Contains(input.Keyword)) ||
                                    (!string.IsNullOrEmpty(u.UserName) && u.UserName.Contains(input.Keyword)) ||
                                    (!string.IsNullOrEmpty(u.EmailAddress) && !string.IsNullOrWhiteSpace(u.EmailAddress) && u.EmailAddress.Contains(input.Keyword))
                    );
                var userCount = await query.CountAsync();
                var users = await query
                    .OrderBy(u => u.UserName)
                    .ThenBy(u => u.UserName)
                    .PageBy(input)
                    .ToListAsync();
                var data = DataResult.ResultSuccess(users, "Get success", userCount);
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "ResultFail");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> InsertUsers(OrganizationStructureInsertUsersDto input)
        {
            try
            {
                foreach (var i in input.UserIds)
                {
                    var deptU = new OrganizationStructureDeptUser()
                    {
                        TenantId = AbpSession.TenantId,
                        DeptId = input.Id,
                        UserId = i

                    };
                    await _deptUserRepo.InsertAndGetIdAsync(deptU);
                    if (AbpSession.TenantId.HasValue)
                    {
                        _organizationUnitManager.CreateDepartmentUserCache(deptU);
                    }
                }
                var data = DataResult.ResultSuccess("Insert success!");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "ResultFail");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> DeleteAccountAsync(long id)
        {
            try
            {
                await _deptUserRepo.DeleteAsync(id);
                if (AbpSession.TenantId.HasValue)
                {
                    _organizationUnitManager.DeleteDepartmentUserCache(id, AbpSession.TenantId.Value);
                }
                var data = DataResult.ResultSuccess("Deleted!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetOrganizationUnitDepartments(GetDepartmentOrganizationUnitInputDto input)
        {
            try
            {
                var query = from ouDept in _dtOuRepo.GetAll()
                            join ou in _organizationRepos.GetAll() on ouDept.OrganizationUnitId equals ou.Id
                            join dept in _deptRepo.GetAll() on ouDept.DeptId equals dept.Id
                            where ouDept.OrganizationUnitId == input.Id
                            select new
                            {
                                ouDept,
                                ou,
                                dept
                            };
                var count = await query.CountAsync();
                var data = await query.PageBy(input).ToListAsync();
                var result = DataResult.ResultSuccess(data, "Query success", count);
                return result;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> AddDepartmentToOrganizationUnitAsync(AddDepartmentToOrganizationUnitDto input)
        {
            try
            {
                foreach (var i in input.DeptIds)
                {
                    var entry = new DepartmentOrganizationUnit()
                    {
                        TenantId = AbpSession.TenantId,
                        OrganizationUnitId = input.Id,
                        DeptId = i

                    };
                    await _dtOuRepo.InsertAndGetIdAsync(entry);
                    if (AbpSession.TenantId.HasValue)
                    {
                        _organizationUnitManager.CreateDepartmentOrganizationUnitCache(entry);
                    }
                }
                var data = DataResult.ResultSuccess("Insert success!");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "ResultFail");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> DeleteDepartmentOrganizationUnitAsync(long id)
        {
            try
            {
                await _dtOuRepo.DeleteAsync(id);
                if (AbpSession.TenantId.HasValue)
                {
                    _organizationUnitManager.DeleteDepartmentOrganizationUnitCache(id, AbpSession.TenantId.Value);
                }
                var data = DataResult.ResultSuccess("Deleted!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<object> GetAvailableOrganizationUnitDepartmentsAsync(OrganizationStructureFindDepartmentDto input)
        {
            try
            {
                var addedIds = await _dtOuRepo.GetAll()
                    .Where(x => x.OrganizationUnitId == input.Id)
                    .Select(x => x.DeptId).ToListAsync();
                var query = _deptRepo.GetAll()
                    .Where(x => !addedIds.Contains(x.Id))
                    .WhereIf(!String.IsNullOrWhiteSpace(input.Keyword), x => !string.IsNullOrEmpty(x.DisplayName) && x.DisplayName.Contains(input.Keyword));
                var count = await query.CountAsync();
                var list = await query.PageBy(input).ToListAsync();
                var data = DataResult.ResultSuccess(list, "Get success", count);
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "ResultFail");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<PagedResultDto<object>> GetOrganizationUnitsByUser()
        {
            try
            {
                var query = from deptU in _deptUserRepo.GetAll()
                            join dept in _deptRepo.GetAll() on deptU.DeptId equals dept.Id //into tbl_dept
                            //from dept in tbl_dept
                            join dOu in _dtOuRepo.GetAll() on dept.Id equals dOu.DeptId //into tbl_dOu
                            //from dOu in tbl_dOu
                            join org in _organizationRepos.GetAll() on dOu.OrganizationUnitId equals org.Id //into tbl_org
                            //from org in tbl_org
                            where deptU.UserId == AbpSession.UserId
                            select new
                            {
                                OrganizationUnitId = org.Id,
                                Type = org.Type,
                                TenantCode = org.ProjectCode,
                                DisplayName = org.DisplayName,
                                ImageUrl = org.ImageUrl,
                                IsManager = org.IsManager,
                                Types = (from ou in _organizationRepos.GetAll()
                                         where ou.ParentId == org.Id
                                         select ou.Type).ToArray(),
                                ParentId = org.ParentId,
                            };
                var totalCount = await query.CountAsync();
                var items = await query.ToListAsync();
                return new PagedResultDto<object>(
                    totalCount,
                    items.ToList());
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "ResultFail");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> GetOrganizationUnitByUnit()
        {
            try
            {
                var query = (from org in _organizationRepos.GetAll()
                             where org.Type == APP_ORGANIZATION_TYPE.REPRESENTATIVE_NAME && org.ParentId == null
                             select new
                             {
                                 //DepartmentId = d.DeptId,
                                 Type = org.Type,
                                 Code = org.Code,
                                 Description = org.Description,
                                 DisplayName = org.DisplayName,
                                 Id = org.Id,
                                 ImageUrl = org.ImageUrl,
                                 ParentId = org.ParentId,
                                 ProjectCode = org.ProjectCode,
                                 IsManager = org.IsManager,
                                 MemberCount = (from ouUser in _userOrganizationUnitRepository.GetAll()
                                                where ouUser.OrganizationUnitId == org.Id
                                                select ouUser.UserId).Count(),
                                 MemberDeptCount = (from dep in _deptRepo.GetAll()
                                                    join dtou in _dtOuRepo.GetAll() on dep.Id equals dtou.DeptId into tbl_dtou
                                                    from b in tbl_dtou
                                                    join c in _deptUserRepo.GetAll() on b.DeptId equals c.DeptId into tbl_deptU
                                                    from d in tbl_deptU
                                                    where b.OrganizationUnitId == org.Id
                                                    select d.UserId).Count()

                             }).Distinct().AsQueryable();
                var result = await query.ToListAsync();
                var data = DataResult.ResultSuccess(result, "Get success", await query.CountAsync());
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "ResultFail");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
    }
}
