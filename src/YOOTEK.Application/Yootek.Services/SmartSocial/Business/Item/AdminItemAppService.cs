using Abp.Application.Services;
using Abp.UI;
using Grpc.Core;
using Yootek.App.Grpc;
using Yootek.Application.Protos.Business.Items;
using Yootek.Common.DataResult;
using Yootek.Services.SmartSocial.Items.Dto;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Yootek.Services.SmartSocial.Items
{
    public interface IAdminItemAppService : IApplicationService
    {
        /// <summary>
        /// 1. Admin :
        /// a. Lấy danh sách tất cả sản phẩm
        /// b. Update trạng thái của sản phẩm : duyệt or not
        /// </summary>

        // Task<DataResult> GetAllItemsByAdminAsync(GetItemsByAdminInputDto input);
        Task<DataResult> GetItemByIdAsync(long id);
        Task<DataResult> DeleteItemAsync(long id);
        Task<DataResult> ApproveListItemsAsync(ApproveListItemsInputDto input);
        Task<DataResult> ApproveItemAsync(ApproveItemInputDto input);
    }
    //[AbpAuthorize(PermissionNames.Social_Business_Items)]
    public class AdminItemAppService : YootekAppServiceBase, IAdminItemAppService
    {
        private readonly ItemProtoGrpc.ItemProtoGrpcClient _itemProtoClient;
        public AdminItemAppService(
            ItemProtoGrpc.ItemProtoGrpcClient itemProtoClient
            )
        {
            _itemProtoClient = itemProtoClient;
        }
        /*public async Task<DataResult> GetAllItemsByAdminAsync(GetItemsByAdminInputDto input)
        {
            try
            {
                GetItemsByAdminResponse result = await _itemProtoClient.GetItemsByAdminAsync(new GetItemsByAdminRequest()
                {
                    Search = input.Search ?? "",
                    CategoryId = input.CategoryId ?? 0,
                    ProviderId = input.ProviderId ?? 0,
                    MinPrice = input.MinPrice ?? 0,
                    MaxPrice = input.MaxPrice ?? 0,
                    MinStock = input.MinStock ?? 0,
                    MaxStock = input.MaxStock ?? 0,
                    MinSales = input.MinSales ?? 0,
                    MaxSales = input.MaxSales ?? 0,
                    OrderBy = (int)(input.OrderBy ?? 0),
                    FormId = input.FormId ?? 0,
                    TenantId = AbpSession.TenantId ?? 0,
                    Rating = input.Rating ?? 0,
                    Condition = (int)(input.Condition ?? 0),
                    SkipCount = input.SkipCount,
                    MaxResultCount = input.MaxResultCount,
                }, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Items, "Get items success", result.TotalCount);
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
        }*/

        public async Task<DataResult> GetItemByIdAsync(long id)
        {
            try
            {
                GetItemDetailRequest request = new() { Id = id };
                GetItemDetailResponse result = await _itemProtoClient.GetItemDetailAsync(request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result, "Get item detail success");
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
        public async Task<DataResult> ApproveListItemsAsync(ApproveListItemsInputDto input)
        {
            try
            {
                UpdateListStatusOfItemRequest request = new()
                {
                    Items = {
                        input.Ids.Select(id => new UpdateStatusOfItemRequest()
                        {
                            Id = id,
                            CurrentStatus = (int)ItemStatus.PENDING,
                            UpdateStatus = (int)ItemStatus.ACTIVATED,
                        }),
                    },
                };
                UpdateListStatusOfItemResponse result = await _itemProtoClient.UpdateListStatusOfItemAsync(
                    request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Data, "Approve list item success");
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
        public async Task<DataResult> ApproveItemAsync(ApproveItemInputDto input)
        {
            try
            {
                UpdateStatusOfItemRequest request = new()
                {
                    Id = input.Id,
                    CurrentStatus = (int)ItemStatus.PENDING,
                    UpdateStatus = (int)ItemStatus.ACTIVATED,
                };
                UpdateStatusOfItemResponse result = await _itemProtoClient.UpdateStatusOfItemAsync(request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Data, "Approve item success");
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
        public async Task<DataResult> DeleteItemAsync(long id)
        {
            try
            {
                DeleteItemResponse result = await _itemProtoClient.DeleteItemAsync(new DeleteItemRequest()
                {
                    Id = id
                }, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Data, "Delete item success");
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
