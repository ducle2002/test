using Abp.Application.Services;
using Yootek.App.ServiceHttpClient.Dto.Business;
using Yootek.App.ServiceHttpClient.Business;
using Yootek.Common.DataResult;
using System;
using System.Threading.Tasks;
using Abp;
using Microsoft.AspNetCore.Mvc;

namespace Yootek.Services.SmartSocial.Advertisements
{
    public interface IAdvertisementAppService : IApplicationService
    {
        Task<object> GetListByPartnerAsync([FromQuery] GetAllAdvertisementsDto input);
        Task<object> GetListByUserAsync([FromQuery] GetAllAdvertisementsDto input);
        Task<object> GetByIdAsync([FromQuery] GetAdvertisementByIdDto input);
        Task<object> CreateAsync(CreateAdvertisementDto input);
        Task<object> UpdateAsync(UpdateAdvertisementDto input);
        Task<object> UpdateStatusAsync([FromBody] UpdateStatusAdvertisementDto input);
        Task<object> DeleteAsync([FromQuery] DeleteAdvertisementDto input);
    }
    public class AdvertisementAppService : YootekAppServiceBase, IAdvertisementAppService
    {
        private readonly IHttpAdvertisementService _httpAdvertisementService;
        public AdvertisementAppService(IHttpAdvertisementService httpAdvertisementService)
        {
            _httpAdvertisementService = httpAdvertisementService;
        }
        public async Task<object> GetListByAdminAsync([FromQuery] GetAllAdvertisementsDto input)
        {
            try
            {
                var result = await _httpAdvertisementService.GetListByAdminAsync(input);
                if (result.Success) return DataResult.ResultSuccess(result.Data.Items, "", result.Data.TotalCount);
                return result;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        
        public async Task<object> GetListByPartnerAsync([FromQuery] GetAllAdvertisementsDto input)
        {
            try
            {
                var result = await _httpAdvertisementService.GetListByPartnerAsync(input);
                if (result.Success) return DataResult.ResultSuccess(result.Data.Items, "", result.Data.TotalCount);
                return result;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        public async Task<object> GetListByUserAsync([FromQuery] GetAllAdvertisementsDto input)
        {
            try
            {
                var result = await _httpAdvertisementService.GetListByUserAsync(input);
                if (result.Success) return DataResult.ResultSuccess(result.Data.Items, "", result.Data.TotalCount);
                return result;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        public async Task<object> GetByIdAsync([FromQuery] GetAdvertisementByIdDto input)
        {
            try
            {
                var result = await _httpAdvertisementService.GetByIdAsync(input);
                return result;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        public async Task<object> CreateAsync([FromBody] CreateAdvertisementDto input)
        {
            try
            {
                return await _httpAdvertisementService.CreateAsync(input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        
        public async Task<object> UpdateAsync([FromBody] UpdateAdvertisementDto input)
        {
            try
            {
                return await _httpAdvertisementService.UpdateAsync(input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        public async Task<object> UpdateStatusAsync([FromBody] UpdateStatusAdvertisementDto input)
        {
            try
            {
                return await _httpAdvertisementService.UpdateStatusAsync(input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        public async Task<object> DeleteAsync([FromQuery] DeleteAdvertisementDto input)
        {
            try
            {
                return await _httpAdvertisementService.DeleteAsync(input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
    }
}
