using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.UI;
using IMAX.App.ServiceHttpClient.Dto;
using IMAX.App.ServiceHttpClient.Dto.Imax.Business;
using IMAX.App.ServiceHttpClient.Imax.Business;
using IMAX.Common.DataResult;
using System;
using System.Threading.Tasks;

namespace IMAX.Services.SmartSocial.ItemAttributes
{
    public interface IAdminItemAttributeAppService : IApplicationService
    {
        Task<DataResult> GetAllItemAttributesAsync(GetAllItemAttributesDto input);
        Task<DataResult> GetItemAttributeByIdAsync(long id);
        Task<DataResult> CreateItemAttributeAsync(CreateItemAttributeDto input);
        Task<DataResult> CreateListItemAttributesAsync(CreateListItemAttributesDto input);
        Task<DataResult> UpdateItemAttributeAsync(UpdateItemAttributeDto input);
        Task<DataResult> UpdateListItemAttributesAsync(UpdateListItemAttributesDto input);
        Task<DataResult> DeleteItemAttributeAsync(long id);
    }
    // [AbpAuthorize(PermissionNames.Social_Business_ItemAttribute)]
    public class AdminItemAttributeAppService : IMAXAppServiceBase, IAdminItemAttributeAppService
    {
        private readonly IHttpItemAttributeService _httpItemAttributeService;
        public AdminItemAttributeAppService(
            IHttpItemAttributeService httpItemAttributeService
        )
        {
            _httpItemAttributeService = httpItemAttributeService;
        }

        // GET (all)
        public async Task<DataResult> GetAllItemAttributesAsync(GetAllItemAttributesDto input)
        {
            try
            {
                MicroserviceResultDto<PagedResultDto<ItemAttributeDto>> result = await _httpItemAttributeService.GetAllItemAttributes(input);
                return DataResult.ResultSuccess(result.Result.Items, "Get all item attribute success", result.Result.TotalCount);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

        // GET (detail)
        public async Task<DataResult> GetItemAttributeByIdAsync(long id)
        {
            try
            {
                MicroserviceResultDto<ItemAttributeDto> result = await _httpItemAttributeService.GetItemAttributeById(id);
                return DataResult.ResultSuccess(result.Result, "Get detail item attribute success");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

        // CREATE (one)
        public async Task<DataResult> CreateItemAttributeAsync(CreateItemAttributeDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpItemAttributeService.CreateItemAttribute(input);
                return DataResult.ResultSuccess("Create item attribute success");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

        // UPDATE (one)
        public async Task<DataResult> UpdateItemAttributeAsync(UpdateItemAttributeDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpItemAttributeService.UpdateItemAttribute(input);
                return DataResult.ResultSuccess("Update item attribute success");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

        // DELETE
        public async Task<DataResult> DeleteItemAttributeAsync(long id)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpItemAttributeService.DeleteItemAttribute(new DeleteItemAttributeDto() { Id = id });
                return DataResult.ResultSuccess("Delete item attribute success");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

        // CREATE (many)
        public async Task<DataResult> CreateListItemAttributesAsync(CreateListItemAttributesDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpItemAttributeService.CreateListItemAttributes(input);
                return DataResult.ResultSuccess(result.Result, "Create list item attribute success");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

        // UPDATE (many)
        public async Task<DataResult> UpdateListItemAttributesAsync(UpdateListItemAttributesDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpItemAttributeService.UpdateListItemAttributes(input);
                return DataResult.ResultSuccess(result.Result, "Update list item attribute success");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }
    }
}
