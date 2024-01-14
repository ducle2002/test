using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.UI;
using Yootek.Application;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Linq.Extensions;
using Yootek.Yootek.Services.Yootek.SmartCommunity.MeterType.dto;

namespace Yootek.Services
{
    public interface IAdminMeterTypeAppService : IApplicationService
    {
        Task<DataResult> GetAllMeterTypeAsync(GetAllMeterTypeDto input);
        Task<DataResult> CreateMeterType(CreateMeterTypeInput input);
        Task<DataResult> UpdateMeterType(UpdateMeterTypeInput input);
        Task<DataResult> DeleteMeterType(long id);
        Task<DataResult> DeleteManyMeterType([FromBody] List<long> ids);
    }

    public class AdminMeterTypeAppService : YootekAppServiceBase, IAdminMeterTypeAppService
    {
        private readonly IRepository<MeterType, long> _meterTypeRepository;

        public AdminMeterTypeAppService(
            IRepository<MeterType, long> meterTypeRepository
        )
        {
            _meterTypeRepository = meterTypeRepository;
        }


        public async Task<DataResult> GetAllMeterTypeAsync(GetAllMeterTypeDto input)
        {
            try
            {
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
                {
                    var tenantId = AbpSession.TenantId;
                    IQueryable<MeterTypeDto> query = (from sm in _meterTypeRepository.GetAll()
                                                      select new MeterTypeDto
                                                      {
                                                          Id = sm.Id,
                                                          TenantId = sm.TenantId,
                                                          Name = sm.Name,
                                                          Description = sm.Description,
                                                          CreationTime = sm.CreationTime,
                                                          CreatorUserId = sm.CreatorUserId ?? 0,
                                                          BillType = sm.BillType
                                                      })
                        .WhereIf(tenantId != null, x => x.TenantId == tenantId)
                        .ApplySearchFilter(input.Keyword, x => x.Name);

                    List<MeterTypeDto> result = await query
                        .Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                    return DataResult.ResultSuccess(result, "Get success!", query.Count());
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> CreateMeterType(CreateMeterTypeInput input)
        {
            try
            {
                MeterType? meterTypeOrg = await _meterTypeRepository.FirstOrDefaultAsync(x => x.Name == input.Name);
                if (meterTypeOrg != null) throw new UserFriendlyException(409, "Name is exist");
                MeterType meterType = ObjectMapper.Map<MeterType>(input);
                meterType.TenantId = AbpSession.TenantId;
                await _meterTypeRepository.InsertAsync(meterType);
                return DataResult.ResultSuccess(true, "Insert success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> UpdateMeterType(UpdateMeterTypeInput input)
        {
            try
            {
                MeterType? updateData = await _meterTypeRepository.FirstOrDefaultAsync(input.Id)
                                        ?? throw new Exception("MeterType not found!");
                MeterType meterType = ObjectMapper.Map(input, updateData);
                await _meterTypeRepository.UpdateAsync(updateData);
                return DataResult.ResultSuccess(true, "Update success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> DeleteMeterType(long id)
        {
            try
            {
                await _meterTypeRepository.DeleteAsync(id);
                return DataResult.ResultSuccess("Delete success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> DeleteManyMeterType([FromBody] List<long> ids)
        {
            try
            {
                await _meterTypeRepository.DeleteAsync(x => ids.Contains(x.Id));
                return DataResult.ResultSuccess("Delete list success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
    }
}