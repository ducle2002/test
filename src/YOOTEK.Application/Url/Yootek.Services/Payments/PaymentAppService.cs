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

        #region Payment

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

        public async Task<DataResultT<PaymentDto>> Create(CreatePaymentDto input)
        {
            if (input.Type == EPaymentType.BILL)
            {
                var paymentBill = await _userBillPaymentAppService.RequestUserBillPayment(input);
                input.TransactionId = paymentBill.Id;
                try
                {
                    var response =
                        await _httpClient.SendSync<PaymentDto>("/api/payments/create", HttpMethod.Post, input);
                    if (!response.Success) await _handlePaymentUtilAppService.HandleUserBillRecoverPayment(paymentBill);
                    return response;
                }
                catch (Exception e)
                {
                    await _handlePaymentUtilAppService.HandleUserBillRecoverPayment(paymentBill);
                    throw;
                }
            }
            else
            {
                return await _httpClient.SendSync<PaymentDto>("/api/payments/create", HttpMethod.Post, input);
            }
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

        #region Onepay merchant

        public async Task<DataResultT<object>> GetListOnepayMerchant(GetListOnepayMerchant input)
        {
            return await _httpClient.SendSync<object>("/api/onepay-merchants/list", HttpMethod.Get, input);
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
    }
}