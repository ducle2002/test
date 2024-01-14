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
    public interface IEcoFarmPackagesAppService : IApplicationService
    {
        Task<object> GetListByPartner([FromQuery] GetAllEcofarmPackagesDto input);
        Task<object> GetListByUser([FromQuery] GetAllEcofarmPackagesDto input);
        Task<object> GetById([FromQuery] GetEcofarmPackageByIdDto input);
        Task<object> Create([FromBody] CreateEcofarmPackageDto input);
        Task<object> Update([FromBody] UpdateEcofarmPackageDto input);
        Task<object> UpdateStatus([FromBody] UpdateStatusEcofarmPackageDto input);
        Task<object> Delete([FromQuery] DeleteEcofarmPackageDto input);
    }

    public class EcoFarmPackagesAppService : YootekAppServiceBase, IEcoFarmPackagesAppService
    {
        private readonly IAbpSession _abpSession;
        private readonly BaseHttpClient _httpClient;

        public EcoFarmPackagesAppService(IAbpSession abpSession, IConfiguration configuration)
        {
            _abpSession = abpSession;
            _httpClient = new BaseHttpClient(abpSession, configuration["ApiSettings:Business.Item"]);
        }

        public async Task<object> GetListByPartner([FromQuery] GetAllEcofarmPackagesDto input)
        {
            try
            {
                var result = await _httpClient.SendSync<PagedResultDto<EcofarmPackageDto>>(
                    "/api/v1/EcoFarmPackages/partner/get-list", HttpMethod.Get, input);
                if (result.Success) return DataResult.ResultSuccess(result.Data.Items, "", result.Data.TotalCount);
                return result;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> GetListByUser(GetAllEcofarmPackagesDto input)
        {
            try
            {
                var result = await _httpClient.SendSync<PagedResultDto<EcofarmPackageDto>>(
                    "/api/v1/EcoFarmPackages/user/get-list", HttpMethod.Get, input);
                if (result.Success) return DataResult.ResultSuccess(result.Data.Items, "", result.Data.TotalCount);
                return result;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> GetById([FromQuery] GetEcofarmPackageByIdDto input)
        {
            try
            {
                return await _httpClient.SendSync<EcofarmPackageDto>("/api/v1/EcoFarmPackages/get-detail",
                    HttpMethod.Get, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> Create(
            [FromBody] CreateEcofarmPackageDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/EcoFarmPackages/create", HttpMethod.Post, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> Update(
            [FromBody] UpdateEcofarmPackageDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/EcoFarmPackages/update", HttpMethod.Put, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> UpdateStatus(
            [FromBody] UpdateStatusEcofarmPackageDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/EcoFarmPackages/update-status", HttpMethod.Put,
                    input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> Delete(
            [FromQuery] DeleteEcofarmPackageDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/EcoFarmPackages/delete", HttpMethod.Delete, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
    }
}