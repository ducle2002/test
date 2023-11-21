using Abp.Runtime.Session;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Policy;
using System.Threading.Tasks;
using IMAX.App.ServiceHttpClient.Dto;

namespace IMAX.App.ServiceHttpClient
{
    public interface IHttpOrderService
    {
        Task<List<OrderDto>> GetAllOrders(GetOrdersDto input);
    }

    public class HttpOrderService : IHttpOrderService
    {
        private readonly HttpClient _client;
        private readonly IAbpSession _session;
        public HttpOrderService(HttpClient client, IAbpSession session)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _session = session ?? throw new ArgumentNullException(nameof(_session));
        }

        public async Task<List<OrderDto>> GetAllOrders(GetOrdersDto input)
        {
            var query = input.GetStringQueryUri();
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/order-service/get-all-orders{query}"))
            {
                request.HandleGet(_session);
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<List<OrderDto>>();
            }

        }

        public async Task<OrderDto> CreateOrder(OrderDto input)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/order-service/create-order"))
            {
                request.HandlePostAsJson<OrderDto>(input, _session);
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<OrderDto>();
            }

        }
    }
}
