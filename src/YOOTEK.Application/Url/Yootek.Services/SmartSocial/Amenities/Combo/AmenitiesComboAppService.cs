using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Runtime.Session;
using Abp.UI;
using Yootek.Common.DataResult;
using Yootek.Services.SmartSocial.Advertisements;
using Yootek.Services.SmartSocial.Ecofarm;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Yootek.Services.SmartSocial
{
    public interface IAmenitiesComboAppService : IApplicationService
    {
        Task<object> GetAll(GetAllAmenitiesComboInput input);
        Task<object> GetById(long id);
        Task<object> Create([FromBody] AmenitiesComboDto input);
        Task<object> Update([FromBody] AmenitiesComboDto input);
        Task<object> Delete(long id);
        Task<object> DeleteMany([FromBody] List<long> ids);
    }

    public class AmenitiesComboAppService : YootekAppServiceBase, IAmenitiesComboAppService
    {
        private readonly IAbpSession _abpSession;
        private readonly BaseHttpClient _httpClient;
        private string baseUrl = "api/v1/AmenitiesCombo/";

        public AmenitiesComboAppService(IAbpSession abpSession, IConfiguration configuration)
        {
            _abpSession = abpSession;
            _httpClient = new BaseHttpClient(abpSession, configuration["ApiSettings:Business.Item"]);
        }

        public async Task<object> GetAll(GetAllAmenitiesComboInput input)
        {
            try
            {
                var result = await _httpClient.SendSync<PagedResultDto<AmenitiesComboDto>>(baseUrl + "get-all", HttpMethod.Get, input);
                if (result.Success) return DataResult.ResultSuccess(result.Data.Items, "", result.Data.TotalCount);
                return result;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetById(long id)
        {
            var result = await _httpClient.SendSync<AmenitiesComboDto>(baseUrl + $"get-by-id?id={id}",
                     HttpMethod.Get);
            return result;
        }

        public async Task<object> Create([FromBody]AmenitiesComboDto input)
        {
            try
            {
                return await _httpClient.SendSync<long>(baseUrl + "create", HttpMethod.Post,
                    input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> Update([FromBody]AmenitiesComboDto input)
        {
            try
            {
                return await _httpClient.SendSync<long>(baseUrl + "update", HttpMethod.Put,
                    input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> Delete(long id)
        {
            try
            {
                return await _httpClient.SendSync<bool>(baseUrl + $"delete?id={id}", HttpMethod.Delete);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> DeleteMany([FromBody]List<long> ids)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, baseUrl + $"delete-many");
                request.Content = new StringContent(JsonConvert.SerializeObject(ids), Encoding.UTF8, "application/json");

                return await _httpClient.SendSync<bool>(request);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

    }
}