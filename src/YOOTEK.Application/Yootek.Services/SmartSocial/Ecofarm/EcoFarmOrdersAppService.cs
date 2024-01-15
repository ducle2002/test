using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services.Dto;
using Abp.Runtime.Session;
using Yootek.App.ServiceHttpClient.Dto.Business;
using Yootek.Common.DataResult;
using Yootek.Services.Notifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Yootek.Services.SmartSocial.Ecofarm
{
    public interface IEcoFarmOrderAppService
    {
        Task<object> GetListByPartnerAsync([FromQuery] GetListEcofarmOrdersByPartnerDto input);
        Task<object> GetListByUserAsync([FromQuery] GetListEcofarmOrdersByUserDto input);
        Task<object> GetByIdAsync([FromQuery] GetOrderEcofarmDto input);
        Task<object> CreateAsync([FromBody] CreateOrderEcofarmDto input);
        Task<object> UpdateAsync([FromBody] UpdateOrderEcofarmDto input);
        Task<object> RatingAsync([FromBody] RatingOrderEcofarmDto input);
        Task<object> DeleteAsync([FromQuery] DeleteOrderEcofarmDto input);
        Task<object> GetRevenueAsync([FromQuery] GetRevenueEcoFarmDto input);
        Task<object> GetCountAsync([FromQuery] GetCountOrderEcoFarmsDto input);
        Task<object> GetStatisticAsync([FromQuery] GetStatisticDto input);
        Task<object> GetStatisticSomeProvidersAsync([FromQuery] GetStatisticSomeProviderDto input);
        Task<object> GetItemRankingAsync([FromQuery] GetItemRankingDto input);
        Task<object> ConfirmByPartnerAsync([FromBody] ConfirmOrderByPartnerDto input);
        Task<object> RefuseByPartnerAsync([FromBody] RefuseOrderByPartnerDto input);
        Task<object> CancelByPartnerAsync([FromBody] CancelOrderDto input);
        Task<object> ConfirmByUserAsync(long id);
        Task<object> CancelByUserAsync([FromBody] CancelOrderDto input);
    }

    public class EcoFarmOrdersAppService : YootekAppServiceBase, IEcoFarmOrderAppService
    {
        private readonly IAppNotifyBusiness _appNotifyBusiness;
        private readonly BaseHttpClient _httpClient;

        public EcoFarmOrdersAppService(
            IAppNotifyBusiness appNotifyBusiness,
            IAbpSession abpSession,
            IConfiguration configuration
        )
        {
            _appNotifyBusiness = appNotifyBusiness;
            _httpClient = new BaseHttpClient(abpSession, configuration["ApiSettings:Business.Order"]);
        }

        public async Task<object> GetListByPartnerAsync(
            [FromQuery] GetListEcofarmOrdersByPartnerDto input)
        {
            try
            {
                var result = await _httpClient.SendSync<PagedResultDto<OrderDetailDto>>(
                    "/api/v1/EcoFarmOrders/partner/get-list", HttpMethod.Get, input);
                if (result.Success) return DataResult.ResultSuccess(result.Data.Items, "", result.Data.TotalCount);
                return result;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> GetListByUserAsync([FromQuery] GetListEcofarmOrdersByUserDto input)
        {
            try
            {
                var result = await _httpClient.SendSync<PagedResultDto<OrderDetailDto>>(
                    "/api/v1/EcoFarmOrders/user/get-list",
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

        public async Task<object> GetByIdAsync([FromQuery] GetOrderEcofarmDto input)
        {
            try
            {
                return await _httpClient.SendSync<OrderDetailDto>("/api/v1/EcoFarmOrders/get-detail", HttpMethod.Get,
                    input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> GetRevenueAsync([FromQuery] GetRevenueEcoFarmDto input)
        {
            try
            {
                return await _httpClient.SendSync<List<double>>("/api/v1/EcoFarmOrders/revenue", HttpMethod.Get,
                    input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> GetCountAsync([FromQuery] GetCountOrderEcoFarmsDto input)
        {
            try
            {
                return await _httpClient.SendSync<CountOrderEcoFarmsDto>("/api/v1/EcoFarmOrders/count", HttpMethod.Get,
                    input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> GetStatisticAsync([FromQuery] GetStatisticDto input)
        {
            try
            {
                return await _httpClient.SendSync<StatisticOrderEcoFarmDto>("/api/v1/EcoFarmOrders/statistic",
                    HttpMethod.Get,
                    input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> GetStatisticSomeProvidersAsync([FromQuery] GetStatisticSomeProviderDto input)
        {
            try
            {
                return await _httpClient.SendSync<StatisticSomeProviderOrderEcoFarmDto>(
                    "/api/v1/EcoFarmOrders/statistic-some-providers", HttpMethod.Get,
                    input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> GetItemRankingAsync([FromQuery] GetItemRankingDto input)
        {
            try
            {
                var result = await _httpClient.SendSync<PagedResultDto<ItemRankingEcoFarmDto>>(
                    "/api/v1/EcoFarmOrders/item-ranking", HttpMethod.Get,
                    input);
                if (result.Success) return DataResult.ResultSuccess(result.Data.Items, "", result.Data.TotalCount);
                return result;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> CreateAsync([FromBody] CreateOrderEcofarmDto input)
        {
            try
            {
                return await _httpClient.SendSync<long>("/api/v1/EcoFarmOrders/create", HttpMethod.Post, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> UpdateAsync([FromBody] UpdateOrderEcofarmDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/EcoFarmOrders/update", HttpMethod.Put, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> RatingAsync([FromBody] RatingOrderEcofarmDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/EcoFarmOrders/rating", HttpMethod.Put, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> DeleteAsync([FromQuery] DeleteOrderEcofarmDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/EcoFarmOrders/delete", HttpMethod.Delete, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> ConfirmByPartnerAsync([FromBody] ConfirmOrderByPartnerDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/EcoFarmOrders/partner/confirm", HttpMethod.Post,
                    input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> RefuseByPartnerAsync([FromBody] RefuseOrderByPartnerDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/EcoFarmOrders/partner/refuse", HttpMethod.Post, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> CancelByPartnerAsync([FromBody] CancelOrderDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/EcoFarmOrders/partner/cancel", HttpMethod.Post, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> ConfirmByUserAsync(long id)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/EcoFarmOrders/user/confirm", HttpMethod.Post,
                    new { Id = id });
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> CancelByUserAsync([FromBody] CancelOrderDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/EcoFarmOrders/user/cancel", HttpMethod.Post, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
    }
}