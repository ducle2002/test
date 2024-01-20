using Abp.Application.Services;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Application;
using Yootek.Common.DataResult;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yootek.Yootek.EntityDb.Clb.Hotlines;

namespace Yootek.Services
{
    public interface IClbHotlineAppService : IApplicationService
    {
        Task<object> GetAllHotlineAsync(ListClbHotlineInputDto input);
        Task<object> CreateOrUpdateHotline(ClbHotlineInputDto input);
        Task<DataResult> DeleteHotline(long id);
    }

    public class ClbHotlineAppService : YootekAppServiceBase, IClbHotlineAppService
    {
        private readonly IRepository<ClbHotlines, long> _hotlineRepos;
        public ClbHotlineAppService(IRepository<ClbHotlines, long> hotlineRepos)
        {
            _hotlineRepos = hotlineRepos;
        }
        public async Task<object> CreateOrUpdateHotline(ClbHotlineInputDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    //update
                    var updateData = await _hotlineRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        //call back
                        await _hotlineRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "ud_hotline");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    var insertInput = input.MapTo<ClbHotlines>();
                    long id = await _hotlineRepos.InsertAndGetIdAsync(insertInput);
                    insertInput.Id = id;
                    mb.statisticMetris(t1, 0, "is_hotline");
                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
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

        public Task<DataResult> DeleteHotline(long id)
        {
            try
            {
                _hotlineRepos.DeleteAsync(id);
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

        public async Task<object> GetAllHotlineAsync(ListClbHotlineInputDto input)
        {
            try
            {
                // var ouUI = await _userOrganizationUnitRepository.GetAll()
                //     .Where(x => x.UserId == AbpSession.UserId)
                //     .Select(x => x.OrganizationUnitId)
                //     .ToListAsync();
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var query = (from dt in _hotlineRepos.GetAll()
                                 select new ClbGetInputHotline()
                                 {
                                     Id = dt.Id,
                                     TenantId = dt.TenantId,
                                     Name = dt.Name,
                                     Properties = dt.Properties,
                                 })
                                 .ApplySearchFilter(input.Keyword, x => x.Name, x => x.Properties);

                    var result = await query
                        .ApplySort(input.OrderBy, input.SortBy)
                        .ApplySort(OrderByHotline.NAME)
                        .PageBy(input).ToListAsync();
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

        public async Task<object> GetById(long id)
        {
            try
            {
                var data = await _hotlineRepos.GetAsync(id);
                return DataResult.ResultSuccess(data, "Success!");
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public Task<DataResult> DeleteMultipleHotline([FromBody] List<long> ids)
        {
            try
            {

                if (ids.Count == 0) return Task.FromResult(DataResult.ResultError("Err", "input empty"));
                var tasks = new List<Task>();
                foreach (var id in ids)
                {
                    var tk = DeleteHotline(id);
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
