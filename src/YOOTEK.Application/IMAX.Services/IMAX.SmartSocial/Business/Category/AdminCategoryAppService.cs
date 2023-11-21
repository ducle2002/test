using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.UI;
using IMAX.App.ServiceHttpClient.Dto;
using IMAX.App.ServiceHttpClient.Dto.Imax.Business;
using IMAX.App.ServiceHttpClient.Imax.Business;
using IMAX.Common.DataResult;
using System;
using System.Threading.Tasks;

namespace IMAX.Services.SmartSocial.Categories
{
    public interface IAdminCategoryAppService : IApplicationService
    {
        Task<DataResult> GetAllCategoriesAsync(GetAllCategoriesDto input);
        Task<DataResult> GetCategoryByIdAsync(long id);
        Task<DataResult> CreateCategoryAsync(CreateCategoryDto input);
        Task<DataResult> UpdateCategoryAsync(UpdateCategoryDto input);
        Task<DataResult> DeleteCategoryAsync(long id);
    }

    //  [AbpAuthorize(PermissionNames.Social_Business_Categories)]
    public class AdminCategoryAppService : IMAXAppServiceBase, IAdminCategoryAppService
    {
        private readonly IHttpCategoryService _httpCategoryService;
        public AdminCategoryAppService(
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

        public async Task<DataResult> CreateCategoryAsync(CreateCategoryDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpCategoryService.CreateCategory(input);
                return DataResult.ResultSuccess(result.Result, "Create success");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<DataResult> UpdateCategoryAsync(UpdateCategoryDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpCategoryService.UpdateCategory(input);
                return DataResult.ResultSuccess(result.Result, "Update success");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<DataResult> DeleteCategoryAsync(long id)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpCategoryService.DeleteCategory(new DeleteCategoryDto() { Id = id });
                return DataResult.ResultSuccess(result.Result, "Delete success");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }
    }
}
