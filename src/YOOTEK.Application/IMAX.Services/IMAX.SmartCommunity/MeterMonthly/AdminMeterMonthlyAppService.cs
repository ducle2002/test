using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.UI;
using IMAX.Application;
using IMAX.Common.DataResult;
using IMAX.EntityDb;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Collections.Extensions;
using Abp.Linq.Extensions;
using IMAX.IMAX.Services.IMAX.SmartCommunity.MeterMonthly.dto;

namespace IMAX.Services
{
    public interface IAdminMeterMonthlyAppService : IApplicationService
    {
        Task<DataResult> GetAllMeterMonthlyAsync(GetAllMeterMonthlyDto input);
        Task<DataResult> CreateMeterMonthly(CreateMeterMonthlyInput input);
        Task<DataResult> UpdateMeterMonthly(UpdateMeterMonthlyInput input);
        Task<DataResult> DeleteMeterMonthly(long id);
        Task<DataResult> DeleteManyMeterMonthly([FromBody] List<long> ids);
    }

    public class AdminMeterMonthlyAppService : IMAXAppServiceBase, IAdminMeterMonthlyAppService
    {
        private readonly IRepository<MeterMonthly, long> _meterMonthlyRepository;
        private readonly IRepository<Meter, long> _meterRepository;

        public AdminMeterMonthlyAppService(
            IRepository<MeterMonthly, long> meterMonthlyRepository,
            IRepository<Meter, long> meterRepository
        )
        {
            _meterMonthlyRepository = meterMonthlyRepository;
            _meterRepository = meterRepository;
        }


        public async Task<DataResult> GetAllMeterMonthlyAsync(GetAllMeterMonthlyDto input)
        {
            try
            {
                DateTime newFromMonth = new DateTime(), newToMonth = new DateTime();
                if (input.FromMonth.HasValue)
                {
                    newFromMonth = new DateTime(input.FromMonth.Value.Year, input.FromMonth.Value.Month, 1, 0, 0, 0);
                }

                if (input.ToMonth.HasValue)
                {
                    newToMonth = new DateTime(input.ToMonth.Value.Year, input.ToMonth.Value.Month, 2, 23, 59, 59);
                }

                IQueryable<MeterMonthlyDto> query = (from sm in _meterMonthlyRepository.GetAll()
                        join meter in _meterRepository.GetAll() on sm.MeterId equals meter.Id into tb_mt
                        from meter in tb_mt.DefaultIfEmpty()
                        select new MeterMonthlyDto
                        {
                            Id = sm.Id,
                            TenantId = sm.TenantId,
                            Period = sm.Period,
                            Value = sm.Value,
                            MeterId = sm.MeterId,
                            CreationTime = sm.CreationTime,
                            CreatorUserId = sm.CreatorUserId ?? 0,
                            MeterTypeId = meter.MeterTypeId,
                            Name = meter.Name,
                            ImageUrl = sm.ImageUrl
                        })
                    .WhereIf(input.MeterTypeId != null, x => x.MeterTypeId == input.MeterTypeId)
                    .WhereIf(input.MinValue.HasValue, x => x.Value >= input.MinValue)
                    .WhereIf(input.MaxValue.HasValue, x => x.Value <= input.MaxValue)
                    .WhereIf(input.FromMonth.HasValue, x => x.Period >= newFromMonth)
                    .WhereIf(input.ToMonth.HasValue, x => x.Period <= newToMonth);

                List<MeterMonthlyDto> result = await query
                    .Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                return DataResult.ResultSuccess(result, "Get success!", query.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        public async Task<DataResult> CreateMeterMonthly(CreateMeterMonthlyInput input)
        {
            try
            {
                MeterMonthly meterMonthly = ObjectMapper.Map<MeterMonthly>(input);
                meterMonthly.TenantId = AbpSession.TenantId;
                if (input.Period != null)
                {
                    meterMonthly.Period = DateTime.Today;
                }

                //chi lay year va month
                meterMonthly.Period = new DateTime(meterMonthly.Period.Year, meterMonthly.Period.Month, 1, 0, 0, 0);

                await _meterMonthlyRepository.InsertAsync(meterMonthly);
                return DataResult.ResultSuccess(true, "Insert success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        public async Task<DataResult> UpdateMeterMonthly(UpdateMeterMonthlyInput input)
        {
            try
            {
                MeterMonthly? updateData = await _meterMonthlyRepository.FirstOrDefaultAsync(input.Id)
                                           ?? throw new Exception("MeterMonthly not found!");
                MeterMonthly meterMonthly = ObjectMapper.Map(input, updateData);
                //chi lay year va month
                meterMonthly.Period = new DateTime(meterMonthly.Period.Year, meterMonthly.Period.Month, 1, 0, 0, 0);
                await _meterMonthlyRepository.UpdateAsync(updateData);
                return DataResult.ResultSuccess(true, "Update success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        public async Task<DataResult> DeleteMeterMonthly(long id)
        {
            try
            {
                await _meterMonthlyRepository.DeleteAsync(id);
                return DataResult.ResultSuccess("Delete success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        public async Task<DataResult> DeleteManyMeterMonthly([FromBody] List<long> ids)
        {
            try
            {
                await _meterMonthlyRepository.DeleteAsync(x => ids.Contains(x.Id));
                return DataResult.ResultSuccess("Delete list success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }
    }
}