using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using AutoMapper;
using Yootek.Application;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Organizations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yootek.Services
{
    public interface IAdminFloorAppService : IApplicationService
    {
        Task<DataResult> GetAllFloorAsync(GetAllFloorInput input);
        Task<DataResult> CreateFloor(CreateFloorInput input);
        Task<DataResult> UpdateFloor(UpdateFloorInput input);
        Task<DataResult> DeleteFloor(long id);
        Task<DataResult> DeleteManyFloors([FromBody] List<long> ids);
    }

    public class AdminFloorAppService : YootekAppServiceBase, IAdminFloorAppService
    {
        private readonly IMapper _mapper;
        private readonly IRepository<Floor, long> _floorRepository;
        private readonly IRepository<AppOrganizationUnit, long> _organizationUnitRepository;

        public AdminFloorAppService(
            IRepository<Floor, long> floorRepository,
            IRepository<AppOrganizationUnit, long> organizationUnitRepository,
            IMapper mapper
        )
        {
            _mapper = mapper;
            _floorRepository = floorRepository;
            _organizationUnitRepository = organizationUnitRepository;
        }

        public async Task<DataResult> GetAllFloorAsync(GetAllFloorInput input)
        {
            try
            {
                IQueryable<FloorDto> query = (from floor in _floorRepository.GetAll()
                                              join building in _organizationUnitRepository.GetAll()
                                              on floor.BuildingId equals building.Id into tb_bd
                                              from building in tb_bd.DefaultIfEmpty()
                                              join urban in _organizationUnitRepository.GetAll() on floor.UrbanId equals urban.Id into tb_ub
                                              from urban in tb_ub.DefaultIfEmpty()
                                              select new FloorDto
                                              {
                                                  Id = floor.Id,
                                                  TenantId = floor.TenantId,
                                                  DisplayName = floor.DisplayName,
                                                  UrbanId = floor.UrbanId,
                                                  Code = floor.Code,
                                                  BuildingId = floor.BuildingId,
                                                  Description = floor.Description,
                                                  CreationTime = floor.CreationTime,
                                                  CreatorUserId = floor.CreatorUserId ?? 0,
                                                  BuildingName = building.DisplayName,
                                                  UrbanName = urban.DisplayName,
                                              })
                    .WhereIf(input.BuildingId.HasValue, u => u.BuildingId == input.BuildingId)
                    .WhereIf(input.UrbanId.HasValue, u => u.UrbanId == input.UrbanId)
                    .ApplySearchFilter(input.Keyword, x => x.DisplayName, x => x.Code);

                List<FloorDto> result = await query
                    .ApplySort(input.OrderBy, input.SortBy)
                    .ApplySort(OrderByFloor.DISPLAY_NAME)
                    .Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();

                return DataResult.ResultSuccess(result, "Get success!", query.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> CreateFloor(CreateFloorInput input)
        {
            try
            {
                Floor floorOrg = await _floorRepository.FirstOrDefaultAsync(x => x.DisplayName == input.DisplayName && x.BuildingId == input.BuildingId);
                if (floorOrg != null) throw new UserFriendlyException(409, "Floor is exist");
                Floor floor = ObjectMapper.Map<Floor>(input);
                floor.TenantId = AbpSession.TenantId;

                await _floorRepository.InsertAsync(floor);
                return DataResult.ResultSuccess(true, "Insert success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> UpdateFloor(UpdateFloorInput input)
        {
            try
            {
                Floor? updateData = await _floorRepository.FirstOrDefaultAsync(input.Id)
                                         ?? throw new Exception("Floor not found!");
                ObjectMapper.Map(input, updateData);
                await _floorRepository.UpdateAsync(updateData);
                return DataResult.ResultSuccess(true, "Update success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> DeleteFloor(long id)
        {
            try
            {
                await _floorRepository.DeleteAsync(id);
                return DataResult.ResultSuccess("Delete success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> DeleteManyFloors([FromBody] List<long> ids)
        {
            try
            {
                await _floorRepository.DeleteAsync(x => ids.Contains(x.Id));
                return DataResult.ResultSuccess("Delete list floors success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
    }
}
