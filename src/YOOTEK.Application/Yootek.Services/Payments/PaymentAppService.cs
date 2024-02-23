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
using Yootek.Services;
using Yootek.App.ServiceHttpClient.Dto.Business;
using Yootek.Yootek.Services.Yootek.SmartCommunity.CitizenFee.Payment;

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
        private readonly IUserBillPaymentAppService _userBillPaymentAppService;
        private readonly HandlePaymentUtilAppService _handlePaymentUtilAppService;


        public PaymentAppService(
            IAbpSession abpSession,
            IConfiguration configuration,
            IUserBillPaymentAppService userBillPaymentAppService,
            HandlePaymentUtilAppService handlePaymentUtilAppService
        )
        {
            _httpClient = new BaseHttpClient(abpSession, configuration["ApiSettings:Payments"]);
            _userBillPaymentAppService = userBillPaymentAppService;
            _handlePaymentUtilAppService = handlePaymentUtilAppService;
        }

        #region Payment

        public async Task<DataResultT<PaymentDto>> Create(CreatePaymentDto input)
        {

            return await _httpClient.SendSync<PaymentDto>("/api/payments/create", HttpMethod.Post, input);
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

        [HttpPut]
        public async Task<DataResult> UpdateManuallyVerified(UpdateManuallyVerifiedDto input)
        {
            var result =
                await _httpClient.SendSync<PaymentDto>("/api/payments/manually-verified", HttpMethod.Put, input);

            return DataResult.ResultSuccess(result.Data, "success");
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
        public async Task<string> OnepayIpn(OnepayIpnInputDto input)
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

        [HttpGet]
        public async Task<DataResultT<object>> AdminGetAllMomoTenants(int? tenantId)
        {
            return await _httpClient.SendSync<object>("/api/momo-tenants/admin", HttpMethod.Get, new { tenantId });
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

        #region Onepay merchant

        [HttpGet]
        public async Task<DataResultT<object>> AdminGetListOnepayMerchant(GetListOnepayMerchant input)
        {
            // Convert input to camelCase
            var json = JsonConvert.SerializeObject(input, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            var query = JsonConvert.DeserializeObject<object>(json);
            return await _httpClient.SendSync<object>("/api/onepay-merchants/admin", HttpMethod.Get, query);
        }

        public async Task<DataResultT<object>> GetListOnepayMerchantOfTenant()
        {
            return await _httpClient.SendSync<object>("/api/onepay-merchants/list-of-tenant", HttpMethod.Get);
        }

        public async Task<DataResultT<object>> GetOnepayMerchant(int id)
        {
            return await _httpClient.SendSync<object>($"/api/onepay-merchants/{id}", HttpMethod.Get);
        }

        public async Task<DataResultT<object>> CreateOnepayMerchant(CreateOnepayMerchantDto input)
        {
            return await _httpClient.SendSync<object>("/api/onepay-merchants", HttpMethod.Post, input);
        }

        public async Task<DataResultT<object>> UpdateOnepayMerchant(UpdateOnepayMerchantDto input)
        {
            return await _httpClient.SendSync<object>($"/api/onepay-merchants/{input.Id}", HttpMethod.Put, input);
        }

        public async Task<DataResultT<object>> DeleteOnepayMerchant(int id)
        {
            return await _httpClient.SendSync<object>($"/api/onepay-merchants/{id}", HttpMethod.Delete);
        }

        #endregion

        #region Onepay Verifications

        public async Task<DataResult> GetOnepayVerificationByPaymentId(string paymentId)
        {
            try
            {
                var result = await _httpClient.SendSync<OnepayVerificationDto>(
                    $"/api/onepay-verifications/get-by-payment-id/{paymentId}",
                    HttpMethod.Get
                );

                return DataResult.ResultSuccess(result.Data, "success");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        #endregion
    }
}