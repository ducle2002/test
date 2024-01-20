using System;
using System.Net.Http;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Runtime.Session;
using Yootek.App.ServiceHttpClient.Dto.Business;
using Yootek.Common.DataResult;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Yootek.Services.SmartSocial.Ecofarm
{
    public interface IEcoFarmProvidersAppService : IApplicationService
    {
        Task<object> GetListByPartner([FromQuery] GetListEcofarmProvidersByPartnerDto input);
        Task<object> GetListByUser([FromQuery] GetListEcofarmProvidersByUserDto input);
        Task<object> GetById([FromQuery] GetItemEcofarmByIdDto input);
        Task<object> Create([FromBody] CreateProviderEcofarmDto input);
        Task<object> Update([FromBody] UpdateProviderEcofarmDto input);
        Task<object> UpdateState([FromBody] UpdateStateProviderEcofarmDto input);
        Task<object> Delete([FromQuery] DeleteProviderEcofarmDto input);
    }

    public class EcoFarmProvidersAppService : YootekAppServiceBase, IEcoFarmProvidersAppService
    {
        private readonly IAbpSession _abpSession;
        private readonly BaseHttpClient _httpClient;

        public EcoFarmProvidersAppService(IAbpSession abpSession, IConfiguration configuration)
        {
            _abpSession = abpSession;
            _httpClient = new BaseHttpClient(abpSession, configuration["ApiSettings:Business.Report"]);
        }

        public async Task<object> GetListByPartner([FromQuery] GetListEcofarmProvidersByPartnerDto input)
        {
            try
            {
                var result = await _httpClient.SendSync<PagedResultDto<EcoFarmProviderGetListDto>>(
                    "/api/v1/EcoFarmProviders/partner/get-list", HttpMethod.Get, input);
                if (result.Success) return DataResult.ResultSuccess(result.Data.Items, "", result.Data.TotalCount);
                return result;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> GetListByUser(GetListEcofarmProvidersByUserDto input)
        {
            try
            {
                var result = await _httpClient.SendSync<PagedResultDto<EcoFarmProviderGetListDto>>(
                    "/api/v1/EcoFarmProviders/user/get-list", HttpMethod.Get, input);
                if (result.Success) return DataResult.ResultSuccess(result.Data.Items, "", result.Data.TotalCount);
                return result;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        public async Task<object> GetById([FromQuery] GetItemEcofarmByIdDto input)
        {
            try
            {
                return await _httpClient.SendSync<EcoFarmProviderDetailDto>("/api/v1/EcoFarmProviders/get-detail",
                    HttpMethod.Get, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        public async Task<object> Create(
            [FromBody] CreateProviderEcofarmDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/EcoFarmProviders/create", HttpMethod.Post, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> Update(
            [FromBody] UpdateProviderEcofarmDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/EcoFarmProviders/update", HttpMethod.Put, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> UpdateState(
            [FromBody] UpdateStateProviderEcofarmDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/EcoFarmProviders/update-state", HttpMethod.Put,
                    input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> Delete(
            [FromQuery] DeleteProviderEcofarmDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/EcoFarmProviders/delete", HttpMethod.Delete, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
    }
}