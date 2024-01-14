using Abp.Dependency;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Yootek.Lib.CrudBase
{
    public interface IYootekHttpClient : ISingletonDependency
    {
        HttpClient GetHttpClient();
        HttpClient GetHttpClient(string baseUrl);
    }
    public class YootekHttpClient: IYootekHttpClient
    {
        private HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public YootekHttpClient(
            IHttpContextAccessor httpContextAccessor
            )
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public HttpClient GetHttpClient()
        {
            return new HttpClient();
        }

        public HttpClient GetHttpClient(string baseUrl)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.BaseAddress = new Uri(baseUrl);
            if (_httpContextAccessor.HttpContext == null) return _httpClient;

            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if(string.IsNullOrEmpty(token)) return _httpClient;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return _httpClient;
        }

    }
}
