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
    public interface IAdminVoucherAppService : IApplicationService
    {
        Task<DataResult> GetAllVouchersByAdminAsync(AdminGetAllVouchersInputDto input);
        Task<DataResult> GetVoucherByIdAsync(long id);
        Task<DataResult> CreateVoucherAsync(CreateVoucherInputDto input);
        Task<DataResult> UpdateVoucherAsync(UpdateVoucherInputDto input);
        Task<DataResult> DeleteVoucherAsync(DeleteVoucherInputDto input);
    }
    public class AdminVoucherAppService : IMAXAppServiceBase, IAdminVoucherAppService
    {
        private readonly VoucherProtoGrpc.VoucherProtoGrpcClient _voucherProtoClient;
        private readonly ProviderProtoGrpc.ProviderProtoGrpcClient _providerProtoClient;
        public AdminVoucherAppService(
            VoucherProtoGrpc.VoucherProtoGrpcClient voucherProtoClient,
            ProviderProtoGrpc.ProviderProtoGrpcClient providerProtoClient
            )
        {
            _voucherProtoClient = voucherProtoClient;
            _providerProtoClient = providerProtoClient;
        }
        public async Task<DataResult> GetAllVouchersByAdminAsync(AdminGetAllVouchersInputDto input)
        {
            try
            {
                #region Validation
                ValidateFormIdAdmin((int)input.FormId);
                #endregion

                GetAllVouchersByAdminRequest request = new()
                {
                    TenantId = input.TenantId ?? 0,
                    ProviderId = input.ProviderId ?? 0,
                    Search = input.Search ?? "",
                    FormId = (int)input.FormId,
                    IsAdminCreate = true,
                    MaxResultCount = input.MaxResultCount,
                    SkipCount = input.SkipCount,
                };
                GetAllVouchersByAdminResponse result = await _voucherProtoClient.GetAllVouchersByAdminAsync(
                    request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Items, "Get all vouchers by admin success", result.TotalCount);
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
                throw new UserFriendlyException(ex.Message);
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
                    IsAdminCreate = true,
                    MaxDistributionBuyer = input.MaxDistributionBuyer,
                    ListItems = { input.ListItems },
                };
                await _voucherProtoClient.CreateVoucherAsync(request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess("Create voucher by admin success");
            }
            catch (RpcException ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

        // UpComming || OnGoing
        public async Task<DataResult> UpdateVoucherAsync(UpdateVoucherInputDto input)
        {
            try
            {
                UpdateVoucherRequest request = new()
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
                await _voucherProtoClient.UpdateVoucherAsync(request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess("Update voucher by admin success");
            }
            catch (RpcException ex)
            {
                throw new UserFriendlyException(ex.Message);
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
                await _voucherProtoClient.DeleteVoucherAsync(
                    new DeleteVoucherRequest() { Id = input.Id }, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess("Delete voucher by admin success");
            }
            catch (RpcException ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

        #region Validation 
        private static void ValidateFormIdAdmin(int formId)
        {
            if (!Enum.IsDefined(typeof(FORM_ADMIN_GET_VOUCHERS), formId))
            {
                throw new UserFriendlyException("FormId is invalid");
            }
        }
        #endregion
    }
}
