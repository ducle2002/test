using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Runtime.Session;
using Yootek.App.ServiceHttpClient.Dto.Business;
using Yootek.Common.DataResult;
using Yootek.Services.SmartSocial.Ecofarm;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Yootek.Services.SmartSocial.Categories
{
    public interface ICategoryAppService : IApplicationService
    {
        Task<object> GetAllCategoriesAsync([FromQuery] GetAllCategoriesDto input);
        Task<object> GetListCategoryFromChildrenAsync(GetListCategoryFromChildrenDto input);
        Task<object> GetCategoryByIdAsync(GetCategoryByIdDto input);
        Task<object> GetAllCategoriesForEcoFarmAsync(GetAllCategoriesDto input);
    }

    public class CategoryAppService : YootekAppServiceBase, ICategoryAppService
    {
        private readonly BaseHttpClient _httpClient;

        public CategoryAppService(IAbpSession abpSession, IConfiguration configuration
        )
        {
            _httpClient = new BaseHttpClient(abpSession, configuration["ApiSettings:Business.Item"]);
        }

        public async Task<object> GetAllCategoriesAsync([FromQuery] GetAllCategoriesDto input)
        {
            try
            {
                var result =
                    await _httpClient.SendSync<PagedResultDto<CategoryDto>>("/api/v1/Categories/get-list",
                        HttpMethod.Get, input);
                if (result.Success) return DataResult.ResultSuccess(result.Data.Items, "", result.Data.TotalCount);
                return result;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> GetListCategoryFromChildrenAsync(GetListCategoryFromChildrenDto input)
        {
            try
            {
                return await _httpClient.SendSync<List<CategoryDto>>("/api/v1/Categories/get-list-from-children",
                    HttpMethod.Get, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> GetCategoryByIdAsync(GetCategoryByIdDto input)
        {
            try
            {
                return await _httpClient.SendSync<object>("/api/v1/Categories/get-detail", HttpMethod.Get, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> GetAllCategoriesForEcoFarmAsync(GetAllCategoriesDto input)
        {
            try
            {
                var result =
                    await _httpClient.SendSync<PagedResultDto<CategoryDto>>("/api/v1/Categories/ecofarm/get-list",
                        HttpMethod.Get, input);
                if (result.Success) return DataResult.ResultSuccess(result.Data.Items, "", result.Data.TotalCount);
                return result;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
    }
}