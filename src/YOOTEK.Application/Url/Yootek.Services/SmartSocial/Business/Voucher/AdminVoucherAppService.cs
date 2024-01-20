using Abp.Application.Services;
using Abp.UI;
using Yootek.Application.Protos.Business.Providers;
using Yootek.Application.Protos.Business.Vouchers;
using Yootek.Services.SmartSocial.Vouchers.Dto;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Runtime.Session;
using Yootek.Services.SmartSocial.Ecofarm;
using Microsoft.Extensions.Configuration;

namespace Yootek.Services.SmartSocial.Vouchers
{
    public interface IAdminVoucherAppService : IApplicationService
    {
        Task<object> GetAllVouchersByAdminAsync(GetAllVouchersByAdminInputDto input);
        Task<object> GetVoucherByIdAsync(GetVoucherByIdInputDto id);
        Task<object> CreateVoucherAsync(CreateVoucherInputDto input);
        Task<object> UpdateVoucherAsync(UpdateVoucherInputDto input);
        Task<object> DeleteVoucherAsync(DeleteVoucherInputDto input);
    }

    public class AdminVoucherAppService : YootekAppServiceBase, IAdminVoucherAppService
    {
        private readonly VoucherProtoGrpc.VoucherProtoGrpcClient _voucherProtoClient;
        private readonly ProviderProtoGrpc.ProviderProtoGrpcClient _providerProtoClient;
        private readonly BaseHttpClient _httpClient;
        private readonly IAbpSession _abpSession;

        public AdminVoucherAppService(
            VoucherProtoGrpc.VoucherProtoGrpcClient voucherProtoClient,
            ProviderProtoGrpc.ProviderProtoGrpcClient providerProtoClient, IAbpSession abpSession,
            IConfiguration configuration)
        {
            _voucherProtoClient = voucherProtoClient;
            _providerProtoClient = providerProtoClient;
            _abpSession = abpSession;
            _httpClient = new BaseHttpClient(abpSession, configuration["ApiSettings:Voucher"]);
        }

        public async Task<object> GetAllVouchersByAdminAsync(GetAllVouchersByAdminInputDto input)
        {
            try
            {
                return await _httpClient.SendSync<PagedResultDto<VoucherDto>>("/Voucher/GetAllByAdmin",
                    HttpMethod.Get, input);
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
                return await _httpClient.SendSync<VoucherDto>("/Voucher/GetDetail", HttpMethod.Get, input);
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
                return await _httpClient.SendSync<bool>("Voucher/Create", HttpMethod.Post, input);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        // UpComming || OnGoing
        public async Task<object> UpdateVoucherAsync(UpdateVoucherInputDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("Voucher/Update", HttpMethod.Put, input);
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
                return await _httpClient.SendSync<bool>("Voucher/Delete", HttpMethod.Delete, input);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
    }
}