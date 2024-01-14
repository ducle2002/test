using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Abp.Runtime.Session;
using Yootek.Common.DataResult;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Yootek.Services.SmartSocial.Ecofarm
{
    public class BaseHttpClient
    {
        private readonly HttpClient _httpClient = new();

        public BaseHttpClient(IAbpSession session, string baseUrl = "")
        {
            _httpClient.BaseAddress = new Uri(baseUrl);

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var userId = session.UserId > 0 ? session.UserId.ToString() : "";
            var tenantId = session.TenantId > 0 ? session.TenantId.ToString() : "";

            _httpClient.DefaultRequestHeaders.Add("userId", userId);
            _httpClient.DefaultRequestHeaders.Add("tenantId", tenantId);
        }


        public HttpClient GetHttpClient()
        {
            return _httpClient;
        }

        private static string _getStringQueryUri(object obj)
        {
            try
            {
                var result = new List<string>();
                foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(obj))
                    if (property.GetValue(obj) != null)
                    {
                        if (property.PropertyType.IsGenericType &&
                            property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            var list = (IEnumerable)property.GetValue(obj);
                            result.AddRange(list.Cast<object>()
                                .Select((item, index) =>
                                    $"{property.Name}[{index}]={(item != null ? item.ToString() : string.Empty)}"));
                        }
                        else
                        {
                            // Handle other types
                            result.Add($"{property.Name}={property.GetValue(obj)}");
                        }
                    }

                return "?" + string.Join("&", result);
            }
            catch
            {
                return null;
            }
        }

        public async Task<DataResultT<T>> SendSync<T>(string path, HttpMethod method, object data = null)
        {
            HttpResponseMessage response;

            var jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            if (method == HttpMethod.Get)
            {
                var query = data == null ? "" : _getStringQueryUri(data);
                response = await _httpClient.GetAsync(path + query);
            }
            else if (method == HttpMethod.Post)
            {
                var content = JsonContent.Create(data);
                response = await _httpClient.PostAsync(path, content);
            }
            else if (method == HttpMethod.Put)
            {
                var content = JsonContent.Create(data);
                response = await _httpClient.PutAsync(path, content);
            }
            else if (method == HttpMethod.Delete)
            {
                var query = data == null ? "" : _getStringQueryUri(data);
                response = await _httpClient.DeleteAsync(path + query);
            }
            else
            {
                throw new Exception("Method not support");
            }


            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
                return DataResultT<T>.ResultSuccess(result, "Success");
            }
            else
            {
                var result = JsonConvert.DeserializeObject<object>(await response.Content.ReadAsStringAsync());
                return DataResultT<T>.ResultWithCode(result, "Error", (int)response.StatusCode);
            }
        }

        public async Task<DataResultT<T>> SendSync<T>(HttpRequestMessage message)
        {
            HttpResponseMessage response =  await _httpClient.SendAsync(message);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
                return DataResultT<T>.ResultSuccess(result, "Success");
            }
            else
            {
                var result = JsonConvert.DeserializeObject<object>(await response.Content.ReadAsStringAsync());
                return DataResultT<T>.ResultWithCode(result, "Error", (int)response.StatusCode);
            }
        }
    }
}