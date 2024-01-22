using Abp.Application.Services;
using Abp.UI;
using Yootek.App.Grpc;
using Yootek.Application.Protos.Business.Providers;
using Yootek.Common.DataResult;
using Yootek.Services.SmartSocial.Providers.Dto;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Yootek.Yootek.Services.Yootek.SmartCommunity.TenantBusinessServiceNEW
{
    public interface ITenantAdminBusinessNewAppService : IApplicationService
    {
        Task<object> GetAllProvidersByAdminAsync(GetListProvidersByAdminDto input);
        Task<object> GetProviderByIdAsync(GetProviderByIdInputDto input);
        Task<object> CreateProviderAsync(CreateProviderByAdminInputDto input);
        Task<object> CreateListProvidersAsync(CreateListProvidersByAdminInputDto input);
        Task<object> UpdateProviderAsync(UpdateProviderByAdminInputDto input);
        Task<object> DeleteProviderAsync(long providerId);
    }

    public class TenantAdminBusinessNewAppService : YootekAppServiceBase, ITenantAdminBusinessNewAppService
    {
        private readonly ProviderProtoGrpc.ProviderProtoGrpcClient _providerProtoClient;
        public TenantAdminBusinessNewAppService(
            ProviderProtoGrpc.ProviderProtoGrpcClient providerProtoClient
            )
        {
            _providerProtoClient = providerProtoClient;
        }

        public async Task<object> GetAllProvidersByAdminAsync(GetListProvidersByAdminDto input)
        {
            try
            {
                GetAllProvidersByAdminRequest request = new()
                {
                    TenantId = AbpSession.TenantId ?? 0,
                    PartnerId = 0,
                    FormId = 0,
                    Keyword = input.Keyword ?? "",
                    DateFrom = ConvertDatetimeToTimestamp(input.DateFrom ?? new DateTime(1970, 1, 1)),
                    DateTo = ConvertDatetimeToTimestamp(input.DateTo ?? DateTime.Now),
                    SkipCount = input.SkipCount,
                    MaxResultCount = input.MaxResultCount,
                };
                GetAllProvidersByAdminResponse result = await _providerProtoClient.GetAllProvidersByAdminAsync(
                    request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Items, "Get providers by admin success", result.TotalCount);
            }
            catch (UserFriendlyException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
        public async Task<object> GetProviderByIdAsync(GetProviderByIdInputDto input)
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
            catch (UserFriendlyException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        // CREATE
        public async Task<object> CreateProviderAsync(CreateProviderByAdminInputDto input)
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
                    SocialTenantId = 0,
                    Type = input.Type ?? 0,
                    GroupType = input.GroupType ?? 0,
                    Latitude = input.Latitude ?? 0,
                    Longitude = input.Longitude ?? 0,
                    PropertyHistories = "",
                    Properties = input.Properties ?? "",
                    State = input.State ?? 0,
                    StateProperties = "",
                    IsAdminCreate = true,
                    DistrictId = input.DistrictId ?? "",
                    ProvinceId = input.ProvinceId ?? "",
                    WardId = input.WardId ?? "",
                    Address = input.Address ?? "",
                    WorkTime = input.WorkTime ?? "",
                    OwnerId = AbpSession.UserId ?? 0,
                };
                CreateProviderResponse result = await _providerProtoClient.CreateProviderAsync(request, MetadataGrpc.MetaDataHeader(AbpSession));

                return DataResult.ResultSuccess("Create provider success");
            }
            catch (UserFriendlyException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
        public async Task<object> CreateListProvidersAsync(CreateListProvidersByAdminInputDto input)
        {
            try
            {
                // 25 field
                CreateListProvidersRequest request = new()
                {
                    Items =
                    { input.Items.Select(i => new CreateProviderRequest
                        {
                            Name = i.Name ?? "",
                            Email = i.Email ?? "",
                            Contact = i.Contact ?? "",
                            Description = i.Description ?? "",
                            PhoneNumber = i.PhoneNumber ?? "",
                            ImageUrls = { i.ImageUrls },
                            OwnerInfo = i.OwnerInfo ?? "",
                            BusinessInfo = i.BusinessInfo ?? "",
                            TenantId = i.TenantId ?? 0,
                            SocialTenantId = 0,
                            Type = i.Type ?? 0,
                            GroupType = i.GroupType ?? 0,
                            Latitude = i.Latitude ?? 0,
                            Longitude = i.Longitude ?? 0,
                            PropertyHistories = "",
                            Properties = i.Properties ?? "",
                            State = i.State ?? 0,
                            StateProperties = "",
                            IsAdminCreate = true,
                            DistrictId = i.DistrictId ?? "",
                            ProvinceId = i.ProvinceId ?? "",
                            WardId = i.WardId ?? "",
                            Address = i.Address ?? "",
                            WorkTime = i.WorkTime ?? "",
                            OwnerId = AbpSession.UserId ?? 0,
                        })
                    }
                };
                CreateListProvidersResponse result = await _providerProtoClient.CreateListProvidersAsync(request, MetadataGrpc.MetaDataHeader(AbpSession));

                return DataResult.ResultSuccess("Create list providers success");
            }
            catch (UserFriendlyException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        // UPDATE
        public async Task<object> UpdateProviderAsync(UpdateProviderByAdminInputDto input)
        {
            try
            {
                // 14 field
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
                // check provider of partner
                GetProviderByIdResponse response = await _providerProtoClient.GetProviderByIdAsync(
                    new GetProviderByIdRequest() { Id = input.Id, IsDataStatic = false });
                if (response.Data != null)
                {
                    UpdateProviderResponse result = await _providerProtoClient.UpdateProviderAsync(request, MetadataGrpc.MetaDataHeader(AbpSession));
                    return DataResult.ResultSuccess("Update provider success");
                }
                throw new UserFriendlyException("Cannot update provider");
            }
            catch (UserFriendlyException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        // DELETE
        public async Task<object> DeleteProviderAsync(long id)
        {
            try
            {
                // check provider exist
                GetProviderByIdResponse response = await _providerProtoClient.GetProviderByIdAsync(
                    new GetProviderByIdRequest() { Id = id, IsDataStatic = false }, MetadataGrpc.MetaDataHeader(AbpSession));
                if (response.Data != null)
                {
                    DeleteProviderResponse result = await _providerProtoClient.DeleteProviderAsync(
                        new DeleteProviderRequest() { Id = id }, MetadataGrpc.MetaDataHeader(AbpSession));
                    return DataResult.ResultSuccess("Delete provider success");
                }
                throw new UserFriendlyException("Provider not found");
            }
            catch (UserFriendlyException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
    }
}
