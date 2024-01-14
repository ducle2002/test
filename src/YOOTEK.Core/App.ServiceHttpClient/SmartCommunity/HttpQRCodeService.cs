using Abp.Application.Services.Dto;
using Abp.Runtime.Session;
using Yootek.App.ServiceHttpClient.Dto;
using Yootek.App.ServiceHttpClient.Dto.Yootek.SmartCommunity;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Yootek.App.ServiceHttpClient
{
    public interface IHttpQRCodeService
    {
        Task<MicroserviceResultDto<PagedResultDto<QRObjectDto>>> GetListQRObject(GetListQRObjectDto input);
        Task<MicroserviceResultDto<Dictionary<string, QRObjectDto>>> GetListQRObjectByListCode(GetListQRObjectByListCodeInput input);
        Task<MicroserviceResultDto<bool>> CreateQRObject(CreateQRObjectDto input);
        Task<MicroserviceResultDto<QRObjectDto>> GetQRObjectByCode(GetQRObjectByCodeDto input);
        Task<MicroserviceResultDto<bool>> UpdateQRObject(UpdateQRObjectDto input);
        Task<MicroserviceResultDto<bool>> DeleteQRObject(DeleteQRObjectDto input);
    }

    public class HttpQRCodeService : IHttpQRCodeService
    {
        private readonly HttpClient _client;
        private readonly IAbpSession _session;

        public HttpQRCodeService(HttpClient client, IAbpSession session)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<MicroserviceResultDto<PagedResultDto<QRObjectDto>>> GetListQRObject(GetListQRObjectDto input)
        {
            var query = $"/api/v1/qrcode/get-list-qrobject" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<QRObjectDto>>>();
        }

        public async Task<MicroserviceResultDto<Dictionary<string, QRObjectDto>>> GetListQRObjectByListCode(GetListQRObjectByListCodeInput input)
        {
            var query = input.GetStringQueryUri();
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/qrcode/get-list-qrobject-by-listcode{query}"))
            {
                request.HandleGet(_session);
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<MicroserviceResultDto<Dictionary<string, QRObjectDto>>>();
            }

        }

        public async Task<MicroserviceResultDto<bool>> CreateQRObject(CreateQRObjectDto input)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/qrcode/create-qrobject"))
            {
                request.HandlePostAsJson(input, _session);
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<MicroserviceResultDto<bool>>();
            }


        }

        public async Task<MicroserviceResultDto<QRObjectDto>> GetQRObjectByCode(GetQRObjectByCodeDto input)
        {
            var query = input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/qrcode/get-qrobject-by-code{query}");

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<QRObjectDto>>();
        }

        public async Task<MicroserviceResultDto<bool>> UpdateQRObject(UpdateQRObjectDto input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/qrcode/update-qrobject");

            request.HandlePutAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }

        public async Task<MicroserviceResultDto<bool>> DeleteQRObject(DeleteQRObjectDto input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/qrcode/delete-qrobject");

            request.HandleDeleteAsJson<DeleteQRObjectDto>(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
    }
}