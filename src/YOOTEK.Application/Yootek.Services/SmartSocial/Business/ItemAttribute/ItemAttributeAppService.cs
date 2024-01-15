using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.UI;
using Yootek.App.ServiceHttpClient.Dto;
using Yootek.App.ServiceHttpClient.Dto.Business;
using Yootek.App.ServiceHttpClient.Business;
using Yootek.Common.DataResult;
using System;
using System.Threading.Tasks;

namespace Yootek.Services.SmartSocial.ItemAttributes
{
    public interface IItemAttributeAppService : IApplicationService
    {
        Task<DataResult> GetAllItemAttributesAsync(GetAllItemAttributesDto input);
        Task<DataResult> GetItemAttributeByIdAsync(long id);
    }
    public class ItemAttributeAppService : YootekAppServiceBase, IItemAttributeAppService
    {
        private readonly IHttpItemAttributeService _httpItemAttributeService;
        public ItemAttributeAppService(
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
    }
}
