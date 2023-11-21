using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.UI;
using Grpc.Core;
using IMAX.App.Grpc;
using IMAX.App.ServiceHttpClient.Dto;
using IMAX.App.ServiceHttpClient.Dto.Imax.Business;
using IMAX.App.ServiceHttpClient.Imax.Business;
using IMAX.Application.Protos.Business.Providers;
using IMAX.Common.DataResult;
using IMAX.Services.SmartSocial.Providers.Dto;
using System;
using System.Threading.Tasks;

namespace IMAX.Services.SmartSocial.Providers
{
    public interface IAdminProviderAppService : IApplicationService
    {
        /// <summary>
        /// 1. Admin :
        /// a. Lấy danh sách tất cả providers
        /// b. Update trạng thái của providers : duyệt or not
        /// 
        /// 2. Đối tác :
        /// a. Lấy danh sách  cửa hàng mình tạo
        /// b. Thêm sửa xóa cửa hàng
        /// 
        /// 3. Người dùng:
        /// a. Lấy danh sách cửa hàng được duyệt
        /// b. API lấy danh sách cửa hàng theo API search chung(có thể elasticsearch)
        /// 
        /// </summary>
        /// 
        /// <remarks>
        /// 
        /// </remarks>

        Task<DataResult> GetAllProvidersByAdminAsync(GetProvidersByAdminInputDto input);
        Task<DataResult> GetProviderByIdAsync(long providerId);
        Task<DataResult> UpdateProviderAsync(UpdateProviderByAdminInputDto input);
        Task<DataResult> UpdateStatusProviderByAdminAsync(UpdateStateProviderByAdminInputDto input);
        Task<DataResult> DeleteProviderAsync(long providerId);
        // Task<DataResult> DeleteAllProvidersAsync(long userId);
        Task<DataResult> ApproveProviderAsync(ApproveProviderInputDto input);
        Task<DataResult> GetListReportAsync(GetAllReportsByAdminInputDto input);
        Task<DataResult> UpdateReportByAdminAsync(ApprovalReportByAdminInputDto input);
        Task<DataResult> DeleteReportAsync(DeleteReportInputDto input);

    }
    // [AbpAuthorize(PermissionNames.Social_Business_Providers)]
    public class AdminProviderAppService : IMAXAppServiceBase, IAdminProviderAppService
    {
        private readonly ProviderProtoGrpc.ProviderProtoGrpcClient _providerProtoClient;
        private readonly IHttpReportService _httpReportService;
        public AdminProviderAppService(
            ProviderProtoGrpc.ProviderProtoGrpcClient providerProtoClient,
            IHttpReportService httpReportService
            )
        {
            _providerProtoClient = providerProtoClient;
            _httpReportService = httpReportService;
        }

        // GET ALL
        public async Task<DataResult> GetAllProvidersByAdminAsync(GetProvidersByAdminInputDto input)
        {
            try
            {
                GetAllProvidersByAdminRequest request = new()
                {
                    TenantId = AbpSession.TenantId ?? 0,
                    FormId = (int)input.FormId,
                    PartnerId = input.PartnerId ?? 0,
                    Keyword = input.Keyword ?? "",
                    DateFrom = ConvertDatetimeToTimestamp(input.DateFrom ?? new DateTime(1970, 1, 1)),
                    DateTo = ConvertDatetimeToTimestamp(input.DateTo ?? DateTime.Now),
                    SkipCount = input.SkipCount,
                    MaxResultCount = input.MaxResultCount,
                };
                GetAllProvidersByAdminResponse result = await _providerProtoClient.GetAllProvidersByAdminAsync(request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Items, "Get providers by admin success", result.TotalCount);
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

        #region GET DETAIL
        public async Task<DataResult> GetProviderByIdAsync(long id)
        {
            try
            {
                GetProviderByIdRequest request = new() { Id = id };
                GetProviderByIdResponse result = await _providerProtoClient.GetProviderByIdAsync(request, MetadataGrpc.MetaDataHeader(AbpSession));

                return DataResult.ResultSuccess(result.Data, "Get provider by id success");
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
        #endregion

        // UPDATE
        public async Task<DataResult> UpdateProviderAsync(UpdateProviderByAdminInputDto input)
        {
            try
            {
                UpdateProviderRequest request = new()
                {
                    Id = input.Id,
                    Name = input.Name ?? "",
                    Email = input.Email ?? "",
                    Contact = input.Contact ?? "",
                    Description = input.Description ?? "",
                    PhoneNumber = input.PhoneNumber ?? "",
                    ImageUrls = { input.ImageUrls },
                    BusinessInfo = input.BusinessInfo ?? "",
                    Latitude = input.Latitude ?? 0,
                    Longitude = input.Longitude ?? 0,
                    DistrictId = input.DistrictId ?? "",
                    ProvinceId = input.ProvinceId ?? "",
                    WardId = input.WardId ?? "",
                    Address = input.Address ?? "",
                };
                UpdateProviderResponse result = await _providerProtoClient.UpdateProviderAsync(request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess("Update provider success");
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
        // UPDATE STATUS (by admin)
        public async Task<DataResult> UpdateStatusProviderByAdminAsync(UpdateStateProviderByAdminInputDto input)
        {
            try
            {
                UpdateStateOfProviderByAdminRequest request = new()
                {
                    Id = input.Id,
                    FormId = input.FormId,
                };
                await _providerProtoClient.UpdateStateOfProviderByAdminAsync(request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess("Update state provider by admin success");
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
        // DELETE
        public async Task<DataResult> DeleteProviderAsync(long id)
        {
            try
            {
                await _providerProtoClient.DeleteProviderAsync(new DeleteProviderRequest() { Id = id }, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess("Delete provider success");
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

        #region DELETE (many)
        private async Task<DataResult> DeleteAllProvidersAsync(long userId)
        {
            try
            {
                await _providerProtoClient.DeleteAllProvidersAsync(new DeleteAllProvidersRequest() { UserId = userId }, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess("Delete all providers success");
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
        #endregion

        // Approve provider 
        public async Task<DataResult> ApproveProviderAsync(ApproveProviderInputDto input)
        {
            try
            {
                UpdateStateOfProviderByAdminRequest request = new()
                {
                    Id = input.Id,
                    FormId = (int)FormIdUpdateState.ACTIVED,
                };
                await _providerProtoClient.UpdateStateOfProviderByAdminAsync(
                    request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess("Approve provider success");
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

        #region report 
        public async Task<DataResult> GetListReportAsync(GetAllReportsByAdminInputDto input)
        {
            try
            {
                GetAllReportsByAdminDto request = new()
                {
                    FormId = (int)input.FormId,
                    IsStatic = input.IsStatic,
                    ProviderId = input.ProviderId,
                    TypeReport = (int?)input.TypeReport,
                    SkipCount = input.SkipCount,
                    MaxResultCount = input.MaxResultCount,
                };

                MicroserviceResultDto<PagedResultDto<JoinReportProvider>> listResult = await _httpReportService.GetAllReportsByAdmin(request);
                return DataResult.ResultSuccess(listResult.Result, listResult.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> UpdateReportByAdminAsync(ApprovalReportByAdminInputDto input)
        {
            try
            {
                await _httpReportService.UpdateStateReport(new UpdateStateReportDto()
                {
                    Id = input.Id,
                    CurrentState = (int)ReportState.PENDING,
                    UpdateState = (int)ReportState.APPROVED
                });
                return DataResult.ResultSuccess("success");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        public async Task<DataResult> DeleteReportAsync(DeleteReportInputDto input)
        {
            try
            {
                await _httpReportService.DeleteReport(new DeleteReportDto()
                {
                    Ids = input.Ids,
                });
                return DataResult.ResultSuccess("success");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        #endregion
    }
}
