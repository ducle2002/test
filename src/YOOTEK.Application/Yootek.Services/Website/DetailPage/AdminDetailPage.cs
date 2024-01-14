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
    public interface IAdminDetailPage : IApplicationService
    {
        Task<object> CreateOrUpdateDetailPage(DetailPageInput input);
        Task<object> DeleteDetailPage(long id);
        Task<object> GetAllDetailPages(GetAllDetailPagesInput input);
        Task<object> GetDetailPage(long id);
    }
    public class AdminDetailPage : YootekAppServiceBase, IAdminDetailPage
    {
        private readonly IRepository<DetailPage, long> _detailPageRepos;
        public AdminDetailPage(IRepository<DetailPage, long> detailPageRepos)
        {
            _detailPageRepos = detailPageRepos;
        }
        public async Task<object> CreateOrUpdateDetailPage(DetailPageInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                if (input.Id > 0)
                {
                    //update
                    var updateData = await _detailPageRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        //call back
                        await _detailPageRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "Ud_news");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    var insertInput = input.MapTo<DetailPage>();
                    long id = await _detailPageRepos.InsertAndGetIdAsync(insertInput);
                    insertInput.Id = id;
                    mb.statisticMetris(t1, 0, "is_news");
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

        public async Task<object> DeleteDetailPage(long id)
        {
            try
            {
                await _detailPageRepos.DeleteAsync(id);
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

        public async Task<object> GetAllDetailPages(GetAllDetailPagesInput input)
        {
            try
            {
                var query = (from ci in _detailPageRepos.GetAll()
                             select new DetailPage()
                             {
                                 Id = ci.Id,
                                 Title = ci.Title,
                                 Description = ci.Description,
                                 Keywords = ci.Keywords,
                                 Image = ci.Image,
                                 Type = ci.Type,
                                 Status = ci.Status,
                                 Language = ci.Language,
                                 CreationTime = ci.CreationTime,
                                 CreatorUserId = ci.CreatorUserId,
                                 LastModificationTime = ci.LastModificationTime,
                             }).WhereIf(input.Language != null, x => input.Language == x.Language);

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

        public async Task<object> GetDetailPage(long id)
        {
            try
            {
                var result = await _detailPageRepos.GetAsync(id);
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
