using Abp.Runtime.Session;
using Yootek.App.ServiceHttpClient.Dto.Business;
using System.Net.Http;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Yootek.Common.DataResult;
using Yootek.Services.SmartSocial.Ecofarm;
using Microsoft.Extensions.Configuration;

namespace Yootek.App.ServiceHttpClient.Business
{
    public interface IHttpAdvertisementService
    {
        Task<IDataResultT<PagedResultDto<AdvertisementDto>>> GetListByAdminAsync(GetAllAdvertisementsDto input);
        Task<IDataResultT<PagedResultDto<AdvertisementDto>>> GetListByPartnerAsync(GetAllAdvertisementsDto input);
        Task<IDataResultT<PagedResultDto<AdvertisementDto>>> GetListByUserAsync(GetAllAdvertisementsDto input);
        Task<IDataResultT<AdvertisementDto>> GetByIdAsync(GetAdvertisementByIdDto input);
        Task<object> CreateAsync(CreateAdvertisementDto input);
        Task<object> UpdateAsync(UpdateAdvertisementDto input);
        Task<object> DeleteAsync(DeleteAdvertisementDto input);
        Task<object> UpdateStatusAsync(UpdateStatusAdvertisementDto input);
    }
    public class HttpAdvertisementService : IHttpAdvertisementService
    {
        private readonly BaseHttpClient _advertisementClient;
        public HttpAdvertisementService(IAbpSession session, IConfiguration configuration)
        {
            _advertisementClient = new BaseHttpClient(session, configuration["ApiSettings:Business.Advertisement"]);
        }
        public async Task<IDataResultT<PagedResultDto<AdvertisementDto>>> GetListByAdminAsync(GetAllAdvertisementsDto input)
        {
            var result = await _advertisementClient.SendSync<PagedResultDto<AdvertisementDto>>("/api/v1/advertisements/admin/get-list", HttpMethod.Get, input);
            return result;
        }
        public async Task<IDataResultT<PagedResultDto<AdvertisementDto>>> GetListByPartnerAsync(GetAllAdvertisementsDto input)
        {
            var result = await _advertisementClient.SendSync<PagedResultDto<AdvertisementDto>>("/api/v1/advertisements/partner/get-list", HttpMethod.Get, input);
            return result;
        }
        public async Task<IDataResultT<PagedResultDto<AdvertisementDto>>> GetListByUserAsync(GetAllAdvertisementsDto input)
        {
            var result = await _advertisementClient.SendSync<PagedResultDto<AdvertisementDto>>("/api/v1/advertisements/user/get-list", HttpMethod.Get, input);
            return result;
        }
        public async Task<IDataResultT<AdvertisementDto>> GetByIdAsync(GetAdvertisementByIdDto input)
        {
            var result = await _advertisementClient.SendSync<AdvertisementDto>("/api/v1/advertisements/get-detail", HttpMethod.Get, input);
            return result;
        }

        public async Task<object> CreateAsync(CreateAdvertisementDto input)
        {
            var result = await _advertisementClient.SendSync<bool>("/api/v1/advertisements/create", HttpMethod.Post, input);
            return result;
        }
        
        public async Task<object> UpdateAsync(UpdateAdvertisementDto input)
        {
            var result = await _advertisementClient.SendSync<bool>("/api/v1/advertisements/update", HttpMethod.Put, input);
            return result;
        }
        public async Task<object> DeleteAsync(DeleteAdvertisementDto input)
        {
            var result = await _advertisementClient.SendSync<bool>("/api/v1/advertisements/delete", HttpMethod.Delete, input);
            return result;
        }
        public async Task<object> UpdateStatusAsync(UpdateStatusAdvertisementDto input)
        {
            var result = await _advertisementClient.SendSync<bool>("/api/v1/advertisements/update-status", HttpMethod.Put, input);
            return result;
        }
    }
}
