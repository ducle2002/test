using Abp.Runtime.Session;
using IMAX.App.ServiceHttpClient.Dto;
using IMAX.App.ServiceHttpClient.Dto.Imax.Business;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace IMAX.App.ServiceHttpClient.Imax.Business
{
    public interface IHttpAdvertisementService
    {
        Task<MicroserviceResultDto<List<Advertisement>>> GetAllAdvertisementsByAdmin(GetAllAdvertisementsDto input);
        Task<MicroserviceResultDto<List<Advertisement>>> GetAllAdvertisementsByPartner(GetAllAdvertisementsDto input);
        Task<MicroserviceResultDto<List<Advertisement>>> GetAllAdvertisementsByUser(GetAllAdvertisementsDto input);
        Task<MicroserviceResultDto<bool>> CreateAdvertisement(CreateAdvertisementDto input);
        Task<MicroserviceResultDto<bool>> DeleteAdvertisement(DeleteAdvertisementDto input);
        Task<MicroserviceResultDto<bool>> ApprovalAdvertisement(ApprovalAdvertisementDto input);
    }
    public class HttpAdvertisementService : IHttpAdvertisementService
    {
        private readonly HttpClient _client;
        private readonly IAbpSession _session;
        public HttpAdvertisementService(HttpClient client, IAbpSession session)
        {
            _client = client;
            _session = session;
        }
        public async Task<MicroserviceResultDto<List<Advertisement>>> GetAllAdvertisementsByAdmin(GetAllAdvertisementsDto input)
        {
            var query = input.GetStringQueryUri();
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/advertisement/admin{query}"))
            {
                request.HandleGet(_session);
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<MicroserviceResultDto<List<Advertisement>>>();
            }
        }
        public async Task<MicroserviceResultDto<List<Advertisement>>> GetAllAdvertisementsByPartner(GetAllAdvertisementsDto input)
        {
            var query = input.GetStringQueryUri();
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/advertisement/partner{query}"))
            {
                request.HandleGet(_session);
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<MicroserviceResultDto<List<Advertisement>>>();
            }
        }
        public async Task<MicroserviceResultDto<List<Advertisement>>> GetAllAdvertisementsByUser(GetAllAdvertisementsDto input)
        {
            var query = input.GetStringQueryUri();
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/advertisement/user{query}"))
            {
                request.HandleGet(_session);
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<MicroserviceResultDto<List<Advertisement>>>();
            }
        }

        public async Task<MicroserviceResultDto<bool>> CreateAdvertisement(CreateAdvertisementDto input)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"api/v1/advertisement"))
            {
                request.HandlePostAsJson<CreateAdvertisementDto>(input, _session);
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<MicroserviceResultDto<bool>>();
            }
        }
        public async Task<MicroserviceResultDto<bool>> DeleteAdvertisement(DeleteAdvertisementDto input)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Delete, $"api/v1/advertisement/delete-advertisement"))
            {
                request.HandleDeleteAsJson<DeleteAdvertisementDto>(input, _session);
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<MicroserviceResultDto<bool>>();
            }
        }
        public async Task<MicroserviceResultDto<bool>> ApprovalAdvertisement(ApprovalAdvertisementDto input)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"api/v1/advertisement/update-status"))
            {
                request.HandlePostAsJson<ApprovalAdvertisementDto>(input, _session);
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<MicroserviceResultDto<bool>>();
            }
        }
    }
}
