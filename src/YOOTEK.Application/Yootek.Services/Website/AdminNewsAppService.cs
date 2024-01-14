using Abp;
using Abp.Application.Services;
using Abp.Authorization;
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

    public interface IAdminNewsAppService : IApplicationService
    {
        Task<object> CreateOrUpdateNewsWebImax(NewsWebImaxDto input);
        Task<object> DeleteNewsWebImax(long id);
        Task<object> GetAllNewsWebImax(GetAllNewsWebImaxInput input);
        Task<object> GetDetailNewsWebImax(long id);
        Task<object> GetDetailNewsWebImaxByKeyUrl(string keyUrl);
    }

    [AbpAuthorize]
    public class AdminNewsAppService : YootekAppServiceBase, IAdminNewsAppService
    {
        private readonly IRepository<NewsWebImax, long> _newWebImaxRepos;
        public AdminNewsAppService(IRepository<NewsWebImax, long> newWebImaxRepos)
        {
            _newWebImaxRepos = newWebImaxRepos;
        }
        public async Task<object> CreateOrUpdateNewsWebImax(NewsWebImaxDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                if (input.Id > 0)
                {

                    input.KeyUrl = ConvertSignedToUnsigned(input.Title) + "-" + (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
                    //update
                    var updateData = await _newWebImaxRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        //call back
                        await _newWebImaxRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "Ud_news");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    input.KeyUrl = ConvertSignedToUnsigned(input.Title) + "-" + (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
                    var insertInput = input.MapTo<NewsWebImax>();
                    long id = await _newWebImaxRepos.InsertAndGetIdAsync(insertInput);
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

        public async Task<object> DeleteNewsWebImax(long id)
        {
            try
            {
                await _newWebImaxRepos.DeleteAsync(id);
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

        public async Task<object> GetAllNewsWebImax(GetAllNewsWebImaxInput input)
        {
            try
            {
                var query = (from ci in _newWebImaxRepos.GetAll()
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
                             }).WhereIf(input.Category.HasValue, x => input.Category == x.Category)
                             .WhereIf(input.State.HasValue, x => input.State == x.State);

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

        public string ConvertSignedToUnsigned(string str)
        {
            str = str.ToLower();
            {
                for (int i = 1; i < VietnameseSigns.Length; i++)
                {
                    for (int j = 0; j < VietnameseSigns[i].Length; j++)
                        str = str.Replace(VietnameseSigns[i][j], VietnameseSigns[0][i - 1]);
                }
                str = str.Replace(" ", "-");
                return str;
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

        private readonly string[] VietnameseSigns = new string[]
        {

            "aAeEoOuUiIdDyY",

            "áàạảãâấầậẩẫăắằặẳẵ",

            "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",

            "éèẹẻẽêếềệểễ",

            "ÉÈẸẺẼÊẾỀỆỂỄ",

            "óòọỏõôốồộổỗơớờợởỡ",

            "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",

            "úùụủũưứừựửữ",

            "ÚÙỤỦŨƯỨỪỰỬỮ",

            "íìịỉĩ",

            "ÍÌỊỈĨ",

            "đ",

            "Đ",

            "ýỳỵỷỹ",

            "ÝỲỴỶỸ"
        };
    }
}
