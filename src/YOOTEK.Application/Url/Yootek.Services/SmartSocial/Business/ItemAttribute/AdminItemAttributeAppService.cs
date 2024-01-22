using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.UI;
using Yootek.App.ServiceHttpClient.Dto;
using Yootek.App.ServiceHttpClient.Dto.Business;
using Yootek.App.ServiceHttpClient.Business;
using Yootek.Common.DataResult;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Yootek.Services.SmartSocial.ItemAttributes
{
    public interface IAdminItemAttributeAppService : IApplicationService
    {
        Task<DataResult> GetAllItemAttributesAsync(GetAllItemAttributesDto input);
        Task<DataResult> GetItemAttributeByIdAsync(long id);
        Task<DataResult> CreateItemAttributeAsync(CreateItemAttributeDto input);
        Task<DataResult> CreateListItemAttributesAsync(List<CreateItemAttributeDto> input);
        Task<DataResult> UpdateItemAttributeAsync(UpdateItemAttributeDto input);
        Task<DataResult> UpdateListItemAttributesAsync(List<UpdateItemAttributeDto> input);
        Task<DataResult> DeleteItemAttributeAsync(long id);
    }
    // [AbpAuthorize(PermissionNames.Social_Business_ItemAttribute)]
    public class AdminItemAttributeAppService : YootekAppServiceBase, IAdminItemAttributeAppService
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
                throw;
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
                throw;
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
                throw;
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
                throw;
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
                throw;
            }
        }

        // CREATE (many)
        public async Task<DataResult> CreateListItemAttributesAsync(List<CreateItemAttributeDto> input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpItemAttributeService.CreateListItemAttributes(input);
                return DataResult.ResultSuccess(result.Result, "Create list item attribute success");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        // UPDATE (many)
        public async Task<DataResult> UpdateListItemAttributesAsync(List<UpdateItemAttributeDto> input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpItemAttributeService.UpdateListItemAttributes(input);
                return DataResult.ResultSuccess(result.Result, "Update list item attribute success");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
    }
}
