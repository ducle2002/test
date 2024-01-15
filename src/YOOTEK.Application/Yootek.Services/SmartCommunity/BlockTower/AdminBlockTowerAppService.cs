using Abp.Application.Services;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
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
    public interface IAdminBlockTowerAppService : IApplicationService
    {
        Task<DataResult> GetAllBlockTowerAsync(GetAllBlockTowerInput input);
        Task<DataResult> CreateBlockTower(CreateBlockTowerInput input);
        Task<DataResult> UpdateBlockTower(UpdateBlockTowerInput input);
        Task<DataResult> DeleteBlockTower(long id);
        Task<DataResult> DeleteManyBlockTowers([FromBody] List<long> ids);
    }

    public class AdminBlockTowerAppService : YootekAppServiceBase, IAdminBlockTowerAppService
    {
        private readonly IRepository<BlockTower, long> _blockTowerRepository;
        private readonly IRepository<AppOrganizationUnit, long> _organizationUnitRepository;
        public AdminBlockTowerAppService(
            IRepository<BlockTower, long> blockTowerRepository,
            IRepository<AppOrganizationUnit, long> organizationUnitRepository
        )
        {
            _blockTowerRepository = blockTowerRepository;
            _organizationUnitRepository = organizationUnitRepository;
        }
        public async Task<DataResult> GetAllBlockTowerAsync(GetAllBlockTowerInput input)
        {
            try
            {
                IQueryable<BlockTowerDto> query = (from blockTower in _blockTowerRepository.GetAll()
                                                   join building in _organizationUnitRepository.GetAll()
                                                   on blockTower.BuildingId equals building.Id into blockTower_building
                                                   from building in blockTower_building.DefaultIfEmpty()
                                                   join urban in _organizationUnitRepository.GetAll()
                                                   on blockTower.UrbanId equals urban.Id into blockTower_ub
                                                   from urban in blockTower_ub.DefaultIfEmpty()
                                                   select new BlockTowerDto
                                                   {
                                                       Id = blockTower.Id,
                                                       TenantId = blockTower.TenantId,
                                                       DisplayName = blockTower.DisplayName,
                                                       Code = blockTower.Code,
                                                       UrbanId = blockTower.UrbanId,
                                                       UrbanName = urban.DisplayName,
                                                       BuildingId = blockTower.BuildingId,
                                                       BuildingName = building.DisplayName,
                                                       Description = blockTower.Description,
                                                       CreationTime = blockTower.CreationTime,
                                                       CreatorUserId = blockTower.CreatorUserId ?? 0,
                                                   })
                    .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                    .WhereIf(input.BuildingId.HasValue, u => u.BuildingId == input.BuildingId)
                    .WhereIf(input.UrbanId.HasValue, u => u.UrbanId == input.UrbanId)
                    .ApplySearchFilter(input.Keyword, x => x.DisplayName, x => x.Code);

                List<BlockTowerDto> result = await query
                    .ApplySort(input.OrderBy, input.SortBy)
                    .ApplySort(OrderByBlockTower.CODE)
                    .Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();

                return DataResult.ResultSuccess(result, "Get success!", query.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> CreateBlockTower(CreateBlockTowerInput input)
        {
            try
            {
                BlockTower? blockTowerOrg = await _blockTowerRepository.FirstOrDefaultAsync(x => x.Code == input.Code);
                if (blockTowerOrg != null) throw new UserFriendlyException(409, "BlockTower is exist");
                BlockTower blockTower = input.MapTo<BlockTower>();
                blockTower.TenantId = AbpSession.TenantId;

                await _blockTowerRepository.InsertAsync(blockTower);
                return DataResult.ResultSuccess(true, "Insert success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> UpdateBlockTower(UpdateBlockTowerInput input)
        {
            try
            {
                BlockTower? updateData = await _blockTowerRepository.FirstOrDefaultAsync(input.Id)
                                         ?? throw new Exception("BlockTower not found!");
                ObjectMapper.Map(input, updateData);
                await _blockTowerRepository.UpdateAsync(updateData);
                return DataResult.ResultSuccess(true, "Update success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> DeleteBlockTower(long id)
        {
            try
            {
                await _blockTowerRepository.DeleteAsync(id);
                return DataResult.ResultSuccess("Delete success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> DeleteManyBlockTowers([FromBody] List<long> ids)
        {
            try
            {
                await _blockTowerRepository.DeleteAsync(x => ids.Contains(x.Id));
                return DataResult.ResultSuccess("Delete list block towers success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
    }
}
