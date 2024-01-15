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
    public interface IEcoFarmPaymentsAppService : IApplicationService
    {
        Task<object> GetListByPartner([FromQuery] GetAllEcofarmPaymentsDto input);
        Task<object> GetListByUser([FromQuery] GetAllEcofarmPaymentsDto input);
        Task<object> GetById([FromQuery] GetPaymentDetailDto input);
        Task<object> Create([FromBody] CreateEcofarmPaymentDto input);
        Task<object> Update([FromBody] UpdateEcofarmPaymentDto input);
        Task<object> UpdateState([FromBody] UpdateStateEcofarmPaymentDto input);
        Task<object> Delete([FromQuery] DeleteEcofarmPaymentDto input);
    }

    public class EcoFarmPaymentsAppService : YootekAppServiceBase, IEcoFarmPaymentsAppService
    {
        private readonly IAbpSession _abpSession;
        private readonly BaseHttpClient _httpClient;

        public EcoFarmPaymentsAppService(IAbpSession abpSession, IConfiguration configuration)
        {
            _abpSession = abpSession;
            _httpClient = new BaseHttpClient(abpSession, configuration["ApiSettings:Business.Item"]);
        }

        public async Task<object> GetListByPartner([FromQuery] GetAllEcofarmPaymentsDto input)
        {
            try
            {
                var result = await _httpClient.SendSync<PagedResultDto<EcofarmPaymentDto>>(
                    "/api/v1/EcoFarmPayments/partner/get-list", HttpMethod.Get, input);
                if (result.Success) return DataResult.ResultSuccess(result.Data.Items, "", result.Data.TotalCount);
                return result;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> GetListByUser(GetAllEcofarmPaymentsDto input)
        {
            try
            {
                var result = await _httpClient.SendSync<PagedResultDto<EcofarmPaymentDto>>(
                    "/api/v1/EcoFarmPayments/user/get-list", HttpMethod.Get, input);
                if (result.Success) return DataResult.ResultSuccess(result.Data.Items, "", result.Data.TotalCount);
                return result;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> GetById([FromQuery] GetPaymentDetailDto input)
        {
            try
            {
                return await _httpClient.SendSync<EcofarmPaymentDto>("/api/v1/EcoFarmPayments/get-detail",
                    HttpMethod.Get, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> Create(
            [FromBody] CreateEcofarmPaymentDto input)
        {
            try
            {
                return await _httpClient.SendSync<long>("/api/v1/EcoFarmPayments/create", HttpMethod.Post, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> Update(
            [FromBody] UpdateEcofarmPaymentDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/EcoFarmPayments/update", HttpMethod.Put, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> UpdateState(
            [FromBody] UpdateStateEcofarmPaymentDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/EcoFarmPayments/update-state", HttpMethod.Put,
                    input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> Delete(
            [FromQuery] DeleteEcofarmPaymentDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/EcoFarmPayments/delete", HttpMethod.Delete, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
    }
}