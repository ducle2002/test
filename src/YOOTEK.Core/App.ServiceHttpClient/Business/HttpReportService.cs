using Abp.Application.Services.Dto;
using Abp.Runtime.Session;
using Yootek.App.ServiceHttpClient.Dto;
using Yootek.App.ServiceHttpClient.Dto.Business;
using System.Net.Http;
using System.Threading.Tasks;

namespace Yootek.App.ServiceHttpClient.Business
{
    public interface IHttpReportService
    {
        Task<MicroserviceResultDto<bool>> CreateReport(CreateReportDto input);
        Task<MicroserviceResultDto<PagedResultDto<JoinReportProvider>>> GetAllReportsByAdmin(GetAllReportsByAdminDto input);
        Task<MicroserviceResultDto<bool>> UpdateStateReport(UpdateStateReportDto input);
        Task<MicroserviceResultDto<bool>> DeleteReport(DeleteReportDto input);
    }
    public class HttpReportService : IHttpReportService
    {
        private readonly HttpClient _client;
        private readonly IAbpSession _session;

        public HttpReportService(HttpClient client, IAbpSession session)
        {
            _client = client;
            _session = session;
        }

        public async Task<MicroserviceResultDto<PagedResultDto<JoinReportProvider>>> GetAllReportsByAdmin(GetAllReportsByAdminDto input)
        {
            var query = "api/v1/Report/GetListReportByAdmin" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<JoinReportProvider>>>();
        }
        public async Task<MicroserviceResultDto<bool>> CreateReport(CreateReportDto input)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/Report/CreateReport"))
            {
                request.HandlePostAsJson(input, _session);
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<MicroserviceResultDto<bool>>();
            }
        }
        public async Task<MicroserviceResultDto<bool>> UpdateStateReport(UpdateStateReportDto input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/Report/UpdateStateReport");

            request.HandlePutAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        public async Task<MicroserviceResultDto<bool>> DeleteReport(DeleteReportDto input)
        {
            var query = "/api/v1/Report/DeleteReport" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Delete, query);

            request.HandleDeleteAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
    }
}
