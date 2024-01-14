using System;
using System.Net.Http;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Runtime.Session;
using Abp.UI;
using Grpc.Core;
using Yootek.App.Grpc;
using Yootek.App.ServiceHttpClient.Dto.Business;
using Yootek.App.ServiceHttpClient.Business;
using Yootek.Application.Protos.Business.Providers;
using Yootek.Common.DataResult;
using Yootek.Services.SmartSocial.Ecofarm;
using Yootek.Services.SmartSocial.Providers.Dto;
using Microsoft.Extensions.Configuration;

namespace Yootek.Services.SmartSocial.Providers
{
    public interface IAdminProviderAppService : IApplicationService
    {
        /// 1. Admin :
        /// a. Lấy danh sách tất cả providers
        /// b. Update trạng thái của providers : duyệt or not
        Task<object> GetListAsync(GetListProvidersByAdminDto input);

        Task<DataResult> GetProviderByIdAsync(long providerId);
        Task<DataResult> UpdateStatusProviderByAdminAsync(UpdateStateProviderByAdminInputDto input);
        Task<DataResult> DeleteProviderAsync(long id);
        Task<DataResult> ApproveProviderAsync(ApproveProviderInputDto input);
        Task<DataResult> GetListReportAsync(GetAllReportsByAdminInputDto input);
        Task<DataResult> UpdateReportByAdminAsync(ApprovalReportByAdminInputDto input);
        Task<DataResult> DeleteReportAsync(DeleteReportInputDto input);
    }

    // [AbpAuthorize(PermissionNames.Social_Business_Providers)]
    public class AdminProviderAppService : YootekAppServiceBase, IAdminProviderAppService
    {
        private readonly BaseHttpClient _httpClient;
        private readonly IHttpReportService _httpReportService;
        private readonly ProviderProtoGrpc.ProviderProtoGrpcClient _providerProtoClient;

        public AdminProviderAppService(
            ProviderProtoGrpc.ProviderProtoGrpcClient providerProtoClient,
            IHttpReportService httpReportService,
            IAbpSession abpSession,
            IConfiguration configuration
        )
        {
            _providerProtoClient = providerProtoClient;
            _httpReportService = httpReportService;
            _httpClient = new BaseHttpClient(abpSession, configuration["ApiSettings:Business.Report"]);
        }

        // GET ALL
        public async Task<object> GetListAsync(GetListProvidersByAdminDto input)
        {
            try
            {
                var result = await _httpClient.SendSync<PagedResultDto<EcoFarmProviderGetListDto>>(
                    "/api/v1/AdminProviders/get-list", HttpMethod.Get, input);
                if (result.Success) return DataResult.ResultSuccess(result.Data.Items, "", result.Data.TotalCount);
                return result;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        #region GET DETAIL

        public async Task<DataResult> GetProviderByIdAsync(long id)
        {
            try
            {
                GetProviderByIdRequest request = new() { Id = id };
                var result =
                    await _providerProtoClient.GetProviderByIdAsync(request, MetadataGrpc.MetaDataHeader(AbpSession));

                return DataResult.ResultSuccess(result.Data, "Get provider by id success");
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

        #endregion

        // UPDATE STATUS (by admin)
        public async Task<DataResult> UpdateStatusProviderByAdminAsync(UpdateStateProviderByAdminInputDto input)
        {
            try
            {
                UpdateStateOfProviderByAdminRequest request = new()
                {
                    Id = input.Id,
                    FormId = input.FormId
                };
                await _providerProtoClient.UpdateStateOfProviderByAdminAsync(request,
                    MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess("Update state provider by admin success");
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

        // DELETE
        public async Task<DataResult> DeleteProviderAsync(long id)
        {
            try
            {
                await _providerProtoClient.DeleteProviderAsync(new DeleteProviderRequest { Id = id },
                    MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess("Delete provider success");
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

        // Approve provider 
        public async Task<DataResult> ApproveProviderAsync(ApproveProviderInputDto input)
        {
            try
            {
                UpdateStateOfProviderByAdminRequest request = new()
                {
                    Id = input.Id,
                    FormId = (int)FormIdUpdateState.ACTIVED
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
                throw;
            }
        }

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
                    Address = input.Address ?? ""
                };
                var result =
                    await _providerProtoClient.UpdateProviderAsync(request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess("Update provider success");
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

        #region DELETE (many)

        private async Task<DataResult> DeleteAllProvidersAsync(long userId)
        {
            try
            {
                await _providerProtoClient.DeleteAllProvidersAsync(new DeleteAllProvidersRequest { UserId = userId },
                    MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess("Delete all providers success");
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

        #endregion

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
                    MaxResultCount = input.MaxResultCount
                };

                var listResult = await _httpReportService.GetAllReportsByAdmin(request);
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
                await _httpReportService.UpdateStateReport(new UpdateStateReportDto
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
                await _httpReportService.DeleteReport(new DeleteReportDto
                {
                    Ids = input.Ids
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