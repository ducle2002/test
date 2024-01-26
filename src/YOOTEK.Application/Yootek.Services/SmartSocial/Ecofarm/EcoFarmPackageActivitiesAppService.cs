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
    public interface IEcoFarmPackageActivitiesAppService : IApplicationService
    {
        Task<object> GetListByPartner([FromQuery] GetAllEcofarmPackageActivitiesDto input);
        Task<object> GetListByUser([FromQuery] GetAllEcofarmPackageActivitiesDto input);
        Task<object> GetById([FromQuery] GetItemEcofarmByIdDto input);
        Task<object> Create([FromBody] CreateEcofarmPackageActivitiesDto input);
        Task<object> Update([FromBody] UpdateEcofarmPackageActivityDto input);
        Task<object> UpdateStatus([FromBody] UpdateStatusEcofarmPackageActivityDto input);
        Task<object> Delete([FromQuery] DeleteEcofarmPackageActivityDto input);
    }

    public class EcoFarmPackageActivitiesAppService : YootekAppServiceBase, IEcoFarmPackageActivitiesAppService
    {
        private readonly IAbpSession _abpSession;
        private readonly BaseHttpClient _httpClient;

        public EcoFarmPackageActivitiesAppService(IAbpSession abpSession, IConfiguration configuration)
        {
            _abpSession = abpSession;
            _httpClient = new BaseHttpClient(abpSession, configuration["ApiSettings:Business.Item"]);
        }

        public async Task<object> GetListByPartner([FromQuery] GetAllEcofarmPackageActivitiesDto input)
        {
            try
            {
                var result = await _httpClient.SendSync<PagedResultDto<EcofarmPackageActivityDto>>(
                    "/api/v1/EcoFarmPackageActivities/partner/get-list", HttpMethod.Get, input);
                if (result.Success) return DataResult.ResultSuccess(result.Data.Items, "", result.Data.TotalCount);
                return result;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> GetListByUser(GetAllEcofarmPackageActivitiesDto input)
        {
            try
            {
                var result = await _httpClient.SendSync<PagedResultDto<EcofarmPackageActivityDto>>(
                    "/api/v1/EcoFarmPackageActivities/user/get-list", HttpMethod.Get, input);
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
                return await _httpClient.SendSync<EcofarmPackageActivityDto>(
                    "/api/v1/EcoFarmPackageActivities/get-detail",
                    HttpMethod.Get, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> Create(
            [FromBody] CreateEcofarmPackageActivitiesDto input)
        {
            try
            {
                return await _httpClient.SendSync<long>("/api/v1/EcoFarmPackageActivities/create", HttpMethod.Post,
                    input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> Update(
            [FromBody] UpdateEcofarmPackageActivityDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/EcoFarmPackageActivities/update", HttpMethod.Put,
                    input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> UpdateStatus(
            [FromBody] UpdateStatusEcofarmPackageActivityDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/EcoFarmPackageActivities/update-status",
                    HttpMethod.Put,
                    input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> Delete(
            [FromQuery] DeleteEcofarmPackageActivityDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/EcoFarmPackageActivities/delete", HttpMethod.Delete,
                    input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
    }
}