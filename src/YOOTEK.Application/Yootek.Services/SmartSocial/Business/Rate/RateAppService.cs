using Abp.Application.Services;
using Abp.UI;
using Grpc.Core;
using Yootek.App.Grpc;
using Yootek.Application.Protos.Business.Rates;
using Yootek.Common.DataResult;
using Yootek.Services.SmartSocial.Rates.Dto;
using System;
using System.Threading.Tasks;

namespace Yootek.Services.SmartSocial.Rates
{
    public interface IUserRateAppService : IApplicationService
    {
        Task<DataResult> GetAllRatesAsync(GetAllRateInputDto input);
        Task<DataResult> GetCountRateAsync(GetAllRateInputDto input);
        Task<DataResult> GetRateByIdAsync(long id);
        Task<DataResult> CreateRateAsync(CreateRateInputDto input);
        Task<DataResult> UpdateRateAsync(UpdateRateByUserInputDto input);
        Task<DataResult> DeleteRateAsync(DeleteRateInputDto input);
    }
    public class RateAppService : YootekAppServiceBase, IUserRateAppService
    {
        private readonly RateProtoGrpc.RateProtoGrpcClient _rateProtoClient;
        public RateAppService(
            RateProtoGrpc.RateProtoGrpcClient rateProtoClient
            )
        {
            _rateProtoClient = rateProtoClient;
        }
        public async Task<DataResult> GetAllRatesAsync(GetAllRateInputDto input)
        {
            try
            {
                GetRatesResponse result = await _rateProtoClient.GetAllRatesAsync(new GetRatesRequest()
                {
                    TenantId = input.TenantId ?? 0,
                    ProviderId = input.ProviderId ?? 0,
                    ItemId = input.ItemId ?? 0,
                    UserId = input.UserId ?? 0,
                    IsComment = input.IsComment ?? false,
                    IsMedia = input.IsMedia ?? false,
                    OrderBy = (int)(input.OrderBy ?? 0),
                    Rating = (int)(input.Rating ?? 0),
                    Type = (int)(input.Type ?? 0),
                    OrderId = input.OrderId ?? 0,
                    BookingId = input.BookingId ?? 0,
                    MaxResultCount = input.MaxResultCount,
                    SkipCount = input.SkipCount,
                }, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Items, "Get rates success", result.TotalCount);
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
        public async Task<DataResult> GetCountRateAsync(GetAllRateInputDto input)
        {
            try
            {
                GetCountRateResponse result = await _rateProtoClient.GetCountRateAsync(new GetCountRateRequest()
                {
                    TenantId = input.TenantId ?? 0,
                    ProviderId = input.ProviderId ?? 0,
                    ItemId = input.ItemId ?? 0,
                    UserId = input.UserId ?? 0,
                    IsComment = input.IsComment ?? false,
                    IsMedia = input.IsMedia ?? false,
                    OrderBy = (int)(input.OrderBy ?? 0),
                    Rating = (int)(input.Rating ?? 0),
                    Type = (int)(input.Type ?? 0),
                    OrderId = input.OrderId ?? 0,
                    BookingId = input.BookingId ?? 0,
                }, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result, "Get count rate success");
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
        public async Task<DataResult> GetRateByIdAsync(long id)
        {
            try
            {
                GetRateDetailRequest request = new() { Id = id };
                GetRateDetailResponse result = await _rateProtoClient.GetRateDetailAsync(request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Data, "Get rate detail success");
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
        public async Task<DataResult> CreateRateAsync(CreateRateInputDto input)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                CreateRateRequest request = new()
                {
                    TenantId = input.TenantId ?? 0,
                    ItemId = input.ItemId ?? 0,
                    ProviderId = input.ProviderId ?? 0,
                    RatePoint = input.RatePoint ?? 0,
                    Type = input.Type,
                    FileUrl = input.FileUrl ?? "",
                    Comment = input.Comment ?? "",
                    UserName = input.UserName ?? "",
                    Email = input.Email ?? "",
                    Avatar = user.ImageUrl ?? "",
                    AnswerRateId = input.AnswerRateId ?? 0,
                    OrderId = input.OrderId ?? 0,
                    BookingId = input.BookingId ?? 0,
                };
                await _rateProtoClient.CreateRateAsync(request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess("Create rate success!");
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

        public async Task<DataResult> UpdateRateAsync(UpdateRateByUserInputDto input)
        {
            try
            {
                UpdateRateByUserRequest request = new()
                {
                    Id = input.Id,
                    RatePoint = input.RatePoint ?? 0,
                    FileUrl = input.FileUrl ?? "",
                    Comment = input.Comment ?? "",
                    UserName = input.UserName ?? "",
                    Email = input.Email ?? "",
                    AnswerRateId = input.AnswerRateId ?? 0,
                };
                await _rateProtoClient.UpdateRateByUserAsync(request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess("Update rate success!");
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

        public async Task<DataResult> DeleteRateAsync(DeleteRateInputDto input)
        {
            try
            {
                DeleteRateByUserRequest request = new()
                {
                    Id = input.Id,
                };
                await _rateProtoClient.DeleteRateByUserAsync(request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess("Delete rate success!");
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
    }
}
