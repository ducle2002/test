using System;
using System.Net.Http;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Runtime.Session;
using Abp.UI;
using Yootek.App.ServiceHttpClient.Dto.Business;
using Yootek.Common.DataResult;
using Yootek.Services.SmartSocial.Ecofarm;
using Microsoft.Extensions.Configuration;

namespace Yootek.Services.SmartSocial.Categories
{
    public interface IAdminCategoryAppService : IApplicationService
    {
        Task<object> GetAllAsync(GetAllCategoriesDto input);
        Task<object> GetByIdAsync(long id);
        Task<object> CreateAsync(CreateCategoryDto input);
        Task<object> UpdateAsync(UpdateCategoryDto input);
        Task<object> DeleteAsync(long id);
    }

    //  [AbpAuthorize(PermissionNames.Social_Business_Categories)]
    public class AdminCategoryAppService : YootekAppServiceBase, IAdminCategoryAppService
    {
        private readonly BaseHttpClient _httpClient;

        public AdminCategoryAppService(
            IAbpSession abpSession,
            IConfiguration configuration
        )
        {
            _httpClient = new BaseHttpClient(abpSession, configuration["ApiSettings:Business.Item"]);
        }

        public async Task<object> GetAllAsync(GetAllCategoriesDto input)
        {
            try
            {
                var result =
                    await _httpClient.SendSync<PagedResultDto<CategoryDto>>("/api/v1/Categories/get-list",
                        HttpMethod.Get, input);
                if (result.Success) return DataResult.ResultSuccess(result.Data.Items, "", result.Data.TotalCount);
                return result;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> GetByIdAsync(long id)
        {
            try
            {
                return await _httpClient.SendSync<object>("/api/v1/Categories/get-detail", HttpMethod.Get, new
                {
                    Id = id
                });
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> CreateAsync(CreateCategoryDto input)
        {
            try
            {
                return await _httpClient.SendSync<long>("/api/v1/Categories/create", HttpMethod.Post, input);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> UpdateAsync(UpdateCategoryDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/Categories/update", HttpMethod.Put, input);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> DeleteAsync(long id)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/Categories/delete", HttpMethod.Delete, new
                {
                    Id = id
                });
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
    }
}