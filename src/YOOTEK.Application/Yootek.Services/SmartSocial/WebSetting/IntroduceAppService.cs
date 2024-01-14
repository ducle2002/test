using Abp;
using Abp.Application.Services;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services
{

    public interface IIntroduceAppService : IApplicationService
    {
        Task<object> CreateOrUpdateIntroduce(IntroduceDto input);
        Task<object> DeleteIntroduce(long id);
        Task<object> GetAllIntroduce(GetAllIntroduceInput input);
        Task<object> GetIntroduce(GetIntroduceInput input);
    }
    public class IntroduceAppService : YootekAppServiceBase, IIntroduceAppService
    {
        private readonly IRepository<Introduce, long> _introduceRepos;
        public IntroduceAppService(IRepository<Introduce, long> introduceRepos)
        {
            _introduceRepos = introduceRepos;
        }


        public async Task<object> CreateOrUpdateIntroduce(IntroduceDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                if (input.Id > 0)
                {
                    //update
                    var updateData = await _introduceRepos.GetAsync((long)input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        //call back
                        await _introduceRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "Ud_intro");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    var insertInput = input.MapTo<Introduce>();
                    long id = await _introduceRepos.InsertAndGetIdAsync(insertInput);
                    insertInput.Id = id;
                    mb.statisticMetris(t1, 0, "is_intro");
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

        public async Task<object> DeleteIntroduce(long id)
        {
            try
            {
                await _introduceRepos.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Delete success!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetIntroduce(GetIntroduceInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var result = await _introduceRepos.FirstOrDefaultAsync(x => x.TenantId == (AbpSession.TenantId == null ? input.TenantId : AbpSession.TenantId));
                var data = DataResult.ResultSuccess(result, "Get success");
                mb.statisticMetris(t1, 0, "gall_intro");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetAllIntroduce(GetAllIntroduceInput input)
        {
            try
            {
                var query = (from ci in _introduceRepos.GetAll()
                             select new Introduce()
                             {
                                 Id = ci.Id,
                                 TenantId = ci.TenantId,
                                 Detail = ci.Detail,
                             });

                var result = await query.PageBy(input).ToListAsync();
                var data = DataResult.ResultSuccess(result, "Get success!", query.Count());
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }
    }
}
