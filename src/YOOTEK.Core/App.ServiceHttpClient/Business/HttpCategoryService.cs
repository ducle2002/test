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
    public interface IHttpCategoryService
    {
        Task<MicroserviceResultDto<PagedResultDto<CategoryDto>>> GetListCategory(GetAllCategoriesDto input);
        Task<MicroserviceResultDto<PagedResultDto<CategoryDto>>> GetListCategoryForEcoFarm(GetAllCategoriesDto input);
        Task<MicroserviceResultDto<CategoryDto>> GetCategoryById(long id);
        Task<MicroserviceResultDto<List<CategoryDto>>> GetListCategoryFromChildren(GetListCategoryFromChildrenDto input);
        Task<MicroserviceResultDto<bool>> CreateCategory(CreateCategoryDto input);
        Task<MicroserviceResultDto<bool>> UpdateCategory(UpdateCategoryDto input);
        Task<MicroserviceResultDto<bool>> DeleteCategory(DeleteCategoryDto input);
    }
    public class HttpCategoryService : IHttpCategoryService
    {
        private readonly HttpClient _client;
        private readonly IAbpSession _session;

        public HttpCategoryService(HttpClient client, IAbpSession session)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        #region CategoryService
        public async Task<MicroserviceResultDto<PagedResultDto<CategoryDto>>> GetListCategory(GetAllCategoriesDto input)
        {
            var query = "api/v1/Categories/get-list" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<CategoryDto>>>();
        }
        public async Task<MicroserviceResultDto<CategoryDto>> GetCategoryById(long id)
        {
            var query = "api/v1/Categories/get-detail?id=" + id;
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<CategoryDto>>();
        }
        public async Task<MicroserviceResultDto<List<CategoryDto>>> GetListCategoryFromChildren(GetListCategoryFromChildrenDto input)
        {
            var query = "api/v1/Categories/get-list-from-children" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<List<CategoryDto>>>();
        }
        public async Task<MicroserviceResultDto<PagedResultDto<CategoryDto>>> GetListCategoryForEcoFarm(GetAllCategoriesDto input)
        {
            var query = "api/v1/Categories/ecofarm/get-list" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<CategoryDto>>>();
        }
        public async Task<MicroserviceResultDto<bool>> CreateCategory(CreateCategoryDto input)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/Categories/create"))
            {
                request.HandlePostAsJson(input, _session);
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<MicroserviceResultDto<bool>>();
            }
        }
        public async Task<MicroserviceResultDto<bool>> UpdateCategory(UpdateCategoryDto input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/Categories/update");

            request.HandlePutAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        public async Task<MicroserviceResultDto<bool>> DeleteCategory(DeleteCategoryDto input)
        {
            var query = "api/v1/Categories/delete" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Delete, query);

            request.HandleDeleteAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        #endregion
    }
}
