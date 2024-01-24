using Abp;
using Abp.Application.Services;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Common.DataResult;
using Yootek.Common.Enum;
using Yootek.EntityDb;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services
{

    public interface IUserNewsAppService : IApplicationService
    {
        Task<object> GetAllNewsWebImax(GetAllNewsWebImaxInput input);
        Task<object> GetDetailNewsWebImax(long id);
        Task<object> GetDetailNewsWebImaxByKeyUrl(string keyUrl);
    }
    public class UserNewsAppService : YootekAppServiceBase, IUserNewsAppService
    {
        private readonly IRepository<NewsWebImax, long> _newWebImaxRepos;
        public UserNewsAppService(IRepository<NewsWebImax, long> newWebImaxRepos)
        {
            _newWebImaxRepos = newWebImaxRepos;
        }
        public async Task<object> GetAllNewsWebImax(GetAllNewsWebImaxInput input)
        {
            try
            {
                var query = (from ci in _newWebImaxRepos.GetAll()
                             where ci.State == (int)CommonENum.STATE_NEWS.SHOW
                             orderby ci.Top > 0 ? ci.Top : ci.Top descending
                             select new NewsWebImaxDto()
                             {
                                 Id = ci.Id,
                                 Title = ci.Title,
                                 Description = ci.Description,
                                 State = ci.State,
                                 Keywords = ci.Keywords,
                                 TitleShort = ci.TitleShort,
                                 DescriptionShort = ci.DescriptionShort,
                                 Top = ci.Top,
                                 Category = ci.Category,
                                 Content = ci.Content,
                                 Avatar = ci.Avatar,
                                 CreationTime = ci.CreationTime,
                                 CreatorUserId = ci.CreatorUserId,
                                 LastModificationTime = ci.LastModificationTime,
                                 KeyUrl = ci.KeyUrl
                             }).WhereIf(input.Category.HasValue, x => input.Category == x.Category);
                var result = await query.OrderByDescending(x => x.CreationTime).PageBy(input).ToListAsync();
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

        public async Task<object> GetDetailNewsWebImax(long id)
        {
            try
            {
                var result = await _newWebImaxRepos.GetAsync(id);
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

        public async Task<object> GetDetailNewsWebImaxByKeyUrl(string keyUrl)
        {
            try
            {
                var result = await _newWebImaxRepos.FirstOrDefaultAsync(x => x.KeyUrl.Contains(keyUrl));
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
