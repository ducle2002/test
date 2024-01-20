/*using Abp;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.UI;
using Google.Protobuf.WellKnownTypes;
using Yootek.App.Grpc;
using Yootek.Application.Protos.Business.Orders;
using Yootek.Application.Yootek.Services.Yootek.SmartSocial.Business.Orders.Dto;
using Yootek.Authorization;
using Yootek.Common.DataResult;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Yootek.Services.SmartSocial.Orders
{
    public interface IAdminOrderAppService : IApplicationService
    {
        // Task<object> GetAllOrdersAsync(GetOrdersInputDto input);
        Task<object> GetOrderByIdAsync(long id);
        Task<object> CreateOrderAsync(CreateOrderInputDto input);
        Task<object> UpdateOrderAsync(AdminUpdateOrderInputDto input);
        Task<object> UpdateStateOrderAsync(UpdateStateOrderInputDto input);
        Task<object> DeleteOrderAsync(long id);
    }
    [AbpAuthorize(PermissionNames.Social_Business_Orders)]
    public class AdminOrderAppService : YootekAppServiceBase, IAdminOrderAppService
    {
        private readonly OrderProtoGrpc.OrderProtoGrpcClient _orderProtoClient;
        public AdminOrderAppService(
            OrderProtoGrpc.OrderProtoGrpcClient orderProtoClient
            )
        {
            _orderProtoClient = orderProtoClient;
        }
        *//*public async Task<object> GetAllOrdersAsync(GetOrdersInputDto input)
        {
            try
            {
                GetAllOrdersRequest request = new()
                {
                    OrdererId = input.OrdererId ?? 0,
                    ProviderId = input.ProviderId ?? 0,
                    OrderBy = (int)(input.OrderBy ?? 0),
                    FormId = input.FormId ?? 0,
                    Search = input.Search ?? "",
                    MinPrice = input.MinPrice ?? 0,
                    MaxPrice = input.MaxPrice ?? 0,
                    DateFrom = input.DateFrom != null ? input.DateFrom.ToString() : (new DateTime(1970, 1, 1)).ToString(),
                    DateTo = input.DateTo != null ? input.DateTo.ToString() : (DateTime.Now).ToString(),
                    SkipCount = input.SkipCount,
                    MaxResultCount = input.MaxResultCount,
                };
                GetAllOrdersResponse result = await _orderProtoClient.GetAllOrdersAsync(request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Items, "Get orders success", result.TotalCount);
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
*//*
        public async Task<object> GetOrderByIdAsync(long id)
        {
            try
            {
                GetOrderByIdRequest request = new() { Id = id, };
                GetOrderByIdResponse result = await _orderProtoClient.GetOrderByIdAsync(
                    request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Data, "Get order detail success");
            }
            catch (UserFriendlyException)
            {
                throw new UserFriendlyException("Cannot get orders");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
        public async Task<object> CreateOrderAsync(CreateOrderInputDto input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(null))
                {
                    CreateOrderResponse result = await _orderProtoClient.CreateOrderAsync(new CreateOrderRequest()
                    {
                        OrdererId = (long)AbpSession.UserId,
                        TenantId = input.TenantId ?? 0,
                        ProviderId = input.ProviderId ?? 0,
                        Type = 1,
                        TotalPrice = input.TotalPrice ?? 0,
                        RecipientAddress = new PRecipientAddress()
                        {
                            City = input.RecipientAddress.City ?? "",
                            FullAddress = input.RecipientAddress.FullAddress ?? "",
                            District = input.RecipientAddress.FullAddress ?? "",
                            Name = input.RecipientAddress.Name ?? "",
                            Town = input.RecipientAddress.Town ?? "",
                            Phone = input.RecipientAddress.Phone ?? "",
                        },
                        OrderItemList =
                        {
                            input.OrderItemList.Select(x => new POrderItemModel()
                            {
                                Id = x.Id,
                                TenantId = x.TenantId ?? 0,
                                Name = x.Name,
                                ItemId = x.ItemId ?? 0,
                                Sku = x.Sku,
                                ImageUrl = x.ImageUrl,
                                OriginalPrice = x.OriginalPrice ?? 0,
                                CurrentPrice = x.CurrentPrice ?? 0,
                                Quantity = x.Quantity,
                                ItemName = x.ItemName,
                            })
                        },
                        PaymentMethod = (int)input.PaymentMethod,
                    }, MetadataGrpc.MetaDataHeader(AbpSession));
                    return DataResult.ResultSuccess(result.Data, "Create order success");
                }
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

        public async Task<object> UpdateOrderAsync(AdminUpdateOrderInputDto input)
        {
            try
            {
                UpdateOrderResponse result = await _orderProtoClient.UpdateOrderAsync(new UpdateOrderRequest()
                {
                    Id = input.Id,
                    RecipientAddress = new PRecipientAddress()
                    {
                        City = input.RecipientAddress.City ?? "",
                        FullAddress = input.RecipientAddress.FullAddress ?? "",
                        District = input.RecipientAddress.FullAddress ?? "",
                        Name = input.RecipientAddress.Name ?? "",
                        Town = input.RecipientAddress.Town ?? "",
                        Phone = input.RecipientAddress.Phone ?? "",
                    },
                    PaymentMethod = (int)input.PaymentMethod,
                }, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Data, "Update order success");
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

        public async Task<object> UpdateStateOrderAsync(UpdateStateOrderInputDto input)
        {
            try
            {
                UpdateStateOrderResponse result = await _orderProtoClient.UpdateStateOrderAsync(new UpdateStateOrderRequest()
                {
                    Id = input.Id,
                    CurrentState = input.CurrentState,
                    UpdateState = input.UpdateState,
                }, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Data, "Update state order success !");
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

        public async Task<object> DeleteOrderAsync(long id)
        {
            try
            {
                DeleteOrderResponse result = await _orderProtoClient.DeleteOrderAsync(new DeleteOrderRequest()
                {
                    Id = id
                }, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Data, "Delete order success");
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
*/