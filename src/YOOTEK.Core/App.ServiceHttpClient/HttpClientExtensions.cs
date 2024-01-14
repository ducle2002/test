using Abp.Runtime.Session;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace Yootek.App.ServiceHttpClient
{
    public static class HttpClientExtensions
    {
        public static async Task<T> ReadContentAs<T>(this HttpResponseMessage response)
        {
            try
            {
                if (!response.IsSuccessStatusCode)
                    throw new ApplicationException($"Something went wrong calling the API: {response.ReasonPhrase}");

                var dataAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                return JsonSerializer.Deserialize<T>(dataAsString,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                return default;
            }
        }

        public static void HandleGet(this HttpRequestMessage request, IAbpSession _appSession)
        {
            request.Headers.AddSessionHeader(_appSession);
        }

        public static void HandlePostAsJson<T>(this HttpRequestMessage request, T data, IAbpSession _appSession)
        {
            var dataAsString = JsonSerializer.Serialize(data);
            var content = new StringContent(dataAsString);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            request.Headers.AddSessionHeader(_appSession);
            request.Content = content;
        }

        public static void HandlePutAsJson<T>(this HttpRequestMessage request, T data, IAbpSession _appSession)
        {
            var dataAsString = JsonSerializer.Serialize(data);
            var content = new StringContent(dataAsString);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            request.Headers.AddSessionHeader(_appSession);
            request.Content = content;
        }

        public static void HandleDelete<T>(this HttpRequestMessage request, T data, IAbpSession _appSession)
        {
            request.Headers.AddSessionHeader(_appSession);
        }

        public static void HandleDeleteAsJson<T>(this HttpRequestMessage request, T data, IAbpSession _appSession)
        {
            var dataAsString = JsonSerializer.Serialize(data);
            var content = new StringContent(dataAsString);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            request.Headers.AddSessionHeader(_appSession);
            request.Content = content;
        }

        public static void HandleMultiDelete<T>(this HttpRequestMessage request, T data, IAbpSession _appSession)
        {
            var dataAsString = JsonSerializer.Serialize(data);
            var content = new StringContent(dataAsString);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            request.Headers.AddSessionHeader(_appSession);
            request.Content = content;
        }

        public static string GetStringQueryUri(this object obj)
        {
            try
            {
                var result = new List<string>();
                foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(obj))
                {
                    if (property.GetValue(obj) != null && property.PropertyType == typeof(List<string>))
                    {
                        foreach (var str in (List<string>)property.GetValue(obj))
                        {
                            if (!string.IsNullOrEmpty(str)) result.Add(property.Name + "=" + str);
                        }
                    }
                    else if (property.GetValue(obj) != null)
                    {
                        result.Add(property.Name + "=" + property.GetValue(obj));
                    }
                }

                return "?" + string.Join("&", result);
            }
            catch
            {
                return null;
            }
        }

        public static void AddSessionHeader(this HttpRequestHeaders headers, IAbpSession _appSession)
        {
            var userId = _appSession.UserId > 0 ? _appSession.UserId.ToString() : "";
            var tenantId = _appSession.TenantId > 0 ? _appSession.TenantId.ToString() : "";

            headers.Add("userId", userId);
            headers.Add("tenantId", tenantId);
        }
    }
}