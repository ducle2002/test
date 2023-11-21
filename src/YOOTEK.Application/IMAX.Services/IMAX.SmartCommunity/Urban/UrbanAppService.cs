using Abp.Application.Services;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.UI;
using IMAX.Application;
using IMAX.Authorization;
using IMAX.Common.DataResult;
using IMAX.EntityDb;
using IMAX.IMAX.Services.IMAX.SmartCommunity.Building.Dto;
using IMAX.Organizations;
using IMAX.Organizations.AppOrganizationUnits;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static IMAX.Common.Enum.CommonENum;

namespace IMAX.Services
{
    public interface IUrbanAppService : IApplicationService
    {
        #region Urbans
        Task<DataResult> GetAllProjectTenantAsync();
        Task<DataResult> GetAllUrbansAsync(GetAllUrbansInput input);
        Task<DataResult> GetAllUrbansAdminAsync();
        Task<DataResult> GetDetailUrbanAsync(long id);
        Task<DataResult> CreateUrban(CreateUrbanDto input);
        Task<DataResult> UpdateUrban(UpdateUrbanDto input);
        Task<DataResult> DeleteUrban(long id);
        Task<DataResult> DeleteListUrbans([FromBody] List<long> ids);

        #endregion

        #region Project
        Task<DataResult> CreateOrUpdateTenantProject(AppOrganizationUnitInput input);
        Task<DataResult> DeleteTenantProject(long id);
        #endregion

        #region Apartment
        Task<DataResult> GetAllApartmentTenant();
        #endregion
    }

    [AbpAuthorize]
    public class UrbanAppService : IMAXAppServiceBase, IUrbanAppService
    {
        private readonly IRepository<AppOrganizationUnit, long> _organizationUnitRepository;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationUnitRepository;
        private readonly IRepository<Apartment, long> _smartHomeRepository;
        private readonly IRepository<CitizenVehicle, long> _citizenVehicleRepos;
        private readonly AppOrganizationUnitManager _organizationUnitManager;
        private readonly IRepository<Citizen, long> _citizenRepository;
        private readonly IRepository<Apartment, long> _apartmentRepository;
        public UrbanAppService(
             IRepository<AppOrganizationUnit, long> organizationUnitRepository,
             IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,
             IRepository<Apartment, long> smartHomeRepository,
             AppOrganizationUnitManager organizationUnitManager,
             IRepository<CitizenVehicle, long> citizenVehicleRepos,
             IRepository<Citizen, long> citizenRespository,
             IRepository<Apartment, long> apartmentRepository

            )
        {
            _organizationUnitRepository = organizationUnitRepository;
            _userOrganizationUnitRepository = userOrganizationUnitRepository;
            _organizationUnitManager = organizationUnitManager;
            _smartHomeRepository = smartHomeRepository;
            _citizenVehicleRepos = citizenVehicleRepos;
            _citizenRepository = citizenRespository;
            _apartmentRepository = apartmentRepository;
        }
        #region Urbans

        public async Task<DataResult> GetAllProjectTenantAsync()
        {
            try
            {
                    IQueryable<AppOrganizationUnitDto> query = (from dt in _organizationUnitRepository.GetAll()
                                 where dt.Type == APP_ORGANIZATION_TYPE.URBAN
                                 select new AppOrganizationUnitDto()
                                 {
                                     Id = dt.ParentId.Value,
                                     ImageUrl = dt.ImageUrl,
                                     DisplayName = dt.DisplayName,
                                     TenantId = dt.TenantId,
                                     ProjectCode = dt.ProjectCode,
                                     Description = dt.Description,
                                     ParentId = dt.ParentId,
                                     Type = dt.Type
                                 });

                    List<AppOrganizationUnitDto> result = await query.ToListAsync();
                    return DataResult.ResultSuccess(result, "Get success!", query.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        public async Task<DataResult> GetAllUrbansAsync(GetAllUrbansInput input)
        {
            try
            {
                // var ouUI = await _userOrganizationUnitRepository.GetAll()
                //     .Where(x => x.UserId == AbpSession.UserId)
                //     .Select(x => x.OrganizationUnitId)
                //     .ToListAsync();

                    List<AppOrganizationUnit> urbans = _organizationUnitRepository.GetAllList(x => x.Type == APP_ORGANIZATION_TYPE.URBAN && x.ParentId.HasValue);
                    if (urbans == null || urbans.Count() == 0) { return DataResult.ResultSuccess("Get success!"); }

                    List<long> urbanIds = urbans.Select(x => x.ParentId.Value).ToList();

                    List<AppOrganizationUnit> buildings = _organizationUnitRepository.GetAllList(x => x.Type == APP_ORGANIZATION_TYPE.BUILDING && x.ParentId.HasValue);

                    List<long> buildingIds = buildings.Select(x => x.ParentId.Value).ToList();

                    IQueryable<UrbanGetAllDto> query = (from dt in _organizationUnitRepository.GetAll()
                                 join bd in _organizationUnitRepository.GetAll()
                                 on dt.Id equals bd.ParentId into dt_bd
                                 from bd in dt_bd.DefaultIfEmpty()
                                 join ap in _apartmentRepository.GetAll() on bd.Id equals ap.BuildingId into bd_ap
                                 from ap in bd_ap.DefaultIfEmpty()
                                 join cz in _citizenRepository.GetAll() on ap.ApartmentCode equals cz.ApartmentCode into ap_cz
                                 from cz in ap_cz.DefaultIfEmpty()
                                 group cz by new { dt.Id, dt.ImageUrl, dt.DisplayName, dt.Address, dt.PhoneNumber, dt.Email, dt.ProjectCode } into g
                                 select new UrbanGetAllDto()
                                 {
                                     Id = g.Key.Id,
                                     ImageUrl = g.Key.ImageUrl,
                                     DisplayName = g.Key.DisplayName,
                                     ProjectCode = g.Key.ProjectCode,
                                     Address = g.Key.Address,
                                     Email = g.Key.Email,
                                     PhoneNumber = g.Key.PhoneNumber,
                                     NumberCitizen = g.Count(x => x != null),
                                     NumberBuilding = (from b in _organizationUnitRepository.GetAll()
                                                       where buildingIds.Contains(b.Id) && b.ParentId == g.Key.Id
                                                       select b).Count()
                                 })
                                 .Where(x => urbanIds.Contains(x.Id))
                                 .ApplySearchFilter(input.Keyword, x => x.DisplayName, x => x.ProjectCode)
                                 .WhereIf(input.Id > 0, x => x.Id == input.Id);

                    var result = await query
                                        .ApplySort(input.OrderBy, input.SortBy)
                                        .ApplySort(OrderByUrban.PROJECT_CODE)
                                        .PageBy(input).ToListAsync();
                    await CurrentUnitOfWork.SaveChangesAsync();
                    return DataResult.ResultSuccess(result, "Get success!", query.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        public async Task<DataResult> GetAllUrbansAdminAsync()
        {
            try
            {
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
                {
                    List<AppOrganizationUnit> urbans = _organizationUnitRepository.GetAllList(x => x.Type == APP_ORGANIZATION_TYPE.URBAN && x.ParentId.HasValue);
                    if (urbans == null || urbans.Count() == 0) { return DataResult.ResultSuccess("Get success!"); }

                    List<long> urbanIds = urbans.Select(x => x.ParentId.Value).ToList();
                    List<UrbanGetAllDto> result = await (from u in _organizationUnitRepository.GetAll()
                                                         select new UrbanGetAllDto()
                                                         {
                                                             Id = u.Id,
                                                             DisplayName = u.DisplayName
                                                         })
                                                .Where(x => urbanIds.Contains(x.Id))
                                                .ToListAsync();
                    return DataResult.ResultSuccess(result, "Get success!");
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }
        public async Task<DataResult> GetDetailUrbanAsync(long id)
        {
            try
            {

                List<AppOrganizationUnit> buildings = _organizationUnitRepository.GetAllList(x => x.Type == APP_ORGANIZATION_TYPE.BUILDING && x.ParentId.HasValue);

                List<long> buildingIds = buildings.Select(x => x.ParentId.Value).ToList();

                UrbanDto urban = await (from dt in _organizationUnitRepository.GetAll()
                                   where dt.Id == id
                                   join bd in _organizationUnitRepository.GetAll()
                                   on dt.Id equals bd.ParentId into dt_bd
                                   from bd in dt_bd.DefaultIfEmpty()
                                   join ap in _apartmentRepository.GetAll() on bd.Id equals ap.BuildingId into bd_ap
                                   from ap in bd_ap.DefaultIfEmpty()
                                   join cz in _citizenRepository.GetAll() on ap.ApartmentCode equals cz.ApartmentCode into ap_cz
                                   from cz in ap_cz.DefaultIfEmpty()
                                   group cz by new { dt.Id, dt.ImageUrl, dt.DisplayName, dt.Address, dt.PhoneNumber, dt.Email, dt.TenantId, dt.ProjectCode, dt.Description, dt.ParentId, dt.Type, dt.Code, dt.ProvinceCode, dt.DistrictCode, dt.WardCode } into g
                                   select new UrbanDto()
                                   {
                                       Id = g.Key.Id,
                                       ImageUrl = g.Key.ImageUrl,
                                       DisplayName = g.Key.DisplayName,
                                       TenantId = g.Key.TenantId,
                                       ProjectCode = g.Key.ProjectCode,
                                       Description = g.Key.Description,
                                       ParentId = g.Key.ParentId,
                                       Type = g.Key.Type,
                                       Code = g.Key.Code,
                                       ProvinceCode = g.Key.ProvinceCode,
                                       DistrictCode = g.Key.DistrictCode,
                                       WardCode = g.Key.WardCode,
                                       Address = g.Key.Address,
                                       Email = g.Key.Email,
                                       PhoneNumber = g.Key.PhoneNumber,
                                       NumberCitizen = g.Count(x => x != null),
                                       NumberBuilding = (from b in _organizationUnitRepository.GetAll()
                                                         where buildingIds.Contains(b.Id) && b.ParentId == g.Key.Id
                                                         select b).Count()
                                   }).FirstOrDefaultAsync();
                return DataResult.ResultSuccess(urban.MapTo<UrbanDto>(), "Get urban detail success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        public async Task<DataResult> CreateUrban(CreateUrbanDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                AppOrganizationUnit insertInput = input.MapTo<AppOrganizationUnit>();
                insertInput.TenantId = AbpSession.TenantId;
                insertInput.Type = (int)APP_ORGANIZATION_TYPE.REPRESENTATIVE_NAME;
                insertInput.Code = await _organizationUnitManager.GetNextChildCodeAsync(insertInput.ParentId);
                long id = await _organizationUnitRepository.InsertAndGetIdAsync(insertInput);

                AppOrganizationUnit organizationUnit = new AppOrganizationUnit(AbpSession.TenantId, input.DisplayName, id, APP_ORGANIZATION_TYPE.URBAN);
                organizationUnit.ImageUrl = input.ImageUrl;
                organizationUnit.Description = input.Description;
                organizationUnit.ProjectCode = input.ProjectCode;

                //organizationUnit.ProvinceCode = input.ProvinceCode;
                //organizationUnit.DistrictCode = input.DistrictCode;
                //organizationUnit.WardCode = input.WardCode;
                //organizationUnit.Address = input.Address;
                //organizationUnit.PhoneNumber = input.PhoneNumber;
                //organizationUnit.Email = input.Email;
                await _organizationUnitManager.CreateAsync(organizationUnit);
                await CurrentUnitOfWork.SaveChangesAsync();
                mb.statisticMetris(t1, 0, "is_urban");
                return DataResult.ResultSuccess("Insert success !");
                
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        public async Task<DataResult> UpdateUrban(UpdateUrbanDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                //update
                AppOrganizationUnit updateData = await _organizationUnitRepository.GetAsync(input.Id);
                if (updateData != null)
                {
                    input.MapTo(updateData);
                    updateData.TenantId = AbpSession.TenantId;
                    updateData.Type = APP_ORGANIZATION_TYPE.REPRESENTATIVE_NAME;
                    await _organizationUnitRepository.UpdateAsync(updateData);

                    AppOrganizationUnit parent = await _organizationUnitRepository.FirstOrDefaultAsync(x => x.ParentId == updateData.Id && x.Type == APP_ORGANIZATION_TYPE.URBAN);
                    parent.DisplayName = input.DisplayName;
                    parent.ImageUrl = input.ImageUrl;
                    parent.Description = input.Description;
                    parent.ProjectCode = input.ProjectCode;

                    //parent.ProvinceCode = input.ProvinceCode;
                    //parent.DistrictCode = input.DistrictCode;
                    //parent.WardCode = input.WardCode;
                    //parent.Address = input.Address;
                    //parent.PhoneNumber = input.PhoneNumber;
                    //parent.Email = input.Email;
                    await _organizationUnitRepository.UpdateAsync(parent);
                }
                mb.statisticMetris(t1, 0, "Ud_urban");

                return DataResult.ResultSuccess("Update success !");

            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        public async Task<object> GetAllUrbanByUserAsync()
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var ouIds = UserManager.GetAccessibleOperationDepartmentIds();
                    var buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
                    var codes = _organizationUnitRepository.GetAllList(x => ouIds.Contains(x.Id) || buIds.Contains(x.Id)).Select(x => x.Code.Split('.')[0]).Distinct().ToList();                   
                    var urbans = await (from dt in _organizationUnitRepository.GetAll()
                                        join dtc in _organizationUnitRepository.GetAll() on dt.ParentId.Value equals dtc.Id into tb_dtc
                                        from dtc in tb_dtc.DefaultIfEmpty()
                                 where dt.Type == APP_ORGANIZATION_TYPE.URBAN
                                 select new AppOrganizationUnitDto()
                                 {
                                     Id = dt.ParentId.Value,
                                     ImageUrl = dt.ImageUrl,
                                     DisplayName = dt.DisplayName,
                                     TenantId = dt.TenantId,
                                     ProjectCode = dt.ProjectCode,
                                     Description = dt.Description,
                                     ParentId = dt.ParentId,
                                     Type = dt.Type,
                                     Code = dtc.Code
                                 })
                                 .WhereIf(!IsGranted(PermissionNames.Data_Admin), x => codes.Contains(x.Code)).ToListAsync();
                    var data = DataResult.ResultSuccess(urbans, "Get success!");
                    return data;
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }
        public async Task<object> CreateOrUpdateUrban(AppOrganizationUnitInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    long t1 = TimeUtils.GetNanoseconds();
                    input.TenantId = AbpSession.TenantId;

                    if (input.Id > 0)
                    {
                        //update
                        var updateData = await _organizationUnitRepository.GetAsync(input.Id);
                        if (updateData != null)
                        {
                            input.Code = updateData.Code;
                            input.ParentId = updateData.ParentId;
                            input.MapTo(updateData);
                            updateData.Type = APP_ORGANIZATION_TYPE.REPRESENTATIVE_NAME;
                            await _organizationUnitRepository.UpdateAsync(updateData);
                            var parent = await _organizationUnitRepository.FirstOrDefaultAsync(x => x.ParentId == updateData.Id && x.Type == APP_ORGANIZATION_TYPE.URBAN);
                            parent.DisplayName = input.DisplayName;
                            parent.ImageUrl = input.ImageUrl;
                            parent.Description = input.Description;
                            parent.ProjectCode = input.ProjectCode;

                            await _organizationUnitRepository.UpdateAsync(parent);
                        }
                        mb.statisticMetris(t1, 0, "Ud_urban");

                        var data = DataResult.ResultSuccess("Update success !");
                        return data;
                    }
                    else
                    {
                        var insertInput = input.MapTo<AppOrganizationUnit>();
                        insertInput.Type = (int)APP_ORGANIZATION_TYPE.REPRESENTATIVE_NAME;
                        insertInput.Code = await _organizationUnitManager.GetNextChildCodeAsync(insertInput.ParentId);
                        long id = await _organizationUnitRepository.InsertAndGetIdAsync(insertInput);
                        insertInput.Id = id;

                        var organizationUnit = new AppOrganizationUnit(AbpSession.TenantId, input.DisplayName, id, APP_ORGANIZATION_TYPE.URBAN);
                        organizationUnit.ImageUrl = input.ImageUrl;
                        organizationUnit.Description = input.Description;
                        organizationUnit.ProjectCode = input.ProjectCode;
                        await _organizationUnitManager.CreateAsync(organizationUnit);
                        await CurrentUnitOfWork.SaveChangesAsync();
                        mb.statisticMetris(t1, 0, "is_urban");
                        var data = DataResult.ResultSuccess("Insert success !");
                        return data;
                    }
                }

            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        public async Task<DataResult> DeleteUrban(long id)
        {
            try
            {
                using(CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    await _organizationUnitManager.DeleteAsync(id);
                    return DataResult.ResultSuccess("Delete success!");
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                return data;
            }
        }

        public async Task<DataResult> DeleteListUrbans([FromBody] List<long> ids)
        {
            try
            {

                if (ids.Count == 0) return DataResult.ResultError("Err", "input empty");
                var tasks = new List<Task>();
                foreach (var id in ids)
                {
                   await  DeleteUrban(id);
                }
               
                var data = DataResult.ResultSuccess("Delete success!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                throw e;
            }
        }
        #endregion

        #region building
        public async Task<object> GetAllBuildingTenantAsync()
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var query = (from tp in _organizationUnitRepository.GetAll()
                                 join ub in _organizationUnitRepository.GetAll() on tp.ParentId.Value equals ub.Id into tbl_ub
                                 from ub in tbl_ub.DefaultIfEmpty()
                                 where tp.Type == APP_ORGANIZATION_TYPE.BUILDING
                                 select new AppOrganizationUnitDto()
                                 {
                                     Type = tp.Type,
                                     CreationTime = tp.CreationTime,
                                     CreatorUserId = tp.CreatorUserId,
                                     Id = tp.ParentId.Value,
                                     Description = tp.Description,
                                     ImageUrl = tp.ImageUrl,
                                     DisplayName = tp.DisplayName,
                                     ParentId = tp.ParentId,
                                     ProjectCode = tp.ProjectCode,
                                     TenantId = tp.TenantId,
                                     IsManager = tp.IsManager,
                                     Code = tp.Code,
                                     UrbanId = ub.ParentId
                                 });

                    var result = await query.ToListAsync();
                    var data = DataResult.ResultSuccess(result, "Get success!", query.Count());
                    return data;
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }

        }


        public async Task<object> GetAllBuildingsAsync(GetAllBuildingsInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {

                    var buildings = _organizationUnitRepository.GetAllList(x => x.Type == APP_ORGANIZATION_TYPE.BUILDING && x.ParentId.HasValue);
                    if (buildings == null || buildings.Count() == 0) { return DataResult.ResultSuccess("Get success!"); }

                    var buildingIds = buildings.Select(x => x.ParentId.Value).ToList();

                    var query = (from tp in _organizationUnitRepository.GetAll()
                                 join ub in _organizationUnitRepository.GetAll() on tp.ParentId equals ub.Id into tb_ub
                                 from ub in tb_ub.DefaultIfEmpty()
                                 select new BuidingDto()
                                 {
                                     Type = tp.Type,
                                     Id = tp.Id,
                                     Description = tp.Description,
                                     ImageUrl = tp.ImageUrl,
                                     DisplayName = tp.DisplayName,
                                     ParentId = tp.ParentId,
                                     ProjectCode = tp.ProjectCode,
                                     TenantId = tp.TenantId,
                                     IsManager = tp.IsManager,
                                     Code = tp.Code,
                                     UrbanName = ub != null ? ub.DisplayName : null,
                                     UrbanId = ub != null ? ub.Id : null,

                                 })
                                .Where(x => buildingIds.Contains(x.Id))
                                 .WhereIf(input.Keyword != null, x => (x.DisplayName != null && x.DisplayName.ToLower().Contains(input.Keyword.ToLower())) || (x.ProjectCode != null && x.ProjectCode.ToLower() == input.Keyword.ToLower()));
                    //.WhereIf(input.ParentId.HasValue, x => x.ParentId == input.ParentId.Value);

                    var result = await query.PageBy(input).ToListAsync();
                    var data = DataResult.ResultSuccess(result, "Get success!", query.Count());
                    return data;
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }

        }


        public async Task<object> CreateOrUpdateBuilding(AppOrganizationUnitInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    long t1 = TimeUtils.GetNanoseconds();
                    input.TenantId = AbpSession.TenantId;
                    input.Type = 0;
                    if (input.Id > 0)
                    {
                        //update
                        var updateData = await _organizationUnitRepository.GetAsync(input.Id);
                        if (updateData != null)
                        {
                            input.Code = updateData.Code;
                            //input.ParentId = updateData.ParentId;
                            input.MapTo(updateData);
                            //call back
                            await _organizationUnitRepository.UpdateAsync(updateData);

                            var child = await _organizationUnitRepository.FirstOrDefaultAsync(x => x.ParentId == updateData.Id && x.Type == APP_ORGANIZATION_TYPE.BUILDING);
                            child.DisplayName = input.DisplayName;
                            child.ImageUrl = input.ImageUrl;
                            child.Description = input.Description;
                            child.ProjectCode = input.ProjectCode;

                            await _organizationUnitRepository.UpdateAsync(child);

                            await CurrentUnitOfWork.SaveChangesAsync();
                        }
                        mb.statisticMetris(t1, 0, "Ud_building");

                        var data = DataResult.ResultSuccess("Update success !");
                        return data;
                    }
                    else
                    {
                        var insertInput = input.MapTo<AppOrganizationUnit>();

                        insertInput.Type = (int)APP_ORGANIZATION_TYPE.REPRESENTATIVE_NAME;
                        insertInput.Code = await _organizationUnitManager.GetNextChildCodeAsync(insertInput.ParentId);
                        long id = await _organizationUnitRepository.InsertAndGetIdAsync(insertInput);
                        insertInput.Id = id;
                        var organizationUnit = new AppOrganizationUnit(AbpSession.TenantId, input.DisplayName, id, APP_ORGANIZATION_TYPE.BUILDING);
                        organizationUnit.ImageUrl = input.ImageUrl;
                        organizationUnit.Description = input.Description;
                        organizationUnit.ProjectCode = input.ProjectCode;
                        await _organizationUnitManager.CreateAsync(organizationUnit);
                        mb.statisticMetris(t1, 0, "is_building");
                        var data = DataResult.ResultSuccess("Insert success !");
                        return data;
                    }
                }

            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }
        public async Task<DataResult> DeleteBuilding(long id)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    await _organizationUnitManager.DeleteAsync(id);
                    var data = DataResult.ResultSuccess("Delete success!");
                    return data;
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                return data;
            }
        }

        public Task<DataResult> DeleteListBuildings([FromBody] List<long> ids)
        {
            try
            {

                if (ids.Count == 0) return Task.FromResult(DataResult.ResultError("Err", "input empty"));
                var tasks = new List<Task>();
                foreach (var id in ids)
                {
                    var tk = DeleteBuilding(id);
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

        #endregion

        #region Project

        public async Task<DataResult> CreateOrUpdateTenantProject(AppOrganizationUnitInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                input.TenantId = AbpSession.TenantId;
                input.Code = "00001";
                if (input.Id > 0)
                {
                    //update
                    AppOrganizationUnit updateData = await _organizationUnitRepository.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        //call back
                        await _organizationUnitRepository.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "Ud_tenantpro");

                    return DataResult.ResultSuccess(updateData, "Update success !");
                }
                else
                {
                    AppOrganizationUnit insertInput = input.MapTo<AppOrganizationUnit>();
                    long id = await _organizationUnitRepository.InsertAndGetIdAsync(insertInput);
                    insertInput.Id = id;
                    mb.statisticMetris(t1, 0, "is_tenantpro");
                    return DataResult.ResultSuccess(insertInput, "Insert success !");
                }

            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        public async Task<DataResult> DeleteTenantProject(long id)
        {
            try
            {
                await _organizationUnitRepository.DeleteAsync(id);
                return DataResult.ResultSuccess("Delete success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        #endregion

        #region Apartment

        public async Task<DataResult> GetAllApartmentTenant()
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var query = (from sm in _smartHomeRepository.GetAll()
                                 where (sm.ApartmentCode != null)
                                 select new
                                 {
                                     Id = sm.Id,
                                     ApartmentCode = sm.ApartmentCode,
                                     Name = sm.Name,
                                     BuildingId = sm.BuildingId,
                                     ApartmentAreas = sm.Area,
                                     UrbanId = sm.UrbanId,
                                     Vehicles = (from vh in _citizenVehicleRepos.GetAll()
                                                 where vh.ApartmentCode == sm.ApartmentCode && vh.State == CitizenVehicleState.ACCEPTED
                                                 select new
                                                 {
                                                     VehicleName = vh.VehicleName,
                                                     VehicleType = vh.VehicleType,
                                                     VehicleCode = vh.VehicleCode,
                                                     ApartmentCode = vh.ApartmentCode,
                                                     ParkingId = vh.ParkingId,
                                                 }).ToList()
                                 })
                             .AsQueryable();
                    query = query.OrderBy(x => x.ApartmentCode);

                    var result = await query.ToListAsync();

                    return DataResult.ResultSuccess(result, "Get success!", query.Count());
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        #endregion
    }
}