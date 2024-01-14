using Abp.Runtime.Session;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Yootek.App.ServiceHttpClient.Dto;
using System.Collections.Generic;
using Yootek.App.ServiceHttpClient.Dto.VietNamAdministrativeUnit;
using Microsoft.AspNetCore.Http;

namespace Yootek.App.ServiceHttpClient
{
    public interface IVietNamAdministrativeUnitService
    {
        Task<Province> GetProvinceByCode(string provinceCode);
        Task<District> GetDistrictByCode(string districtCode);
        Task<Ward> GetWardByCode(string wardCode);
        Task<List<Province>> GetProvinces();
        Task<List<District>> GetDistricts(string provinceCode);
        Task<List<Ward>> GetWards(string districtCode);
    }

    public class VietNamAdministrativeUnitService : IVietNamAdministrativeUnitService
    {
        private readonly HttpClient _client;

        public VietNamAdministrativeUnitService(HttpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<List<Province>> GetProvinces()
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/VietnameseAdministrativeUnit/Provinces");
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<List<Province>>();
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<District>> GetDistricts(string provinceCode)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/VietnameseAdministrativeUnit/Districts?provinceCode={provinceCode}");
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<List<District>>();
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<Ward>> GetWards(string districtCode)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/VietnameseAdministrativeUnit/Wards?districtCode={districtCode}");
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<List<Ward>>();
            }
            catch
            {
                 return null;
            }
        }

        public async Task<Province> GetProvinceByCode(string provinceCode)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/VietnameseAdministrativeUnit/GetProvinceByCode?provinceCode={provinceCode}");
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<Province>();
            }
            catch
            {
                return null;
            }

        }
        public async Task<District> GetDistrictByCode(string districtCode)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/VietnameseAdministrativeUnit/GetDistrictByCode?districtCode={districtCode}");
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<District>();
            }
            catch
            {
                return null;
            }

        }
        public async Task<Ward> GetWardByCode(string wardCode)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/VietnameseAdministrativeUnit/GetWardByCode?wardCode={wardCode}");
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<Ward>();
            }
            catch
            {
                return null;
            }

        }
    }
}