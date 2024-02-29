using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Collections;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using System.Text;

namespace Yootek.Extensions
{
    public static class YootekHttpClientExtensions
    {

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

        private static async Task<HttpResponseMessage> ReponseMessageAsync(HttpClient httpClient, string path, HttpMethod method, object data = null)
        {
            HttpResponseMessage response;

            if (method == HttpMethod.Get)
            {
                var query = data == null ? "" : _getStringQueryUri(data);
                response = await httpClient.GetAsync(path + query);
            }
            else if (method == HttpMethod.Post)
            {
                var jsonContent = JsonConvert.SerializeObject(data);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                response = await httpClient.PostAsync(path, content);
            }
            else if (method == HttpMethod.Put)
            {
                var jsonContent = JsonConvert.SerializeObject(data);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                response = await httpClient.PutAsync(path, content);
            }
            else if (method == HttpMethod.Delete)
            {
                var query = data == null ? "" : _getStringQueryUri(data);
                response = await httpClient.DeleteAsync(path + query);
            }
            else
            {
                throw new Exception("Method not support");
            }

            return response;
        }

        public static async Task<ServiceResult<T>> SendAsync<T>(this HttpClient httpClient, string path, HttpMethod method, object data = null)
        {
          
            var response =  await ReponseMessageAsync(httpClient, path, method, data);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
                return ServiceResult<T>.Return(response, result);
            }
            else
            {
                return ServiceResult<T>.Return(response);
            }
        }

        public static async Task<ServiceResult<T>> SendAsync<T>(this HttpClient client, HttpRequestMessage message)
        {
            HttpResponseMessage response = await client.SendAsync(message);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
                return ServiceResult<T>.Return(response, result);
            }
            else
            {
                return ServiceResult<T>.Return(response);
            }
        }

    }

    public class ServiceResult<T> 
    {
        public T Data { get; set; }
        public HttpResponseMessage Response { get; set; }
        public bool Success { get; set; } = true;
        public static ServiceResult<T> Return(HttpResponseMessage response, T data)
        {
            var result = new ServiceResult<T>()
            {
                Data = data,
                Success = (int)response.StatusCode is >= 200 and < 300,
                Response = response
            };
            return result;
        }

        public static ServiceResult<T> Return(HttpResponseMessage response)
        {
            var result = new ServiceResult<T>()
            {
                Success = (int)response.StatusCode is >= 200 and < 300,
                Response = response
            };
            return result;
        }
    }
}
