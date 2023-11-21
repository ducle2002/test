using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.UI;
using IMAX.App.ServiceHttpClient.Dto;
using IMAX.App.ServiceHttpClient.Dto.Imax.Business;
using IMAX.App.ServiceHttpClient.Imax.Business;
using IMAX.Common.DataResult;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IMAX.Services.SmartSocial.Categories
{
    public interface ICategoryAppService : IApplicationService
    {
        Task<DataResult> GetAllCategoriesAsync(GetAllCategoriesDto input);
        Task<DataResult> GetListCategoryFromChildrenAsync(long id);
        Task<DataResult> GetCategoryByIdAsync(long id);
    }

    public class CategoryAppService : IMAXAppServiceBase, ICategoryAppService
    {
        private readonly IHttpCategoryService _httpCategoryService;
        public CategoryAppService(
            IHttpCategoryService httpCategoryService
        )
        {
            _httpCategoryService = httpCategoryService;
        }
        public async Task<DataResult> GetAllCategoriesAsync(GetAllCategoriesDto input)
        {
            try
            {
                MicroserviceResultDto<PagedResultDto<CategoryDto>> result = await _httpCategoryService.GetListCategory(input);
                return DataResult.ResultSuccess(result.Result.Items, "Get success", result.Result.TotalCount);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<DataResult> GetListCategoryFromChildrenAsync(long id)
        {
            try
            {
                MicroserviceResultDto<List<CategoryDto>> result = await _httpCategoryService.GetListCategoryFromChildren(
                    new GetListCategoryFromChildrenDto() { Id = id });
                return DataResult.ResultSuccess(result.Result, "Get success");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<DataResult> GetCategoryByIdAsync(long id)
        {
            try
            {
                MicroserviceResultDto<CategoryDto> result = await _httpCategoryService.GetCategoryById(id);
                return DataResult.ResultSuccess(result.Result, "Get success");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }
    }
}
