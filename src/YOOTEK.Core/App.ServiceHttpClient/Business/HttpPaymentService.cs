using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Abp.Runtime.Session;
using Yootek.App.ServiceHttpClient.Dto;
using Yootek.App.ServiceHttpClient.Dto.Business;

namespace Yootek.App.ServiceHttpClient.Business
{
    public interface IHttpPaymentService
    {
        Task<MicroserviceResultDto<List<PaymentDto>>> GetListPayments(GetAllPaymentsDto input);
        Task<MicroserviceResultDto<PaymentDto>> GetPaymentDetail(GetPaymentDetailDto input);
        Task<string> CreatePaymentAsync(CreatePaymentDto input);
        Task<MicroserviceResultDto<bool>> DeletePayment(DeletePaymentDto input);
    }
    public class HttpPaymentService : IHttpPaymentService
    {
        private readonly HttpClient _client;
        private readonly IAbpSession _session;
        public HttpPaymentService(HttpClient client, IAbpSession session)
        {
            _client = client;
            _session = session;
        }
        public async Task<MicroserviceResultDto<List<PaymentDto>>> GetListPayments(GetAllPaymentsDto input)
        {
            var query = "api/v1/Payments/get-list" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<List<PaymentDto>>>();
        }
        public async Task<MicroserviceResultDto<PaymentDto>> GetPaymentDetail(GetPaymentDetailDto input)
        {
            var query = "api/v1/Payments/get-payment-detail" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PaymentDto>>();
        }
        public async Task<string> CreatePaymentAsync(CreatePaymentDto input)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/Payments/create"))
            {
                request.HandlePostAsJson(input, _session);
                var response = await _client.SendAsync(request);
                MicroserviceResultDto<string> responsePayment = await response.ReadContentAs<MicroserviceResultDto<string>>();
                return responsePayment?.Result;
            }
        }
        public async Task<MicroserviceResultDto<bool>> DeletePayment(DeletePaymentDto input)
        {
            var query = "api/v1/Payments/delete" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Delete, query);

            request.HandleDeleteAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
    }
}
