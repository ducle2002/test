using Abp.Application.Services.Dto;
using Abp.Runtime.Session;
using Yootek.App.ServiceHttpClient.Dto;
using Yootek.App.ServiceHttpClient.Dto.Business;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Yootek.App.ServiceHttpClient.Business
{
    public interface IHttpItemAttributeService
    {
        Task<MicroserviceResultDto<PagedResultDto<ItemAttributeDto>>> GetAllItemAttributes(GetAllItemAttributesDto input);
        Task<MicroserviceResultDto<ItemAttributeDto>> GetItemAttributeById(long id);
        Task<MicroserviceResultDto<bool>> CreateListItemAttributes(List<CreateItemAttributeDto> input);
        Task<MicroserviceResultDto<bool>> CreateItemAttribute(CreateItemAttributeDto input);
        Task<MicroserviceResultDto<bool>> UpdateItemAttribute(UpdateItemAttributeDto input);
        Task<MicroserviceResultDto<bool>> UpdateListItemAttributes(List<UpdateItemAttributeDto> input);
        Task<MicroserviceResultDto<bool>> DeleteItemAttribute(DeleteItemAttributeDto input);
    }
    public class HttpItemAttributeService : IHttpItemAttributeService
    {
        private readonly HttpClient _client;
        private readonly IAbpSession _session;

        public HttpItemAttributeService(HttpClient client, IAbpSession session)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        #region ItemAttributeService
        public async Task<MicroserviceResultDto<PagedResultDto<ItemAttributeDto>>> GetAllItemAttributes(GetAllItemAttributesDto input)
        {
            var query = "api/v1/ItemAttributes/get-all-item-attribute" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<ItemAttributeDto>>>();
        }
        public async Task<MicroserviceResultDto<ItemAttributeDto>> GetItemAttributeById(long id)
        {
            var query = "api/v1/ItemAttributes/get-item-attribute-by-id?id=" + id;
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<ItemAttributeDto>>();
        }
        public async Task<MicroserviceResultDto<bool>> CreateItemAttribute(CreateItemAttributeDto input)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/ItemAttributes/create-item-attribute"))
            {
                request.HandlePostAsJson(input, _session);
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<MicroserviceResultDto<bool>>();
            }
        }
        public async Task<MicroserviceResultDto<bool>> CreateListItemAttributes(List<CreateItemAttributeDto> input)
        {
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/ItemAttributes/create-list-item-attribute"))
                {
                    request.HandlePostAsJson(input, _session);
                    var response = await _client.SendAsync(request);
                    return await response.ReadContentAs<MicroserviceResultDto<bool>>();
                }
            }catch(Exception e)
            {
                throw e;
            }
        }
        public async Task<MicroserviceResultDto<bool>> UpdateItemAttribute(UpdateItemAttributeDto input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/ItemAttributes/update-item-attribute");
            request.HandlePutAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }

        public async Task<MicroserviceResultDto<bool>> UpdateListItemAttributes(List<UpdateItemAttributeDto> input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/ItemAttributes/update-list-item-attribute");
            request.HandlePutAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }

        public async Task<MicroserviceResultDto<bool>> DeleteItemAttribute(DeleteItemAttributeDto input)
        {
            var query = "api/v1/ItemAttributes/delete-item-attribute" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Delete, query);

            request.HandleDeleteAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        #endregion
    }
}
