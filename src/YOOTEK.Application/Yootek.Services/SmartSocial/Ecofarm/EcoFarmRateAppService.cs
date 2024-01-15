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
    public interface IEcoFarmRatesAppService : IApplicationService
    {
        Task<object> GetListAsync([FromQuery] GetListRateEcoFarmsDto input);
        Task<object> GetDetailAsync([FromQuery] GetRateEcoFarmDetailDto input);
        Task<object> CreateAsync([FromBody] CreateRateEcoFarmDto input);
        Task<object> UpdateAsync([FromBody] UpdateRateEcoFarmDto input);
        Task<object> DeleteAsync([FromQuery] DeleteRateEcoFarmDto input);
        Task<object> DeleteManyAsync([FromQuery] DeleteManyRateEcoFarmDto input);
    }

    public class EcoFarmRatesAppService : YootekAppServiceBase, IEcoFarmRatesAppService
    {
        private readonly IAbpSession _abpSession;
        private readonly BaseHttpClient _httpClient;

        public EcoFarmRatesAppService(IAbpSession abpSession, IConfiguration configuration)
        {
            _abpSession = abpSession;
            _httpClient = new BaseHttpClient(abpSession, configuration["ApiSettings:Business.Rate"]);
        }

        public async Task<object> GetListAsync([FromQuery] GetListRateEcoFarmsDto input)
        {
            try
            {
                var result = await _httpClient.SendSync<PagedResultDto<RateEcoFarmDto>>(
                    "/api/v1/EcoFarmRates/get-list", HttpMethod.Get, input);
                if (result.Success) return DataResult.ResultSuccess(result.Data.Items, "", result.Data.TotalCount);
                return result;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> GetDetailAsync([FromQuery] GetRateEcoFarmDetailDto input)
        {
            try
            {
                return await _httpClient.SendSync<RateEcoFarmDto>("/api/v1/EcoFarmRates/get-detail", HttpMethod.Get,
                    input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> CreateAsync(
            [FromBody] CreateRateEcoFarmDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/EcoFarmRates/create", HttpMethod.Post, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> UpdateAsync(
            [FromBody] UpdateRateEcoFarmDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/EcoFarmRates/update", HttpMethod.Put, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }


        public async Task<object> DeleteAsync(
            [FromQuery] DeleteRateEcoFarmDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/EcoFarmRates/delete", HttpMethod.Delete, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        public async Task<object> DeleteManyAsync(
            [FromQuery] DeleteManyRateEcoFarmDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/EcoFarmRates/delete-many", HttpMethod.Delete, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
    }
}