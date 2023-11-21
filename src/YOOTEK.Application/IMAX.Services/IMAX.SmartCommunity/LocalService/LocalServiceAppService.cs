using Abp;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using IMAX.Common.DataResult;
using IMAX.EntityDb;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAX.Services
{
    public interface ILocalServiceAppService : IApplicationService
    {
        Task<object> CreateOrUpdateLocalService(LocalServiceDto input);
        Task<object> DeleteLocalService(long id);
        Task<object> GetAllLocalServiceByOrganization(GetAllLocalServiceInput input);
    }

    [AbpAuthorize]
    public class LocalServiceAppService : IMAXAppServiceBase, ILocalServiceAppService
    {
        private readonly IRepository<LocalService, long> _localServiceRepos;
        public LocalServiceAppService(IRepository<LocalService, long> localServiceRepos)
        {
            _localServiceRepos = localServiceRepos;
        }
        public async Task<object> CreateOrUpdateLocalService(LocalServiceDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    //update
                    var updateData = await _localServiceRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        //call back
                        await _localServiceRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "ud_local_service");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    var insertInput = input.MapTo<LocalService>();
                    long id = await _localServiceRepos.InsertAndGetIdAsync(insertInput);
                    insertInput.Id = id;
                    mb.statisticMetris(t1, 0, "is_local_service");
                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
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

        public async Task<object> DeleteLocalService(long id)
        {
            try
            {
                await _localServiceRepos.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Delete success!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        public async Task<object> DeleteMultipleLocalServices([FromBody] List<long> ids)
        {
            try
            {

                if (ids.Count == 0) return DataResult.ResultError("Err", "input empty");
                var tasks = new List<Task>();
                foreach (var id in ids)
                {
                    var tk = DeleteLocalService(id);
                    tasks.Add(tk);
                }
                Task.WaitAll(tasks.ToArray());

                var data = DataResult.ResultSuccess("Delete success!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        public async Task<object> GetAllLocalServiceByOrganization(GetAllLocalServiceInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var a = AbpSession.UserId;
                var query = _localServiceRepos.GetAll()
                   .WhereIf(input.OrganizationUnitId.HasValue, x => x.OrganizationUnitId == input.OrganizationUnitId)
                   .WhereIf(input.GroupType.HasValue, x => x.GroupType == input.GroupType)
                   .AsQueryable();

                var result = await query.PageBy(input).ToListAsync();

                var data = DataResult.ResultSuccess(result, "Get success", query.Count());
                mb.statisticMetris(t1, 0, "gall_list_localservice");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<object> GetLocalServiceByIdAsync(long id)
        {
            try
            {
                var data = _localServiceRepos.GetAsync(id);
                return DataResult.ResultSuccess(data, "Success!");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<object> GetHighestLocalServiceTypeNumber()
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var a = AbpSession.UserId;
                var query = (from ls in _localServiceRepos.GetAll()
                             select new
                             {
                                 ls.Type
                             })
                   .AsQueryable();

                var result = await query.OrderByDescending(x => x.Type).FirstOrDefaultAsync();

                var data = DataResult.ResultSuccess(result?.Type, "Get success", query.Count());
                mb.statisticMetris(t1, 0, "gall_list_localservice");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<object> CreateListLocalService(List<LocalServiceDto> input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                if (input != null)
                {
                    foreach (var obj in input)
                    {
                        obj.TenantId = AbpSession.TenantId;
                        var insertInput = obj.MapTo<LocalServiceDto>();
                        await _localServiceRepos.InsertAndGetIdAsync(insertInput);
                    }
                }

                await CurrentUnitOfWork.SaveChangesAsync();

                mb.statisticMetris(t1, 0, "admin_islist_obj");
                var data = DataResult.ResultSuccess("Insert success !");
                return data;

            }
            catch (Exception e)
            {
                throw new UserFriendlyException(e.Message);
            }

        }
    }
}
