using System;
using System.Threading.Tasks;
using Abp.Runtime.Session;
using System.Net.Http;
using Abp.Application.Services;
using Abp.Web.Models;
using Yootek.Common.DataResult;
using Yootek.Yootek.Services.Yootek.Payments.Dto;
using Yootek.Services.SmartSocial.Ecofarm;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Yootek.Yootek.Services.Yootek.Payments
{
    public interface IPaymentAppService : IApplicationService
    {
        Task<DataResultT<object>> Create(CreatePaymentDto input);
        Task<DataResultT<object>> GetList(GetListPaymentDto input);
        Task<DataResultT<object>> GetListMomoTenant();
        Task<DataResultT<object>> GetMomoTenant(int id);
        Task<DataResultT<object>> CreateMomoTenant(CreateMomoTenantDto input);
        Task<DataResultT<object>> UpdateMomoTenant(UpdateMomoTenantDto input);
        Task<DataResultT<object>> DeleteMomoTenant(int id);
    }

    public class PaymentAppService : YootekAppServiceBase, IApplicationService
    {
        private readonly BaseHttpClient _httpClient;

        #region Payment

        public PaymentAppService(IAbpSession abpSession, IConfiguration configuration)
        {
            _httpClient = new BaseHttpClient(abpSession, configuration["ApiSettings:Payments"]);
        }

        public async Task<DataResultT<object>> Create(CreatePaymentDto input)
        {
            return await _httpClient.SendSync<object>("/api/payments/create", HttpMethod.Post, input);
        }

        public async Task<DataResult> GetList(GetListPaymentDto input)
        {
            // Convert input to camelCase
            var json = JsonConvert.SerializeObject(input, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            var x = JsonConvert.DeserializeObject<object>(json);

            var result =
                await _httpClient.SendSync<GetListPaymentListResultDto>("/api/payments/list", HttpMethod.Get, x);

            return DataResult.ResultSuccess(result.Data.Items, "success", result.Data.TotalCount);
        }

        [DontWrapResult]
        public async Task<object> MomoIpn(MomoIpnInputDto input)
        {
            try
            {
                await _httpClient.SendSync<object>("/api/payments/momo-ipn", HttpMethod.Post, input);

                return null;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return null;
            }
        }

        [DontWrapResult]
        public async Task<string> OnepayIpn(MomoIpnInputDto input)
        {
            try
            {
                await _httpClient.SendSync<object>("/api/payments/onepay-ipn", HttpMethod.Get, input);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }

            return "responsecode=1&desc=confirm-success";
        }

        #endregion

        #region Momo tenant

        public async Task<DataResultT<object>> GetListMomoTenant()
        {
            return await _httpClient.SendSync<object>("/api/momo-tenants/list-of-tenant", HttpMethod.Get);
        }

        public async Task<DataResultT<object>> GetMomoTenant(int id)
        {
            return await _httpClient.SendSync<object>($"/api/momo-tenants/{id}", HttpMethod.Get);
        }

        public async Task<DataResultT<object>> CreateMomoTenant(CreateMomoTenantDto input)
        {
            return await _httpClient.SendSync<object>("/api/momo-tenants", HttpMethod.Post, input);
        }

        public async Task<DataResultT<object>> UpdateMomoTenant(UpdateMomoTenantDto input)
        {
            return await _httpClient.SendSync<object>($"/api/momo-tenants/{input.Id}", HttpMethod.Put, input);
        }

        public async Task<DataResultT<object>> DeleteMomoTenant(int id)
        {
            return await _httpClient.SendSync<object>($"/api/momo-tenants/{id}", HttpMethod.Delete);
        }

        #endregion
    }
}