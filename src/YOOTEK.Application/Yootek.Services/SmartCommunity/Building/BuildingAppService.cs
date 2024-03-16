using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Application;
using Yootek.Authorization;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Yootek.Services.Yootek.SmartCommunity.Building.Dto;
using Yootek.Organizations;
using Yootek.Organizations.AppOrganizationUnits;
using Yootek.QueriesExtension;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yootek.Yootek.Services.Yootek.SmartCommunity.Building
{
    public interface IBuildingAppService : IApplicationService
    {
        Task<DataResult> GetAllBuildingTenantAsync();
        Task<DataResult> GetAllBuildingsAsync(GetAllBuildingsInput input);
        Task<DataResult> GetAllBuildingByUrban(long id);
        Task<DataResult> GetBuildingDetailAsync(long id);
        Task<DataResult> GetBuildingByIdNotAuthAsync(long id);
        Task<DataResult> GetAllBuildingAdminAsync(long? urbanId);
        Task<DataResult> CreateBuilding(CreateBuildingDto input);
        Task<DataResult> UpdateBuilding(UpdateBuildingDto input);
        Task<DataResult> DeleteBuilding(long id);
        Task<DataResult> DeleteListBuildings([FromBody] List<long> ids);
    }
    [AbpAuthorize]
    public class BuildingAppService : YootekAppServiceBase, IBuildingAppService
    {
        private readonly IRepository<AppOrganizationUnit, long> _organizationUnitRepository;
        private readonly AppOrganizationUnitManager _organizationUnitManager;
        private readonly IRepository<Citizen, long> _citizenRepository;
        private readonly IRepository<Floor, long> _floorRepositoy;
        private readonly IRepository<Apartment, long> _apartmentRepository;

        public BuildingAppService(
              IRepository<AppOrganizationUnit, long> organizationUnitRepository,
              AppOrganizationUnitManager organizationUnitManager,
              IRepository<Citizen, long> citizenRespository,
              IRepository<Floor, long> floorRepository,
              IRepository<Apartment, long> apartmentRepository
        )
        {
            _organizationUnitRepository = organizationUnitRepository;
            _organizationUnitManager = organizationUnitManager;
            _citizenRepository = citizenRespository;
            _floorRepositoy = floorRepository;
            _apartmentRepository = apartmentRepository;
        }
        public async Task<DataResult> GetAllBuildingTenantAsync()
        {
            try
            {
                IQueryable<AppOrganizationUnitDto> query = (from tp in _organizationUnitRepository.GetAll()
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

                List<AppOrganizationUnitDto> result = await query.ToListAsync();
                return DataResult.ResultSuccess(result, "Get success!", query.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }

        }
        public async Task<DataResult> GetAllBuildingByUrban(long urbanId)
        {
            try
            {
                List<AppOrganizationUnit> buildings = _organizationUnitRepository.GetAllList(x => x.Type == APP_ORGANIZATION_TYPE.BUILDING && x.ParentId.HasValue);
                if (buildings == null || buildings.Count() == 0) { return DataResult.ResultSuccess("Get success!"); }

                List<long> buildingIds = buildings.Select(x => x.ParentId.Value).ToList();

                IQueryable<BuidingDto> query = (from bd in _organizationUnitRepository.GetAll()
                                                where bd.ParentId == urbanId && buildingIds.Contains(bd.Id)
                                                select new BuidingDto()
                                                {
                                                    Type = bd.Type,
                                                    Id = bd.Id,
                                                    Description = bd.Description,
                                                    ImageUrl = bd.ImageUrl,
                                                    DisplayName = bd.DisplayName,
                                                    ParentId = bd.ParentId,
                                                    ProjectCode = bd.ProjectCode,
                                                    PhoneNumber = bd.PhoneNumber,
                                                    Email = bd.Email,

                                                    Address = bd.Address,
                                                    Area = bd.Area,
                                                    ProvinceCode = bd.ProvinceCode,
                                                    DistrictCode = bd.DistrictCode,
                                                    WardCode = bd.WardCode,
                                                    BuildingType = bd.BuildingType,
                                                    NumberFloor = bd.NumberFloor,

                                                    TenantId = bd.TenantId,
                                                    IsManager = bd.IsManager,
                                                    Code = bd.Code

                                                });
                List<BuidingDto> result = await query.ToListAsync();
                return DataResult.ResultSuccess(result, "Get success!");
            }

            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> GetAllBuildingByUser(long? urbanId)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var buildings = new List<BuildingByUserDto>();
                    List<long> buildingIds = new List<long>();
                    var query = (from dt in _organizationUnitRepository.GetAll()
                                 select new BuildingByUserDto()
                                 {
                                     Id = dt.Id,
                                     ImageUrl = dt.ImageUrl,
                                     DisplayName = dt.DisplayName,
                                     TenantId = dt.TenantId,
                                     ProjectCode = dt.ProjectCode,
                                     Description = dt.Description,
                                     ParentId = dt.ParentId,
                                     Type = dt.Type,
                                     Code = dt.Code,
                                     UrbanId = dt.ParentId,
                                 }).AsQueryable();
                    if (urbanId.HasValue)
                    {
                        buildingIds = _organizationUnitRepository.GetAll().Where(x => x.Type == APP_ORGANIZATION_TYPE.BUILDING && x.ParentId.HasValue).Select(x => x.ParentId.Value).ToList();
                        if (buildingIds == null || buildingIds.Count() == 0) { return DataResult.ResultSuccess("Get success!"); }
                        var ub = _organizationUnitRepository.FirstOrDefault(x => x.Id == urbanId);
                        buildings = await query.Where(x => x.ParentId == urbanId && buildingIds.Contains(x.Id)).ToListAsync();
                        foreach (var x in buildings)
                        {
                            if (ub != null)
                            {
                                x.Urban = ub.MapTo<BuildingByUserDto>();
                            }
                        };

                    }
                    else
                    {
                        buildingIds = UserManager.GetAccessibleBuildingOrUrbanIds();
                        buildings = await query.Where(x => x.ParentId.HasValue && x.Type == APP_ORGANIZATION_TYPE.BUILDING).ToListAsync();
                        if (buildings != null && buildings.Count > 0)
                        {
                            foreach (var x in buildings)
                            {
                                x.Id = x.ParentId.Value;
                                var code = x.Code.Split('.')[0];
                                var ub = _organizationUnitRepository.GetAll().Where(x => x.Type == APP_ORGANIZATION_TYPE.URBAN && x.Code.StartsWith(code)).FirstOrDefault();
                                if (ub != null)
                                {

                                    x.Urban = ub.MapTo<BuildingByUserDto>();
                                    x.Urban.Id = x.Urban.ParentId.Value;
                                    x.UrbanId = x.Urban.ParentId.Value;

                                }

                            }
                        }
                    }

                    return DataResult.ResultSuccess(buildings, "Get success!", buildings.Count());
                }
            }

            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> GetBuildingDetailAsync(long id)
        {
            try
            {

                List<AppOrganizationUnit> buildings = _organizationUnitRepository.GetAllList(x => x.Type == APP_ORGANIZATION_TYPE.BUILDING && x.ParentId.HasValue);
                if (buildings == null || buildings.Count() == 0) { return DataResult.ResultSuccess("Get success!"); }

                List<long> buildingIds = buildings.Select(x => x.ParentId.Value).ToList();

                IQueryable<BuidingDto> query = (from tp in _organizationUnitRepository.GetAll()
                                                join ub in _organizationUnitRepository.GetAll() on tp.ParentId equals ub.Id into tb_ub
                                                from ub in tb_ub.DefaultIfEmpty()
                                                join ap in _apartmentRepository.GetAll() on tp.Id equals ap.BuildingId into tb_ap
                                                from ap in tb_ap.DefaultIfEmpty()
                                                join cz in _citizenRepository.GetAll() on ap.ApartmentCode equals cz.ApartmentCode into tb_cz
                                                from cz in tb_cz.DefaultIfEmpty()
                                                group new { ap, cz } by new
                                                {
                                                    tp.Type,
                                                    tp.Id,
                                                    tp.Description,
                                                    tp.ImageUrl,
                                                    tp.DisplayName,
                                                    tp.ParentId,
                                                    tp.ProjectCode,
                                                    tp.PhoneNumber,
                                                    tp.Email,
                                                    tp.Area,
                                                    tp.Address,
                                                    tp.ProvinceCode,
                                                    tp.DistrictCode,
                                                    tp.WardCode,
                                                    tp.BuildingType,
                                                    tp.NumberFloor,
                                                    tp.TenantId,
                                                    tp.IsManager,
                                                    tp.Code,
                                                    UrbanId = ub != null ? (long?)ub.Id : null,
                                                    UrbanName = ub != null ? ub.DisplayName : null
                                                } into g
                                                select new BuidingDto()
                                                {
                                                    Type = g.Key.Type,
                                                    Id = g.Key.Id,
                                                    Description = g.Key.Description,
                                                    ImageUrl = g.Key.ImageUrl,
                                                    DisplayName = g.Key.DisplayName,
                                                    ParentId = g.Key.ParentId,
                                                    ProjectCode = g.Key.ProjectCode,
                                                    PhoneNumber = g.Key.PhoneNumber,
                                                    Email = g.Key.Email,

                                                    Address = g.Key.Address,
                                                    Area = g.Key.Area,
                                                    ProvinceCode = g.Key.ProvinceCode,
                                                    DistrictCode = g.Key.DistrictCode,
                                                    WardCode = g.Key.WardCode,
                                                    BuildingType = g.Key.BuildingType,
                                                    NumberFloor = g.Key.NumberFloor,

                                                    TenantId = g.Key.TenantId,
                                                    IsManager = g.Key.IsManager,
                                                    Code = g.Key.Code,
                                                    NumberApartment = g.Count(x => x.ap != null),
                                                    NumberCitizen = g.Count(x => x.cz != null),

                                                    UrbanId = g.Key.UrbanId,
                                                    UrbanName = g.Key.UrbanName

                                                })
                            .Where(x => buildingIds.Contains(x.Id) && x.Id == id);

                BuidingDto result = await query.FirstOrDefaultAsync();

                return DataResult.ResultSuccess(result, "Get success!", query.Count());

            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        [AbpAllowAnonymous]
        public async Task<DataResult> GetBuildingByIdNotAuthAsync(long id)
        {
            try
            {
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
                {
                    IQueryable<BuildingBasicDto> query = from building in _organizationUnitRepository.GetAll()
                                                         join urban in _organizationUnitRepository.GetAll()
                                                         on building.ParentId equals urban.Id into table_temp
                                                         from organizationUnit in table_temp.DefaultIfEmpty()
                                                         where building.Id == id
                                                         select new BuildingBasicDto
                                                         {
                                                             Id = building.Id,
                                                             Code = building.ProjectCode,
                                                             Name = building.DisplayName,
                                                             ImageUrl = building.ImageUrl,
                                                             UrbanId = organizationUnit.Id,
                                                             UrbanCode = organizationUnit.ProjectCode,
                                                             UrbanImageUrl = organizationUnit.ImageUrl,
                                                             UrbanName = organizationUnit.DisplayName,
                                                         };
                    return DataResult.ResultSuccess(query.FirstOrDefault(), "Get success!");
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> GetAllBuildingAdminAsync(long? urbanId)
        {
            try
            {
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
                {
                    List<AppOrganizationUnit> buildings = _organizationUnitRepository.GetAllList(x => x.Type == APP_ORGANIZATION_TYPE.BUILDING && x.ParentId.HasValue);
                    if (buildings == null || buildings.Count() == 0) { return DataResult.ResultSuccess("Get success!"); }

                    List<long> buildingIds = buildings.Select(x => x.ParentId.Value).ToList();

                    IQueryable<BuidingDto> query = (from bd in _organizationUnitRepository.GetAll()
                                                    where buildingIds.Contains(bd.Id) && bd.ParentId == urbanId
                                                    select new BuidingDto()
                                                    {
                                                        Id = bd.Id,
                                                        DisplayName = bd.DisplayName,
                                                    });
                    List<BuidingDto> result = await query.ToListAsync();
                    return DataResult.ResultSuccess(result, "Get success!");
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> GetAllBuildingsAsync(GetAllBuildingsInput input)
        {
            try
            {
                List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
                List<AppOrganizationUnit> buildings = _organizationUnitRepository.GetAllList(x => x.Type == APP_ORGANIZATION_TYPE.BUILDING && x.ParentId.HasValue);
                if (buildings == null || buildings.Count() == 0) { return DataResult.ResultSuccess("Get success!"); }

                List<long> buildingIds = buildings.Select(x => x.ParentId.Value).ToList();

                IQueryable<BuidingAllDto> query = (from tp in _organizationUnitRepository.GetAll()
                                                   join ub in _organizationUnitRepository.GetAll() on tp.ParentId equals ub.Id into tb_ub
                                                   from ub in tb_ub.DefaultIfEmpty()
                                                   join ap in _apartmentRepository.GetAll() on tp.Id equals ap.BuildingId into tb_ap
                                                   from ap in tb_ap.DefaultIfEmpty()
                                                   join cz in _citizenRepository.GetAll() on ap.ApartmentCode equals cz.ApartmentCode into tb_cz
                                                   from cz in tb_cz.DefaultIfEmpty()
                                                   group new { ap, cz } by new
                                                   {
                                                       tp.Id,
                                                       tp.ImageUrl,
                                                       tp.DisplayName,
                                                       tp.ProjectCode,
                                                       tp.PhoneNumber,
                                                       tp.Email,
                                                       tp.Area,
                                                       tp.Address,
                                                       tp.ProvinceCode,
                                                       tp.DistrictCode,
                                                       tp.WardCode,
                                                       tp.NumberFloor,
                                                       UrbanId = ub != null ? (long?)ub.Id : null,
                                                       UrbanName = ub != null ? ub.DisplayName : null
                                                   } into g
                                                   select new BuidingAllDto()
                                                   {
                                                       Id = g.Key.Id,
                                                       ImageUrl = g.Key.ImageUrl,
                                                       DisplayName = g.Key.DisplayName,
                                                       ProjectCode = g.Key.ProjectCode,
                                                       PhoneNumber = g.Key.PhoneNumber,
                                                       Email = g.Key.Email,
                                                       Address = g.Key.Address,
                                                       Area = g.Key.Area,
                                                       ProvinceCode = g.Key.ProvinceCode,
                                                       DistrictCode = g.Key.DistrictCode,
                                                       WardCode = g.Key.WardCode,
                                                       NumberFloor = g.Key.NumberFloor,
                                                       NumberApartment = g.Count(x => x.ap != null),
                                                       NumberCitizen = g.Count(x => x.cz != null),
                                                       UrbanId = g.Key.UrbanId,
                                                       UrbanName = g.Key.UrbanName,
                                                       BuildingId = g.Key.Id
                                                   })
                            .Where(x => buildingIds.Contains(x.Id))
                            .ApplySearchFilter(input.Keyword, x => x.DisplayName, x => x.ProjectCode)
                            .WhereByBuildingOrUrbanIf(!IsGranted(IOCPermissionNames.Data_Admin), buIds)
                            .WhereIf(input.UrbanId != null, x => x.UrbanId == input.UrbanId);

                var result = await query
                                     .ApplySort(input.OrderBy, input.SortBy)
                                    .ApplySort(OrderByBuilding.PROJECT_CODE)
                                    .PageBy(input).ToListAsync();
                return DataResult.ResultSuccess(result, "Get success!", query.Count());

            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }

        }
        public async Task<DataResult> CreateBuilding(CreateBuildingDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                AppOrganizationUnit insertInput = input.MapTo<AppOrganizationUnit>();
                insertInput.TenantId = AbpSession.TenantId;
                insertInput.Type = (int)APP_ORGANIZATION_TYPE.REPRESENTATIVE_NAME;
                insertInput.Code = await _organizationUnitManager.GetNextChildCodeAsync(insertInput.ParentId);
                long id = await _organizationUnitRepository.InsertAndGetIdAsync(insertInput);

                input.ParentId = id;
                AppOrganizationUnit organizationUnit = input.MapTo<AppOrganizationUnit>();
                organizationUnit.TenantId = AbpSession.TenantId;
                organizationUnit.Type = APP_ORGANIZATION_TYPE.BUILDING;
                await _organizationUnitManager.CreateAsync(organizationUnit);
                mb.statisticMetris(t1, 0, "is_building");
                return DataResult.ResultSuccess("Insert success !");

            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> UpdateBuilding(UpdateBuildingDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                //update
                AppOrganizationUnit updateData = await _organizationUnitRepository.GetAsync(input.Id);
                if (updateData != null)
                {
                    ObjectMapper.Map(input, updateData);
                    updateData.TenantId = AbpSession.TenantId;
                    updateData.Type = APP_ORGANIZATION_TYPE.REPRESENTATIVE_NAME;
                    //call back
                    await _organizationUnitRepository.UpdateAsync(updateData);

                    AppOrganizationUnit child = await _organizationUnitRepository.FirstOrDefaultAsync(x => x.ParentId == updateData.Id && x.Type == APP_ORGANIZATION_TYPE.BUILDING);
                    input.ParentId = child.ParentId;
                    input.Id = child.Id;
                    ObjectMapper.Map(input, child);
                    child.TenantId = AbpSession.TenantId;
                    await _organizationUnitRepository.UpdateAsync(child);

                    await CurrentUnitOfWork.SaveChangesAsync();
                }
                mb.statisticMetris(t1, 0, "Ud_building");

                return DataResult.ResultSuccess("Update success !");


            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> DeleteBuilding(long id)
        {
            try
            {
                await _organizationUnitManager.DeleteAsync(id);
                return DataResult.ResultSuccess("Delete success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public Task<DataResult> DeleteListBuildings([FromBody] List<long> ids)
        {
            try
            {

                if (ids.Count == 0) return Task.FromResult(DataResult.ResultError("Err", "input empty"));
                List<Task> tasks = new List<Task>();
                foreach (var id in ids)
                {
                    Task<DataResult> tk = DeleteBuilding(id);
                    tasks.Add(tk);
                }
                Task.WaitAll(tasks.ToArray());

                return Task.FromResult(DataResult.ResultSuccess("Delete success!"));
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
    }
}
