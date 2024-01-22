using Abp.Application.Services;
using Abp.AutoMapper;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using Abp;
using Abp.UI;

namespace Yootek.Services
{
    public interface IAdminComponentPage : IApplicationService
    {
        Task<object> CreateOrUpdateComponentPage(ComponentPageInputDto input);
        Task<object> DeleteComponentPage(long id);
        Task<object> GetAllComponentPages(GetAllComponentPagesDto input);
        Task<object> GetComponentPage(long id);
    }
    public class AdminComponentPage : YootekAppServiceBase, IAdminComponentPage
    {
        private readonly IRepository<ComponentPage, long> _componentPageRepos;
        public AdminComponentPage(IRepository<ComponentPage, long> componentPageRepos)
        {
            _componentPageRepos = componentPageRepos;
        }
        public async Task<object> CreateOrUpdateComponentPage(ComponentPageInputDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                if (input.Id > 0)
                {
                    //update
                    var updateData = await _componentPageRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        //call back
                        await _componentPageRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "Ud_component");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    var insertInput = input.MapTo<ComponentPage>();
                    long id = await _componentPageRepos.InsertAndGetIdAsync(insertInput);
                    insertInput.Id = id;
                    mb.statisticMetris(t1, 0, "is_component");
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

        public async Task<object> DeleteComponentPage(long id)
        {
            try
            {
                await _componentPageRepos.DeleteAsync(id);
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

        public async Task<object> GetAllComponentPages(GetAllComponentPagesDto input)
        {
            try
            {
                var query = (from ci in _componentPageRepos.GetAll()
                             select new ComponentPage()
                             {
                                 Id = ci.Id,
                                 Title1 = ci.Title1,
                                 Title2 = ci.Title2,
                                 Content = ci.Content,
                                 ImageBackground = ci.ImageBackground,
                                 Image1 = ci.Image1,
                                 Image2 = ci.Image2,
                                 Type = ci.Type,
                                 Status = ci.Status,
                                 Language = ci.Language,
                                 CreationTime = ci.CreationTime,
                                 CreatorUserId = ci.CreatorUserId,
                                 LastModificationTime = ci.LastModificationTime,
                                 Order = ci.Order,
                                 Button = ci.Button
                             }).WhereIf(input.Language != null, x => input.Language == x.Language)
                             .WhereIf(input.Order.HasValue, x => input.Order == x.Order)
                             .WhereIf(input.Status.HasValue, x => input.Status == x.Status)
                             .WhereIf(input.Type.HasValue, x => input.Type == x.Type);

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

        public async Task<object> GetComponentPage(long id)
        {
            try
            {
                var result = await _componentPageRepos.GetAsync(id);
                var data = DataResult.ResultSuccess(result, "Get detail success!");
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
