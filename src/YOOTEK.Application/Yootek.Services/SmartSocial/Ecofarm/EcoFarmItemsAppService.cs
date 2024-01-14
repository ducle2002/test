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
    public interface IEcoFarmItemsAppService : IApplicationService
    {
        Task<object> GetListByPartnerAsync([FromQuery] GetAllEcofarmItemsByPartnerDto input);
        Task<object> GetListByUserAsync([FromQuery] GetAllItemsByUserDto input);
        Task<object> GetByIdAsync([FromQuery] GetItemEcofarmByIdDto input);
        Task<object> CreateAsync([FromBody] CreateItemForEcoFarmDto input);
        Task<object> UpdateAsync([FromBody] UpdateItemForEcoFarmDto input);
        Task<object> UpdateStatusAsync([FromBody] UpdateItemStatusEcoFarmDto input);
        Task<object> DeleteAsync([FromQuery] DeleteItemForEcoFarmDto input);
        Task<object> GetCartAsync(GetCartForEcoFarmDto input);
        Task<object> UpdateCartAsync([FromBody] UpdateCartEcoFarmDto input);
        Task<object> AddItemModelToCartAsync([FromBody] AddItemModelToCartForEcoFarmDto input);
    }

    public class EcoFarmItemsAppService : YootekAppServiceBase, IEcoFarmItemsAppService
    {
        private readonly IAbpSession _abpSession;
        private readonly BaseHttpClient _httpClient;

        public EcoFarmItemsAppService(IAbpSession abpSession, IConfiguration configuration)
        {
            _abpSession = abpSession;
            _httpClient = new BaseHttpClient(abpSession, configuration["ApiSettings:Business.Item"]);
        }

        public async Task<object> GetListByPartnerAsync([FromQuery] GetAllEcofarmItemsByPartnerDto input)
        {
            try
            {
                var result = await _httpClient.SendSync<PagedResultDto<EcofarmItemDto>>(
                    "/api/v1/EcoFarmItems/partner/get-list", HttpMethod.Get, input);
                if (result.Success) return DataResult.ResultSuccess(result.Data.Items, "", result.Data.TotalCount);
                return result;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> GetListByUserAsync([FromQuery] GetAllItemsByUserDto input)
        {
            try
            {
                var result = await _httpClient.SendSync<PagedResultDto<EcofarmItemDto>>(
                    "/api/v1/EcoFarmItems/user/get-list",
                    HttpMethod.Get, input);
                if (result.Success) return DataResult.ResultSuccess(result.Data.Items, "", result.Data.TotalCount);
                return result;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> GetByIdAsync([FromQuery] GetItemEcofarmByIdDto input)
        {
            try
            {
                return await _httpClient.SendSync<object>("/api/v1/EcoFarmItems/get-detail", HttpMethod.Get,
                    input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> CreateAsync(
            [FromBody] CreateItemForEcoFarmDto input)
        {
            try
            {
                return await _httpClient.SendSync<long>("/api/v1/EcoFarmItems/create", HttpMethod.Post, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> UpdateAsync(
            [FromBody] UpdateItemForEcoFarmDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/EcoFarmItems/update", HttpMethod.Put, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> UpdateStatusAsync(
            [FromBody] UpdateItemStatusEcoFarmDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/EcoFarmItems/update-status", HttpMethod.Put, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> DeleteAsync(
            [FromQuery] DeleteItemForEcoFarmDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/EcoFarmItems/delete", HttpMethod.Delete, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> GetCartAsync(GetCartForEcoFarmDto input)
        {
            try
            {
                return await _httpClient.SendSync<PagedResultDto<CartItemModel>>("/api/v1/EcoFarmItems/get-cart",
                    HttpMethod.Get, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> UpdateCartAsync(
            [FromBody] UpdateCartEcoFarmDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/EcoFarmItems/update-cart", HttpMethod.Put, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> AddItemModelToCartAsync(
            [FromBody] AddItemModelToCartForEcoFarmDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/EcoFarmItems/add-item-model-to-cart", HttpMethod.Put,
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