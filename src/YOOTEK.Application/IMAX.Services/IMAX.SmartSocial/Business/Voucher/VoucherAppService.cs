using Abp.Application.Services;
using Abp.UI;
using Grpc.Core;
using IMAX.App.Grpc;
using IMAX.Application.Protos.Business.Providers;
using IMAX.Application.Protos.Business.Vouchers;
using IMAX.Common.DataResult;
using IMAX.Services.SmartSocial.Vouchers.Dto;
using System;
using System.Threading.Tasks;

namespace IMAX.Services.SmartSocial.Vouchers
{
    public interface IVoucherAppService : IApplicationService
    {
        Task<DataResult> GetAllVouchersByPartnerAsync(GetAllVouchersByPartnerInputDto input);
        Task<DataResult> GetAllVouchersByUserAsync(GetAllVouchersByUserInputDto input);
        Task<DataResult> GetVoucherByIdAsync(long id);
        Task<DataResult> CreateVoucherAsync(CreateVoucherInputDto input);
        Task<DataResult> CheckVoucherAvailableAsync(CheckVoucherAvailableInputDto input);
        Task<DataResult> UpdateVoucherAsync(UpdateVoucherInputDto input);
        Task<DataResult> EndedVoucherAsync(EndedVoucherInputDto input);
        Task<DataResult> StartEarlyVoucherAsync(StartEarlyVoucherInputDto input);
        Task<DataResult> DeleteVoucherAsync(DeleteVoucherInputDto input);
    }
    public class VoucherAppService : IMAXAppServiceBase, IVoucherAppService
    {
        private readonly VoucherProtoGrpc.VoucherProtoGrpcClient _voucherProtoClient;
        private readonly ProviderProtoGrpc.ProviderProtoGrpcClient _providerProtoClient;
        public VoucherAppService(
            VoucherProtoGrpc.VoucherProtoGrpcClient voucherProtoClient,
            ProviderProtoGrpc.ProviderProtoGrpcClient providerProtoClient
            )
        {
            _voucherProtoClient = voucherProtoClient;
            _providerProtoClient = providerProtoClient;
        }
        public async Task<DataResult> GetAllVouchersByPartnerAsync(GetAllVouchersByPartnerInputDto input)
        {
            try
            {
                GetAllVouchersByPartnerRequest request = new()
                {
                    TenantId = input.TenantId ?? 0,
                    ProviderId = input.ProviderId ?? 0,
                    Search = input.Search ?? "",
                    FormId = (int)input.FormId,
                    IsAdminCreate = input.IsAdminCreate ?? false,
                    MaxResultCount = input.MaxResultCount,
                    SkipCount = input.SkipCount,
                };
                GetAllVouchersByPartnerResponse result = await _voucherProtoClient.GetAllVouchersByPartnerAsync(
                    request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Items, "Get all vouchers success", result.TotalCount);
            }
            catch (RpcException ex)
            {
                throw new UserFriendlyException(ex.Status.Detail);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<DataResult> GetAllVouchersByUserAsync(GetAllVouchersByUserInputDto input)
        {
            try
            {
                GetAllVouchersByUserRequest request = new()
                {
                    TenantId = input.TenantId ?? 0,
                    ProviderId = input.ProviderId ?? 0,
                    Search = input.Search ?? "",
                    FormId = (int)input.FormId,
                    IsAdminCreate = input.IsAdminCreate ?? false,
                    MaxResultCount = input.MaxResultCount,
                    SkipCount = input.SkipCount,
                };
                GetAllVouchersByUserResponse result = await _voucherProtoClient.GetAllVouchersByUserAsync(
                    request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Items, "Get all vouchers success", result.TotalCount);
            }
            catch (RpcException ex)
            {
                throw new UserFriendlyException(ex.Status.Detail);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<DataResult> GetVoucherByIdAsync(long id)
        {
            try
            {
                GetVoucherDetailRequest request = new() { Id = id };
                GetVoucherDetailResponse result = await _voucherProtoClient.GetVoucherDetailAsync(
                    request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Data, "Get voucher detail success");
            }
            catch (RpcException ex)
            {
                throw new UserFriendlyException(ex.Status.Detail);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<DataResult> CreateVoucherAsync(CreateVoucherInputDto input)
        {
            try
            {
                CreateVoucherRequest request = new()
                {
                    TenantId = input.TenantId ?? 0,
                    ProviderId = input.ProviderId ?? 0,
                    Type = input.Type,
                    Scope = input.Scope,
                    DiscountType = input.DiscountType,
                    VoucherCode = input.VoucherCode,
                    Name = input.Name ?? "",
                    Quantity = input.Quantity,
                    MinBasketPrice = input.MinBasketPrice,
                    MaxPrice = input.MaxPrice,
                    Percentage = input.Percentage,
                    DiscountAmount = input.DiscountAmount,
                    DateStart = ConvertDatetimeToString(input.DateStart),
                    DateEnd = ConvertDatetimeToString(input.DateEnd),
                    Description = input.Description ?? "",
                    IsAdminCreate = input.IsAdminCreate ?? false,
                    MaxDistributionBuyer = input.MaxDistributionBuyer,
                    ListItems = { input.ListItems },
                };
                await _voucherProtoClient.CreateVoucherAsync(
                    request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess("Create voucher success");
            }
            catch (RpcException ex)
            {
                throw new UserFriendlyException(ex.Status.Detail);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

        // Check voucher available
        public async Task<DataResult> CheckVoucherAvailableAsync(CheckVoucherAvailableInputDto input)
        {
            CheckVoucherAvailableRequest request = new()
            {
                Items = { input.Items }
            };
            CheckVoucherAvailableResponse result = await _voucherProtoClient.CheckVoucherAvailableAsync(
                request, MetadataGrpc.MetaDataHeader(AbpSession));
            return DataResult.ResultSuccess(result.Data, "Check voucher success");
        }

        // UpComming || OnGoing
        public async Task<DataResult> UpdateVoucherAsync(UpdateVoucherInputDto input)
        {
            try
            {
                UpdateVoucherByPartnerRequest request = new()
                {
                    Id = input.Id,
                    Name = input.Name ?? "",
                    Description = input.Description ?? "",
                    Quantity = input.Quantity,
                    MinBasketPrice = input.MinBasketPrice,
                    MaxPrice = input.MaxPrice,
                    Percentage = input.Percentage,
                    DiscountAmount = input.DiscountAmount,
                    DateStart = ConvertDatetimeToString(input.DateStart),
                    DateEnd = ConvertDatetimeToString(input.DateEnd),
                    MaxDistributionBuyer = input.MaxDistributionBuyer,
                };
                await _voucherProtoClient.UpdateVoucherByPartnerAsync(request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess("Update voucher success");
            }
            catch (RpcException ex)
            {
                throw new UserFriendlyException(ex.Status.Detail);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

        // UpComming || OnGoing
        public async Task<DataResult> ReceiveVoucherAsync(ReceiveVoucherInputDto input)
        {
            try
            {
                UpdateListUserRequest request = new()
                {
                    Id = input.Id,
                    Item = new PBuyer
                    {
                        UserId = (long)AbpSession.UserId,
                    }
                };
                await _voucherProtoClient.UpdateListUserAsync(request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess("Receive voucher success");
            }
            catch (RpcException ex)
            {
                throw new UserFriendlyException(ex.Status.Detail);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

        // UpComming mới xóa đc 
        public async Task<DataResult> DeleteVoucherAsync(DeleteVoucherInputDto input)
        {
            try
            {
                await _voucherProtoClient.DeleteVoucherByPartnerAsync(
                    new DeleteVoucherByPartnerRequest() { Id = input.Id }, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess("Delete voucher success");
            }
            catch (RpcException ex)
            {
                throw new UserFriendlyException(ex.Status.Detail);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

        // UpComming
        public async Task<DataResult> StartEarlyVoucherAsync(StartEarlyVoucherInputDto input)
        {
            try
            {
                StartEarlyVoucherRequest request = new()
                {
                    Id = input.Id,
                };
                await _voucherProtoClient.StartEarlyVoucherAsync(request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess("Start early voucher success");
            }
            catch (RpcException ex)
            {
                throw new UserFriendlyException(ex.Status.Detail);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

        // OnGoing
        public async Task<DataResult> EndedVoucherAsync(EndedVoucherInputDto input)
        {
            try
            {
                EndedVoucherRequest request = new()
                {
                    Id = input.Id,
                };
                await _voucherProtoClient.EndedVoucherAsync(request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess("End voucher success");
            }
            catch (RpcException ex)
            {
                throw new UserFriendlyException(ex.Status.Detail);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }
    }
}
