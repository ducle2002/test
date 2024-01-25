using Abp.Application.Services;
using Abp.Authorization;
using Abp.UI;
using Grpc.Core;
using Yootek.App.Grpc;
using Yootek.Application.Protos.Business.Rates;
using Yootek.Authorization;
using Yootek.Common.DataResult;
using Yootek.Services.SmartSocial.Rates.Dto;
using System;
using System.Threading.Tasks;

namespace Yootek.Services.SmartSocial.Rates
{
    public interface IAdminRateAppService : IApplicationService
    {
        Task<DataResult> GetAllRatesAsync(GetAllRateInputDto input);
        Task<DataResult> GetRateByIdAsync(long id);
        Task<DataResult> CreateRateAsync(CreateRateInputDto input);
        Task<DataResult> UpdateRateAsync(UpdateRateInputDto input);
        Task<DataResult> DeleteRateAsync(long id);
    }

    [AbpAuthorize]
    public class AdminRateAppService : YootekAppServiceBase, IAdminRateAppService
    {
        private readonly RateProtoGrpc.RateProtoGrpcClient _rateProtoClient;
        public AdminRateAppService(
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
                    MaxResultCount = input.MaxResultCount,
                    SkipCount = input.SkipCount,
                }, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Items, "Get rates success", result.TotalCount);
            }
            catch (RpcException ex)
            {
                throw;
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
                throw;
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
                    AnswerRateId = input.AnswerRateId ?? 0,
                };
                await _rateProtoClient.CreateRateAsync(request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess("Create rate success!");
            }
            catch (RpcException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<DataResult> UpdateRateAsync(UpdateRateInputDto input)
        {
            try
            {
                UpdateRateRequest request = new()
                {
                    Id = input.Id,
                    RatePoint = input.RatePoint ?? 0,
                    FileUrl = input.FileUrl ?? "",
                    Comment = input.Comment ?? "",
                    UserName = input.UserName ?? "",
                    Email = input.Email ?? "",
                    AnswerRateId = input.AnswerRateId ?? 0,
                };
                UpdateRateResponse res = await _rateProtoClient.UpdateRateAsync(request, MetadataGrpc.MetaDataHeader(AbpSession));

                return DataResult.ResultSuccess("Update rate success!");
            }
            catch (RpcException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
        public async Task<DataResult> DeleteRateAsync(long id)
        {
            try
            {
                DeleteRateRequest request = new() { Id = id };
                await _rateProtoClient.DeleteRateAsync(request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess("Delete rate success!");
            }
            catch (RpcException ex)
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
