using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Runtime.Session;
using Abp.UI;
using Grpc.Core;
using Yootek.Common.DataResult;
using Yootek.Services.SmartSocial.Ecofarm;
using Yootek.Services.SmartSocial.Vouchers.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Yootek.Services.SmartSocial.Vouchers
{
    public interface IVoucherAppService : IApplicationService
    {
        Task<object> GetAllVouchersByPartnerAsync(GetAllVouchersByPartnerInputDto input);
        Task<object> GetAllVouchersByUserAsync(GetAllVouchersByUserInputDto input);
        Task<object> GetVoucherByIdAsync(GetVoucherByIdInputDto id);
        Task<object> CreateVoucherAsync(CreateVoucherInputDto input);
        Task<object> CheckVoucherAvailableAsync(CheckVoucherAvailableInputDto input);
        Task<object> UpdateVoucherAsync(UpdateVoucherInputDto input);
        Task<object> EndedVoucherAsync(EndedVoucherInputDto input);
        Task<object> StartEarlyVoucherAsync(StartEarlyVoucherInputDto input);
        Task<object> DeleteVoucherAsync(DeleteVoucherInputDto input);
        Task<object> ReceiveVoucherAsync([FromBody] ReceiveVoucherInputDto input);
    }

    public class VoucherAppService : YootekAppServiceBase, IVoucherAppService
    {
        private readonly BaseHttpClient _httpClient;

        public VoucherAppService(
            IConfiguration configuration, IAbpSession abpSession
        )
        {
            _httpClient = new BaseHttpClient(abpSession, configuration["ApiSettings:Business.Voucher"]);
        }

        public async Task<object> GetAllVouchersByPartnerAsync(GetAllVouchersByPartnerInputDto input)
        {
            try
            {
                var result = await _httpClient.SendSync<PagedResultDto<VoucherDto>>("/api/v1/Vouchers/partner/get-list",
                    HttpMethod.Get,
                    input);
                if (result.Success) return DataResult.ResultSuccess(result.Data.Items, "", result.Data.TotalCount);
                return result;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> GetAllVouchersByUserAsync(GetAllVouchersByUserInputDto input)
        {
            try
            {
                var result =  await _httpClient.SendSync<PagedResultDto<VoucherDto>>("/api/v1/Vouchers/user/get-list",
                    HttpMethod.Get,
                    input);
                if (result.Success) return DataResult.ResultSuccess(result.Data.Items, "", result.Data.TotalCount);
                return result;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> GetVoucherByIdAsync(GetVoucherByIdInputDto input)
        {
            try
            {
                return await _httpClient.SendSync<VoucherDto>("/api/v1/Vouchers/get-detail", HttpMethod.Get, input);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> CreateVoucherAsync(CreateVoucherInputDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/Vouchers/create", HttpMethod.Post, input);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        // Check voucher available
        public async Task<object> CheckVoucherAvailableAsync(CheckVoucherAvailableInputDto input)
        {
            return await _httpClient.SendSync<List<bool>>("/api/v1/Vouchers/check-available", HttpMethod.Post, input);
        }

        // UpComming || OnGoing
        public async Task<object> UpdateVoucherAsync(UpdateVoucherInputDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/Vouchers/partner/update", HttpMethod.Put, input);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        // UpComming mới xóa đc 
        public async Task<object> DeleteVoucherAsync(DeleteVoucherInputDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/Vouchers/delete", HttpMethod.Delete, input);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        // UpComming
        public async Task<object> StartEarlyVoucherAsync(StartEarlyVoucherInputDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/Vouchers/start-early", HttpMethod.Post, input);
            }
            catch (RpcException ex)
            {
                throw new UserFriendlyException(ex.Status.Detail);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        // OnGoing
        public async Task<object> EndedVoucherAsync(EndedVoucherInputDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/Vouchers/end-early", HttpMethod.Post, input);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        // UpComming || OnGoing
        public async Task<object> ReceiveVoucherAsync([FromBody] ReceiveVoucherInputDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/Vouchers/receive-voucher", HttpMethod.Post, input);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
    }
}