using Abp.Application.Services;
using Abp.UI;
using Grpc.Core;
using Yootek.App.Grpc;
using Yootek.App.ServiceHttpClient.Dto.Business;
using Yootek.App.ServiceHttpClient.Business;
using Yootek.Application.Protos.Business.Providers;
using Yootek.Common.DataResult;
using Yootek.Services.SmartSocial.Providers.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Yootek.Services.SmartSocial.Providers
{
    public interface IProviderAppService : IApplicationService
    {
        /// <summary>
        /// 1. Đối tác :
        /// a. Lấy danh sách  cửa hàng mình tạo
        /// b. Thêm sửa xóa cửa hàng
        /// 
        /// 2. Người dùng:
        /// a. Lấy danh sách cửa hàng được duyệt
        /// b. API lấy danh sách cửa hàng theo API search chung(có thể elasticsearch)
        /// 

        Task<DataResult> GetAllProvidersByPartnerAsync(GetProvidersByPartnerInputDto input);
        Task<DataResult> GetAllProvidersByUserAsync(GetProvidersByUserInputDto input);
        Task<DataResult> GetAllProvidersRandomAsync(GetProvidersRandomInputDto input);
        Task<DataResult> GetProviderByIdAsync(GetProviderByIdInputDto input);
        Task<PProvider?> GetProviderById(GetProviderByIdInputDto input);
        Task<DataResult> CreateProviderAsync(CreateProviderByPartnerInputDto input);
        Task<DataResult> UpdateProviderAsync(UpdateProviderByPartnerInputDto input);
        Task<DataResult> DeleteProviderAsync(long providerId);
    }
    public class ProviderAppService : YootekAppServiceBase, IProviderAppService
    {
        private readonly ProviderProtoGrpc.ProviderProtoGrpcClient _providerProtoClient;
        private readonly IHttpReportService _httpReportService;
        public ProviderAppService(
            ProviderProtoGrpc.ProviderProtoGrpcClient providerProtoClient,
            IHttpReportService httpReportService
            )
        {
            _providerProtoClient = providerProtoClient;
            _httpReportService = httpReportService;
        }

        // GET
        public async Task<DataResult> GetAllProvidersByPartnerAsync(GetProvidersByPartnerInputDto input)
        {
            try
            {
                GetAllProvidersByPartnerRequest request = new()
                {
                    TenantId = AbpSession.TenantId ?? 0,
                    UserId = AbpSession.UserId ?? 0,
                    Type = input.Type ?? 0,
                    FormId = (int)input.FormId,
                    GroupType = input.GroupType ?? 0,
                    Keyword = input.Keyword ?? "",
                    OrderBy = (int)(input.OrderBy ?? 0),
                    SkipCount = input.SkipCount,
                    MaxResultCount = input.MaxResultCount,
                };
                GetAllProvidersByPartnerResponse result = await _providerProtoClient.GetAllProvidersByPartnerAsync(
                    request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Items, "Get providers by partner success", result.TotalCount);
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
        public async Task<DataResult> GetAllProvidersByUserAsync(GetProvidersByUserInputDto input)
        {
            try
            {
                GetAllProvidersByUserRequest request = new()
                {
                    TenantId = AbpSession.TenantId ?? 0,
                    UserId = 0,
                    Type = input.Type ?? 0,
                    FormId = (int)FORM_ID_USER_GET_PROVIDER.FORM_USER_GET_PROVIDER_GETALL,
                    GroupType = input.GroupType ?? 0,
                    Keyword = input.Keyword ?? "",
                    IsDataStatic = false,
                    OrderBy = { input.OrderBy ?? new List<int>() },
                    DateFrom = ConvertDatetimeToTimestamp(input.DateFrom ?? new DateTime(1970, 1, 1)),
                    DateTo = ConvertDatetimeToTimestamp(input.DateTo ?? DateTime.Now),
                    Latitude = input.Latitude ?? 0,
                    Longitude = input.Longitude ?? 0,
                    MinRatePoint = input.MinRatePoint ?? 0,
                    ListServiceType = { input.ListServiceType },
                    SkipCount = input.SkipCount,
                    MaxResultCount = input.MaxResultCount,
                };
                GetAllProvidersByUserResponse result = await _providerProtoClient.GetAllProvidersByUserAsync(
                    request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Items, "Get providers by user success", result.TotalCount);
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
        public async Task<DataResult> GetAllProvidersRandomAsync(GetProvidersRandomInputDto input)
        {
            try
            {
                GetAllProvidersRandomRequest request = new()
                {
                    Latitude = input.Latitude ?? 0,
                    Longitude = input.Longitude ?? 0,
                    SkipCount = input.SkipCount,
                    MaxResultCount = input.MaxResultCount,
                };
                GetAllProvidersRandomResponse result = await _providerProtoClient.GetAllProvidersRandomAsync(
                    request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Items, "Get providers random success", result.TotalCount);
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
        public async Task<DataResult> GetProviderByIdAsync(GetProviderByIdInputDto input)
        {
            try
            {
                GetProviderByIdRequest request = new()
                {
                    Id = input.Id,
                    IsDataStatic = input.IsDataStatic ?? false,
                };
                GetProviderByIdResponse result = await _providerProtoClient.GetProviderByIdAsync(request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Data, "Get provider success");
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
        [RemoteService(false)]
        public async Task<PProvider?> GetProviderById(GetProviderByIdInputDto input)
        {
            try
            {
                GetProviderByIdRequest request = new()
                {
                    Id = input.Id,
                    IsDataStatic = input.IsDataStatic ?? false,
                };
                GetProviderByIdResponse result = await _providerProtoClient.GetProviderByIdAsync(request, MetadataGrpc.MetaDataHeader(AbpSession));
                return result?.Data;
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
        // CREATE
        public async Task<DataResult> CreateProviderAsync(CreateProviderByPartnerInputDto input)
        {
            try
            {
                // 25 field
                CreateProviderRequest request = new()
                {
                    Name = input.Name ?? "",
                    Email = input.Email ?? "",
                    Contact = input.Contact ?? "",
                    Description = input.Description ?? "",
                    PhoneNumber = input.PhoneNumber ?? "",
                    ImageUrls = { input.ImageUrls },
                    OwnerInfo = input.OwnerInfo ?? "",
                    BusinessInfo = input.BusinessInfo ?? "",
                    TenantId = input.TenantId ?? 0,
                    Type = input.Type ?? 0,
                    GroupType = input.GroupType ?? 0,
                    Latitude = input.Latitude ?? 0,
                    Longitude = input.Longitude ?? 0,
                    PropertyHistories = "",
                    Properties = input.Properties ?? "",
                    State = (int)PROVIDER_STATE.PENDING,
                    StateProperties = "",
                    IsAdminCreate = false,
                    DistrictId = input.DistrictId ?? "",
                    ProvinceId = input.ProvinceId ?? "",
                    WardId = input.WardId ?? "",
                    Address = input.Address ?? "",
                    WorkTime = input.WorkTime ?? "",
                    OwnerId = AbpSession.UserId ?? 0,
                };
                if (true)
                {
                    request.SocialTenantId = AbpSession.TenantId ?? 0; // if IsTenantPartner == true
                }
                await _providerProtoClient.CreateProviderAsync(request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess("Create provider success");
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
        public async Task<DataResult> UpdateProviderAsync(UpdateProviderByPartnerInputDto input)
        {
            try
            {
                // 14 field
                UpdateProviderByPartnerRequest request = new()
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
                await _providerProtoClient.UpdateProviderByPartnerAsync(request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess("Update provider by partner success");
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

        // UPDATE STATE
        public async Task<DataResult> UpdateStateOfProviderAsync(UpdateStateOfProviderByPartnerInputDto input)
        {
            try
            {
                UpdateStateOfProviderRequest request = new()
                {
                    Id = input.Id,
                    FormId = input.FormId,
                };
                await _providerProtoClient.UpdateStateOfProviderAsync(request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess("Update state provider success");
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
                await _providerProtoClient.DeleteProviderByPartnerAsync(new DeleteProviderByPartnerRequest() { Id = id },
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

        #region report 
        public async Task<DataResult> CreateReportAsync(CreateReportInputDto input)
        {
            try
            {
                CreateReportDto request = new()
                {
                    ImageUrls = input.ImageUrls,
                    IsStatic = input.IsStatic,
                    ProviderId = input.ProviderId,
                    TypeReport = (int)input.TypeReport,
                    ReportMessage = input.ReportMessage,
                };
                await _httpReportService.CreateReport(request);
                return DataResult.ResultSuccess("Success");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        #endregion
    }
}
