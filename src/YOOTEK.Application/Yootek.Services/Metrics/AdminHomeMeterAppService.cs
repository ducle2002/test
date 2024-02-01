using Abp.Application.Services;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Common.DataResult;
using Yootek.Yootek.EntityDb.Yootek.Metrics;
using Yootek.Yootek.Services.Yootek.Metrics.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yootek.Yootek.Services.Yootek.Metrics
{
    public interface IAdminHomeMeterAppService: IApplicationService
    {
        Task<object> GetAllHomeMeter(HomeMeterQueryDto input);
        Task<object> CreateOrUpdateHomeMeter(HomeMeterInputDto input);
    }
    public class AdminHomeMeterAppService : YootekAppServiceBase, IAdminHomeMeterAppService
    {
        private readonly IRepository<HomeMeter, long> _homeMeterRepo;
        public AdminHomeMeterAppService(IRepository<HomeMeter, long> homeMeterRepo)
        {
            _homeMeterRepo = homeMeterRepo;
        }

        [Obsolete]
        public async Task<object> CreateOrUpdateHomeMeter(HomeMeterInputDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                input.TenantId = AbpSession.TenantId;
                if(input.Id > 0)
                {
                    var updateData = await _homeMeterRepo.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        await _homeMeterRepo.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "update_homemeter");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                } else
                {
                    var insertInput = input.MapTo<HomeMeter>();
                    long id = await _homeMeterRepo.InsertAndGetIdAsync(insertInput);
                    mb.statisticMetris(t1, 0, "insert_homeMeter");
                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                    return data;
                }
            } catch(Exception e)
            {
                throw;
            }
        }

        [Obsolete]
        public async Task<object> CreateMultipleHomeMeters([FromBody] List<HomeMeterInputDto> input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var outputList = new List<HomeMeter>();
                foreach(var i in input)
                {
                    i.TenantId = AbpSession.TenantId;
                    var insertInput = i.MapTo<HomeMeter>();
                    long id = await _homeMeterRepo.InsertAndGetIdAsync(insertInput);
                    outputList.Add(await _homeMeterRepo.FirstOrDefaultAsync(id));
                }
                mb.statisticMetris(t1, 0, "insert_homeMeter_multiple");
                var data = DataResult.ResultSuccess(outputList,"Insert success !", outputList.Count());
                return data;
            } catch(Exception e)
            {
                throw;
            }
        }

        protected IEnumerable<KeyValuePair<string, List<HomeMeter>>> QueryHomeMeter(HomeMeterQueryDto input)
        {
            var query = _homeMeterRepo.GetAll()
                .WhereIf(!String.IsNullOrEmpty(input.ApartmentCode), x => x.ApartmentCode.Trim().Contains(input.ApartmentCode.Trim()) 
                    || x.NameElectricMeter.Contains(input.ApartmentCode.Trim()) 
                    || x.NameWaterMeter.Contains(input.ApartmentCode.Trim()))
                .OrderBy(x => x.ApartmentCode).ThenByDescending(x => x.Period).ThenByDescending(x => x.CreationTime).AsQueryable().AsEnumerable()
                .GroupBy(x => x.ApartmentCode).Select(x => new {Key = x.Key, Detail = x.ToList()})
                .ToDictionary(x => x.Key, y => y.Detail);
            return query;
        }

        public async Task<object> GetAllHomeMeter(HomeMeterQueryDto input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    long t1 = TimeUtils.GetNanoseconds();
                    var query = QueryHomeMeter(input);
                    var list = query.Skip(input.SkipCount)
                    .Take(input.MaxResultCount).ToList();

                    var data = DataResult.ResultSuccess(list, "Get success", query.Count());
                    mb.statisticMetris(t1, 0, "gall_forum");
                    return data;
                }
            } catch (Exception e)
            {
                throw;
            }
        }

        public async Task<object> DeleteHomeMeter(long id)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                await _homeMeterRepo.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Delete Success");
                mb.statisticMetris(t1, 0, "del_homeRepo");
                return data;
            } catch(Exception e)
            {
                throw;
            }
        }

        public Task<DataResult> DeleteMultipleHomeMeter([FromBody] List<long> ids)
        {
            try
            {
                if (ids.Count == 0) return Task.FromResult(DataResult.ResultError("Err", "input empty"));
                var tasks = new List<Task>();
                foreach (var id in ids)
                {
                    var tk = DeleteHomeMeter(id);
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
    }
}
