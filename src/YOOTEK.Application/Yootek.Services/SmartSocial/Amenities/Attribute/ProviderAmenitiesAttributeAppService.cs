using Abp.Application.Services.Dto;
using Abp.Application.Services;
using Abp.Runtime.Session;
using Abp.UI;
using Yootek.Common.DataResult;
using Yootek.Services.SmartSocial.Advertisements;
using Yootek.Services.SmartSocial.Ecofarm;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Yootek.Yootek.Services.Yootek.SmartSocial
{
    public interface IProviderAmenitiesAttributeAppService : IApplicationService
    {
        Task<object> GetByBusinessType(int type);
    }

    public class ProviderAmenitiesAttributeAppService : YootekAppServiceBase, IProviderAmenitiesAttributeAppService
    {
        private readonly IAbpSession _abpSession;
        private readonly BaseHttpClient _httpClient;
        private string baseUrl = "api/v1/AmenitiesGroup/";

        public ProviderAmenitiesAttributeAppService(IAbpSession abpSession, IConfiguration configuration)
        {
            _abpSession = abpSession;
            _httpClient = new BaseHttpClient(abpSession, configuration["ApiSettings:Business.Item"]);
        }

        public async Task<object> GetByBusinessType(int type)
        {
            try
            {
                var input = new GetAllAmenitiesAttributeInput()
                {
                    BusinessType = type,
                    IsPagination = false

                };
                var result = await _httpClient.SendSync<PagedResultDto<AmenitiesGroupDto>>(baseUrl + "get-all", HttpMethod.Get, input);
                if (result.Success) return DataResult.ResultSuccess(result.Data.Items, "", result.Data.TotalCount);
                return result;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

     
    }

}
