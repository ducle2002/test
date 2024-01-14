using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.UI;
using Grpc.Core;
using Yootek.App.Grpc;
using Yootek.App.ServiceHttpClient.Dto.Business;
using Yootek.App.ServiceHttpClient.Business;
using Yootek.Application.Protos.Business.Orders;
using Yootek.Application.Protos.Business.Providers;
using Yootek.Application.Protos.Business.Rates;
using Yootek.Application.Protos.Business.Vouchers;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.Notifications;
using Yootek.Services.Notifications;
using Yootek.Services.SmartSocial.Orders.Dto;
using static Yootek.Notifications.AppNotifier;
using static Yootek.Services.Notifications.AppNotifyBusiness;
using PRecipientAddress = Yootek.Application.Protos.Business.Orders.PRecipientAddress;

namespace Yootek.Services.SmartSocial.Orders
{
    public interface IOrderAppService : IApplicationService
    {
        Task<DataResult> GetAllOrdersByPartnerAsync(GetOrdersByPartnerInputDto input);
        Task<DataResult> GetAllOrdersByUserAsync(GetOrdersByUserInputDto input);
        Task<DataResult> GetOrderByIdAsync(long id);
        Task<DataResult> GetRevenueAsync(GetRevenueInputDto input);
        Task<DataResult> GetCountOrdersAsync(GetCountOrdersInputDto input);
        Task<DataResult> GetStatisticOrdersAsync(GetStatisticOrderInputDto input);
        Task<DataResult> GetItemRankingAsync(GetItemRankingInputDto input);
        Task<DataResult> CreateOrderAsync(CreateOrderByUserInputDto input);
        Task<DataResult> UpdateOrderAsync(UpdateOrderInputDto input);
        Task<DataResult> RatingOrderAsync(RatingOrderInputDto input);
        Task<DataResult> PartnerConfirmOrderAsync(ConfirmOrderInputDto input);
        Task<DataResult> PartnerRefuseOrderAsync(RefuseOrderInputDto input);
        Task<DataResult> PartnerCancelOrderAsync(CancelOrderInputDto input);
        Task<DataResult> UserConfirmOrderAsync(long id);
        Task<DataResult> UserCancelOrderAsync(CancelOrderInputDto input);
    }
    public class OrderAppService : YootekAppServiceBase, IOrderAppService
    {
        private readonly OrderProtoGrpc.OrderProtoGrpcClient _orderProtoClient;
        private readonly ProviderProtoGrpc.ProviderProtoGrpcClient _providerProtoClient;
        private readonly VoucherProtoGrpc.VoucherProtoGrpcClient _voucherProtoGrpc;
        private readonly IHttpPaymentService _httpPaymentService;
        private readonly IAppNotifier _appNotifier;
        private readonly IAppNotifyBusiness _appNotifyBusiness;
        private readonly RateProtoGrpc.RateProtoGrpcClient _rateProtoGrpc;
        public OrderAppService(
            OrderProtoGrpc.OrderProtoGrpcClient orderProtoClient,
            ProviderProtoGrpc.ProviderProtoGrpcClient providerProtoClient,
            VoucherProtoGrpc.VoucherProtoGrpcClient voucherProtoGrpcClient,
            RateProtoGrpc.RateProtoGrpcClient rateProtoGrpcClient,
            IHttpPaymentService httpPaymentService,
            IAppNotifyBusiness appNotifyBusiness,
            IAppNotifier appNotifier
            )
        {
            _orderProtoClient = orderProtoClient;
            _providerProtoClient = providerProtoClient;
            _voucherProtoGrpc = voucherProtoGrpcClient;
            _appNotifyBusiness = appNotifyBusiness;
            _httpPaymentService = httpPaymentService;
            _rateProtoGrpc = rateProtoGrpcClient;
            _appNotifier = appNotifier;
        }
        public async Task<DataResult> GetAllOrdersByPartnerAsync(GetOrdersByPartnerInputDto input)
        {
            try
            {
                GetAllOrdersByPartnerRequest request = new()
                {
                    TenantId = 0,
                    OrdererId = 0,
                    ProviderId = input.ProviderId ?? 0,
                    PartnerId = (long)AbpSession.UserId,
                    FormId = (int)input.FormId,
                    Search = input.Search ?? "",
                    MinPrice = input.MinPrice ?? 0,
                    MaxPrice = input.MaxPrice ?? 0,
                    OrderBy = (int)(input.OrderBy ?? 0),
                    DateFrom = ConvertDatetimeToTimestamp(input.DateFrom ?? new DateTime(1970, 1, 1)),
                    DateTo = ConvertDatetimeToTimestamp(input.DateTo ?? DateTime.Now),
                    SkipCount = input.SkipCount,
                    MaxResultCount = input.MaxResultCount,
                };
                GetAllOrdersByPartnerResponse result = await _orderProtoClient.GetAllOrdersByPartnerAsync(
                    request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Items, "Get orders by partner success", result.TotalCount);
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
        public async Task<DataResult> GetAllOrdersByUserAsync(GetOrdersByUserInputDto input)
        {
            try
            {
                GetAllOrdersByUserRequest request = new()
                {
                    OrderBy = (int)(input.OrderBy ?? 0),
                    OrdererId = (long)AbpSession.UserId,
                    ProviderId = 0,
                    PartnerId = 0,
                    FormId = (int)input.FormId,
                    Search = input.Search ?? "",
                    MinPrice = input.MinPrice ?? 0,
                    MaxPrice = input.MaxPrice ?? 0,
                    DateFrom = ConvertDatetimeToTimestamp(input.DateFrom ?? new DateTime(1970, 1, 1)),
                    DateTo = ConvertDatetimeToTimestamp(input.DateTo ?? DateTime.Now),
                    SkipCount = input.SkipCount,
                    MaxResultCount = input.MaxResultCount,
                };
                GetAllOrdersByUserResponse result = await _orderProtoClient.GetAllOrdersByUserAsync(
                    request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Items, "Get orders by user success", result.TotalCount);
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
        public async Task<DataResult> GetOrderByIdAsync(long id)
        {
            try
            {
                GetOrderByIdRequest request = new() { Id = id, };
                GetOrderByIdResponse response = await _orderProtoClient.GetOrderByIdAsync(
                    request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(response.Data, "Get order detail success");
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
        public async Task<DataResult> GetRevenueAsync(GetRevenueInputDto input)
        {
            try
            {
                GetRevenueRequest request = new()
                {
                    ProviderId = input.ProviderId,
                    PartnerId = (long)AbpSession.UserId,
                    Year = input.Year,
                    Month = input.Month,
                    Type = input.Type,
                };
                GetRevenueResponse response = await _orderProtoClient.GetRevenueAsync(
                    request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(response.Data, "Get revenue success");
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
        public async Task<DataResult> GetCountOrdersAsync(GetCountOrdersInputDto input)
        {
            try
            {
                GetCountOrdersRequest request = new()
                {
                    ProviderId = input.ProviderId ?? 0,
                    PartnerId = (long)AbpSession.UserId,
                    DateFrom = ConvertDatetimeToTimestamp(input.DateFrom ?? new DateTime(1970, 1, 1)),
                    DateTo = ConvertDatetimeToTimestamp(input.DateTo ?? DateTime.Now),
                };
                GetCountOrdersResponse response = await _orderProtoClient.GetCountOrdersAsync(
                    request, MetadataGrpc.MetaDataHeader(AbpSession));

                return DataResult.ResultSuccess(response.Data, "Get count orders success");
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
        public async Task<DataResult> GetStatisticOrdersAsync(GetStatisticOrderInputDto input)
        {
            try
            {
                GetStatisticRequest request = new()
                {
                    ProviderId = input.ProviderId,
                    FormId = (int)input.FormId,
                    Type = input.Type,
                    Day = input.Day,
                    Month = input.Month,
                    Year = input.Year,
                    DateFrom = ConvertDatetimeToTimestamp(input.DateFrom ?? new DateTime(1970, 1, 1)),
                    DateTo = ConvertDatetimeToTimestamp(input.DateTo ?? DateTime.Now),
                };
                GetStatisticResponse response = await _orderProtoClient.GetStatisticAsync(
                    request, MetadataGrpc.MetaDataHeader(AbpSession));

                return DataResult.ResultSuccess(response.Data, "Get statistic orders success");
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

        public async Task<DataResult> GetStatisticSomeProvidersAsync(GetStatisticSomeProvidersInputDto input)
        {
            try
            {
                GetStatisticSomeProvidersRequest request = new()
                {
                    ListProviderId = { input.ListProviderId },
                    FormId = (int)input.FormId,
                    Type = input.Type,
                    Day = input.Day,
                    Month = input.Month,
                    Year = input.Year,
                    DateFrom = ConvertDatetimeToTimestamp(input.DateFrom ?? new DateTime(1970, 1, 1)),
                    DateTo = ConvertDatetimeToTimestamp(input.DateTo ?? DateTime.Now),
                };
                GetStatisticSomeProvidersResponse response = await _orderProtoClient.GetStatisticSomeProvidersAsync(
                    request, MetadataGrpc.MetaDataHeader(AbpSession));

                return DataResult.ResultSuccess(response.Data, "Get statistic some providers success");
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

        public async Task<DataResult> GetItemRankingAsync(GetItemRankingInputDto input)
        {
            try
            {
                GetItemRankingRequest request = new()
                {
                    FormId = (int)input.FormId,
                    ProviderId = input.ProviderId,
                    TenantId = input.TenantId,
                    DateFrom = ConvertDatetimeToTimestamp(input.DateFrom ?? new DateTime(1970, 1, 1)),
                    DateTo = ConvertDatetimeToTimestamp(input.DateTo ?? DateTime.Now),
                    SortBy = (int)input.SortBy,
                };
                GetItemRankingResponse response = await _orderProtoClient.GetItemRankingAsync(
                    request, MetadataGrpc.MetaDataHeader(AbpSession));

                return DataResult.ResultSuccess(response.Items, "Get item ranking success");
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
        public async Task<DataResult> CreateOrderAsync(CreateOrderByUserInputDto input)
        {
            try
            {
                #region Validation
                PProvider provider = ValidateProvider(input.ProviderId);
                #endregion

                CreateOrderRequest request = new()
                {
                    OrdererId = (long)AbpSession.UserId,
                    TenantId = input.TenantId ?? 0,
                    ProviderId = input.ProviderId,
                    Type = 1,
                    PartnerId = provider.CreatorUserId,
                    Description = input.Description ?? "",
                    TotalPrice = input.TotalPrice,
                    RecipientAddress = new PRecipientAddress()
                    {
                        FullAddress = input.RecipientAddress.FullAddress ?? "",
                        Name = input.RecipientAddress.Name ?? "",
                        Phone = input.RecipientAddress.Phone ?? "",
                        ProvinceId = input.RecipientAddress.ProvinceId ?? "",
                        DistrictId = input.RecipientAddress.DistrictId ?? "",
                        WardId = input.RecipientAddress.WardId ?? "",
                    },
                    OrderItemList =
                    {
                        input.OrderItemList.Select(x => new POrderItemModelDto()
                        {
                            Id = x.Id,
                            Quantity = x.Quantity,
                        })
                    },
                    PaymentMethod = (int)input.PaymentMethod,
                    ListVouchers = { input.ListVouchers },
                };
                CreateOrderResponse result = await _orderProtoClient.CreateOrderAsync(request,
                    MetadataGrpc.MetaDataHeader(AbpSession));

                #region Push notification
                if (result.Data <= 0) throw new UserFriendlyException("Create order fail");
                
                await PushNotificationCreateOrder(new()
                {
                    UserId = (long)AbpSession.UserId,
                    PartnerId = provider.CreatorUserId,
                    TenantIdUser = AbpSession.TenantId ?? 0,
                    TenantIdPartner = provider.TenantId,
                    ProviderId = provider.Id,
                    ProviderName = provider.Name,
                    OrderId = result.Data,
                    TransactionId = result.Data,
                    ImageUrl = input.ImageUrl,
                });
                return DataResult.ResultSuccess(result.Data, "Create order success");
                #endregion
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
        public async Task<DataResult> UpdateOrderAsync(UpdateOrderInputDto input)
        {
            try
            {
                UpdateOrderRequest requestUpdate = new()
                {
                    Id = input.Id,
                    RecipientAddress = new PRecipientAddress()
                    {
                        FullAddress = input.RecipientAddress.FullAddress ?? "",
                        Name = input.RecipientAddress.Name ?? "",
                        Phone = input.RecipientAddress.Phone ?? "",
                        ProvinceId = input.RecipientAddress.ProvinceId ?? "",
                        DistrictId = input.RecipientAddress.DistrictId ?? "",
                        WardId = input.RecipientAddress.WardId ?? "",
                    },
                };
                await _orderProtoClient.UpdateOrderAsync(requestUpdate, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess("Update order success");
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
        public async Task<DataResult> RatingOrderAsync(RatingOrderInputDto input)
        {
            try
            {
                User user = await GetCurrentUserAsync();
                RatingOrderRequest request = new()
                {
                    Id = input.Id,
                    Items = { input.Items.Select(i => new PRateCreate()
                        {
                            TenantId = i.TenantId ?? 0,
                            ItemId = i.ItemId ?? 0,
                            ProviderId = i.ProviderId ?? 0,
                            RatePoint = i.RatePoint ?? 0,
                            Type = i.Type,
                            FileUrl = i.FileUrl ?? "",
                            Comment = i.Comment ?? "",
                            UserName = i.UserName ?? "",
                            Email = i.Email ?? "",
                            Avatar = user.ImageUrl ?? "",
                            AnswerRateId = i.AnswerRateId ?? 0,
                            OrderId = i.OrderId ?? 0,
                            BookingId = i.BookingId ?? 0,
                        }).ToList()
                    },
                };
                await _orderProtoClient.RatingOrderAsync(request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess("Rating order success");
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

        #region Partner Confirm Order
        public async Task<DataResult> PartnerConfirmOrderAsync(ConfirmOrderInputDto input)
        {
            try
            {
                #region Validation
                POrder order = await ValidateOrder(input.Id);
                // check partner
                if (order.PartnerId != AbpSession.UserId)
                    throw new UserFriendlyException("You don't have permission");
                PProvider provider = ValidateProvider(order.ProviderId);
                #endregion

                switch (input.Type)
                {
                    case TYPE_CONFIRM_ORDER.CONFIRM:
                        await ConfirmOrder(order, input);
                        break;
                    case TYPE_CONFIRM_ORDER.CONFIRM_SHIPPING:
                        await ConfirmShippingOrder(order, input);
                        await PushNotificationShippingOrder(new()
                        {
                            TenantIdUser = order.TenantId,
                            ProviderName = order.ProviderName,
                            UserId = order.CreatorUserId,
                            ImageUrl = order.OrderItemList[0].ImageUrl,
                            ProviderId = order.ProviderId,
                            OrderId = order.Id,
                            OrderCode = order.OrderCode,
                            TransactionId = order.Id,
                            TransactionCode = order.OrderCode,
                        });
                        break;
                    case TYPE_CONFIRM_ORDER.CONFIRM_SHIPPER_COMPLETED:
                        await ConfirmShipperCompletedOrder(order, input);
                        await PushNotificationShipperCompletedOrder(new()
                        {
                            UserId = order.CreatorUserId,
                            TenantIdUser = order.TenantId,
                            ImageUrl = order.OrderItemList[0].ImageUrl,
                            ProviderName = provider.Name,
                            ProviderId = provider.Id,
                            OrderId = order.Id,
                            OrderCode = order.OrderCode,
                            TransactionId = order.Id,
                            TransactionCode = order.OrderCode,
                        });
                        break;
                    case TYPE_CONFIRM_ORDER.CONFIRM_CANCEL:
                        await ConfirmCancelOrder(order, input);
                        await PushNotificationUserCancelOrder(new()
                        {
                            UserId = order.CreatorUserId,
                            PartnerId = order.PartnerId,
                            TenantIdUser = order.TenantId,
                            TenantIdPartner = provider.TenantId,
                            ImageUrl = order.OrderItemList[0].ImageUrl,
                            OrderId = order.Id,
                            OrderCode = order.OrderCode,
                            TransactionId = order.Id,
                            TransactionCode = order.OrderCode,
                            ProviderId = provider.Id,
                            ProviderName = provider.Name,
                        });
                        break;
                    case TYPE_CONFIRM_ORDER.CONFIRM_RETURN_REFUND:
                        await ConfirmReturnRefundOrder(order, input);
                        break;
                }
                return DataResult.ResultSuccess("Confirm order success!");
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
        private async Task ConfirmOrder(POrder order, ConfirmOrderInputDto input)
        {
            if (order.State == (int)STATE_ORDER.TO_PAY)
            {
                await _orderProtoClient.UpdateStateOrderAsync(
                    new UpdateStateOrderRequest()
                    {
                        Id = input.Id,
                        CurrentState = (int)STATE_ORDER.TO_PAY,
                        UpdateState = (int)STATE_ORDER.TO_SHIP_TO_PROCESS,
                    }, MetadataGrpc.MetaDataHeader(AbpSession));
            }
            else throw new UserFriendlyException("Cannot confirm order");
        }
        private async Task ConfirmShippingOrder(POrder order, ConfirmOrderInputDto input)
        {
            if (order.State == (int)STATE_ORDER.TO_SHIP_TO_PROCESS)
            {
                await _orderProtoClient.UpdateStateOrderAsync(
                    new UpdateStateOrderRequest()
                    {
                        Id = input.Id,
                        CurrentState = (int)STATE_ORDER.TO_SHIP_TO_PROCESS,
                        UpdateState = (int)STATE_ORDER.SHIPPING,
                    }, MetadataGrpc.MetaDataHeader(AbpSession));
            }
            else throw new UserFriendlyException("Cannot confirm shipping order");
        }
        private async Task ConfirmShipperCompletedOrder(POrder order, ConfirmOrderInputDto input)
        {
            if (order.State == (int)STATE_ORDER.SHIPPING)
            {
                await _orderProtoClient.UpdateStateOrderAsync(
                    new UpdateStateOrderRequest()
                    {
                        Id = input.Id,
                        CurrentState = (int)STATE_ORDER.SHIPPING,
                        UpdateState = (int)STATE_ORDER.SHIPPER_COMPLETED,
                    }, MetadataGrpc.MetaDataHeader(AbpSession));
            }
            else throw new UserFriendlyException("Cannot confirm shipper completed order");
        }
        private async Task ConfirmCancelOrder(POrder order, ConfirmOrderInputDto input)
        {
            if (order.State == (int)STATE_ORDER.CANCELLATION_TO_RESPOND)
            {
                await _orderProtoClient.UpdateStateOrderAsync(
                    new UpdateStateOrderRequest()
                    {
                        Id = input.Id,
                        CurrentState = (int)STATE_ORDER.CANCELLATION_TO_RESPOND,
                        UpdateState = (int)STATE_ORDER.CANCELLATION_CANCELLED,
                    }, MetadataGrpc.MetaDataHeader(AbpSession));
            }
            else throw new UserFriendlyException("Cannot confirm cancel order");
        }
        private async Task ConfirmReturnRefundOrder(POrder order, ConfirmOrderInputDto input)
        {
            if (order.State == (int)STATE_ORDER.RETURN_REFUND_NEW_REQUEST)
            {
                await _orderProtoClient.UpdateStateOrderAsync(
                    new UpdateStateOrderRequest()
                    {
                        Id = input.Id,
                        CurrentState = (int)STATE_ORDER.RETURN_REFUND_NEW_REQUEST,
                        UpdateState = (int)STATE_ORDER.RETURN_REFUND_COMPLETED,
                    }, MetadataGrpc.MetaDataHeader(AbpSession));
            }
            else throw new UserFriendlyException("Cannot confirm return_refund order");
        }
        #endregion

        #region Partner Refuse Order
        public async Task<DataResult> PartnerRefuseOrderAsync(RefuseOrderInputDto input)
        {
            #region Validation
            POrder order = await ValidateOrder(input.Id);
            // check partner
            if (order.PartnerId != AbpSession.UserId)
                throw new UserFriendlyException("You don't have permission");
            #endregion

            switch (input.Type)
            {
                case TYPE_REFUSE_ORDER.REFUSE:
                    await RefuseOrder(order, input);
                    break;
                case TYPE_REFUSE_ORDER.REFUSE_CANCEL:
                    await RefuseCancelOrder(order, input);
                    break;
                case TYPE_REFUSE_ORDER.REFUSE_RETURN_REFUND:
                    await RefuseReturnRefundOrder(order, input);
                    break;
            }
            return DataResult.ResultSuccess("Refuse order success!");
        }
        private async Task RefuseOrder(POrder order, RefuseOrderInputDto input)
        {
            if (order.State == (int)STATE_ORDER.TO_PAY
                || order.State == (int)STATE_ORDER.TO_SHIP_TO_PROCESS)
            {
                await _orderProtoClient.UpdateStateOrderAsync(
                    new UpdateStateOrderRequest()
                    {
                        Id = input.Id,
                        CurrentState = order.State,
                        UpdateState = (int)STATE_ORDER.CANCELLATION_CANCELLED,
                    }, MetadataGrpc.MetaDataHeader(AbpSession));
            }
            else throw new UserFriendlyException("Cannot refuse order");
        }
        private async Task RefuseCancelOrder(POrder order, RefuseOrderInputDto input)
        {
            if (order.State == (int)STATE_ORDER.CANCELLATION_TO_RESPOND)
            {
                await _orderProtoClient.UpdateStateOrderAsync(
                    new UpdateStateOrderRequest()
                    {
                        Id = input.Id,
                        CurrentState = (int)STATE_ORDER.CANCELLATION_TO_RESPOND,
                        UpdateState = (int)STATE_ORDER.TO_SHIP_TO_PROCESS,
                    }, MetadataGrpc.MetaDataHeader(AbpSession));
            }
            else throw new UserFriendlyException("Cannot refuse cancel order");
        }
        private async Task RefuseReturnRefundOrder(POrder order, RefuseOrderInputDto input)
        {
            if (order.State == (int)STATE_ORDER.RETURN_REFUND_NEW_REQUEST)
            {
                await _orderProtoClient.UpdateStateOrderAsync(
                    new UpdateStateOrderRequest()
                    {
                        Id = input.Id,
                        CurrentState = (int)STATE_ORDER.RETURN_REFUND_NEW_REQUEST,
                        UpdateState = (int)STATE_ORDER.SHIPPER_COMPLETED,
                    }, MetadataGrpc.MetaDataHeader(AbpSession));
            }
            else throw new UserFriendlyException("Cannot refuse cancel order");
        }
        #endregion

        // Partner chủ động hủy đơn
        public async Task<DataResult> PartnerCancelOrderAsync(CancelOrderInputDto input)
        {
            try
            {
                #region Validation
                POrder order = await ValidateOrder(input.Id);
                // check user
                if (order.PartnerId != AbpSession.UserId)
                    throw new UserFriendlyException("You don't have permission");
                PProvider provider = ValidateProvider(order.ProviderId);
                #endregion

                // check state order
                if (order.State == (int)STATE_ORDER.TO_PAY
                    || order.State == (int)STATE_ORDER.TO_SHIP_TO_PROCESS)
                {
                    CancelOrderResponse result = await _orderProtoClient.CancelOrderAsync(
                       new CancelOrderRequest()
                       {
                           Id = input.Id,
                           Reason = input.Reason ?? "",
                           UpdateState = (int)STATE_ORDER.CANCELLATION_CANCELLED,
                       }, MetadataGrpc.MetaDataHeader(AbpSession));
                    // push notification
                    await PushNotificationPartnerCancelOrder(new()
                    {
                        UserId = order.CreatorUserId,
                        PartnerId = order.PartnerId,
                        TenantIdUser = order.TenantId,
                        TenantIdPartner = provider.TenantId,
                        ImageUrl = order.OrderItemList[0].ImageUrl,
                        OrderId = order.Id,
                        OrderCode = order.OrderCode,
                        TransactionId = order.Id,
                        TransactionCode = order.OrderCode,
                        ProviderId = provider.Id,
                        ProviderName = provider.Name,
                    });
                    return DataResult.ResultSuccess(result.Data, "Cancellation request has been sent successfully!");
                }
                throw new UserFriendlyException("Cannot cancel order");
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

        // User
        public async Task<DataResult> UserConfirmOrderAsync(long id)
        {
            try
            {
                #region Validation
                POrder order = await ValidateOrder(id);
                // check user
                if (order.CreatorUserId != AbpSession.UserId)
                    throw new UserFriendlyException("You don't have permission");
                // check state order
                if (order.State != (int)STATE_ORDER.SHIPPER_COMPLETED)
                    throw new UserFriendlyException("Can not confirm completed order");
                #endregion

                UpdateStateOrderResponse result = await _orderProtoClient.UpdateStateOrderAsync(
                    new UpdateStateOrderRequest()
                    {
                        Id = id,
                        CurrentState = (int)STATE_ORDER.SHIPPER_COMPLETED,
                        UpdateState = (int)STATE_ORDER.USER_COMPLETED,
                    }, MetadataGrpc.MetaDataHeader(AbpSession));

                return DataResult.ResultSuccess(result.Data, "Order completion confirmation!");
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
        public async Task<DataResult> UserCancelOrderAsync(CancelOrderInputDto input)
        {
            try
            {
                #region Validation
                POrder order = await ValidateOrder(input.Id);
                // check user
                if (order.CreatorUserId != AbpSession.UserId)
                    throw new UserFriendlyException("You don't have permission");
                GetProviderByIdResponse resProvider = await _providerProtoClient.GetProviderByIdAsync(new GetProviderByIdRequest()
                {
                    Id = order.ProviderId,
                    IsDataStatic = false,
                }, MetadataGrpc.MetaDataHeader(AbpSession));
                #endregion

                // check state order
                if (order.State == (int)STATE_ORDER.TO_PAY)
                {
                    CancelOrderResponse result = await _orderProtoClient.CancelOrderAsync(
                       new CancelOrderRequest()
                       {
                           Id = input.Id,
                           Reason = input.Reason ?? "",
                           UpdateState = (int)STATE_ORDER.CANCELLATION_CANCELLED,
                       }, MetadataGrpc.MetaDataHeader(AbpSession));
                    // push notification
                    await PushNotificationUserCancelOrder(new()
                    {
                        UserId = order.CreatorUserId,
                        PartnerId = order.PartnerId,
                        TenantIdUser = order.TenantId,
                        TenantIdPartner = resProvider.Data.TenantId,
                        ImageUrl = order.OrderItemList[0].ImageUrl,
                        OrderId = order.Id,
                        OrderCode = order.OrderCode,
                        TransactionId = order.Id,
                        TransactionCode = order.OrderCode,
                        ProviderId = resProvider.Data.Id,
                    });
                    return DataResult.ResultSuccess(result.Data, "Cancellation request has been sent successfully!");
                }
                if (order.State == (int)STATE_ORDER.TO_SHIP_TO_PROCESS)
                {
                    CancelOrderResponse result = await _orderProtoClient.CancelOrderAsync(
                        new CancelOrderRequest()
                        {
                            Id = input.Id,
                            Reason = input.Reason ?? "",
                            UpdateState = (int)STATE_ORDER.CANCELLATION_TO_RESPOND,
                        }, MetadataGrpc.MetaDataHeader(AbpSession));

                    // push notification
                    await PushNotificationUserRequestCancelOrder(new()
                    {
                        PartnerId = order.PartnerId,
                        TenantIdPartner = resProvider.Data.TenantId,
                        ImageUrl = order.OrderItemList[0].ImageUrl,
                        OrderId = order.Id,
                        OrderCode = order.OrderCode,
                        TransactionId = order.Id,
                        TransactionCode = order.OrderCode,
                        ProviderId = resProvider.Data.Id,
                        ProviderName = resProvider.Data.Name,
                    });
                    return DataResult.ResultSuccess(result.Data, "Cancellation request has been sent successfully!");
                }
                throw new UserFriendlyException("Cannot cancel order");
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

        #region Validation 
        private async Task<POrder> ValidateOrder(long orderId)
        {
            GetOrderByIdResponse response = await _orderProtoClient.GetOrderByIdAsync(
                new GetOrderByIdRequest() { Id = (long)orderId }, MetadataGrpc.MetaDataHeader(AbpSession));
            if (response.Data == null) throw new UserFriendlyException("Order not found");
            return response.Data;
        }
        private PProvider ValidateProvider(long? providerId)
        {
            if (providerId == null) throw new UserFriendlyException("ProviderId is required");
            GetProviderByIdResponse response = _providerProtoClient.GetProviderById(
                    new GetProviderByIdRequest() { Id = (long)providerId, IsDataStatic = false }, MetadataGrpc.MetaDataHeader(AbpSession));
            if (response.Data == null) throw new UserFriendlyException("Provider not found");
            return response.Data;
        }
        #endregion

        #region Notification
        private async Task PushNotificationCreateOrder(NotificationMessage message) //Tạo đơn hàng
        {
            // user
            await _appNotifyBusiness.SendNotifyFullyToUser(new()
            {
                TenantId = (int)message.TenantIdUser,
                Title = "Đặt hàng thành công",
                Message = "Đơn hàng đang được xử lý bởi người bán",
                Action = AppNotificationAction.CreateOrderSuccess,
                Icon = AppNotificationIcon.CreateOrderSuccessIcon,
                AppType = (int)AppType.APP_USER,
                Type = (int)TYPE_NOTIFICATION.SOCIAL,
                UserId = (long)message.UserId,
                PageId = (int)TAB_ID.NOTIFICATION_USER_SHOPPING,
                ProviderId = message.ProviderId,
                OrderId = message.OrderId,
                TransactionId = message.TransactionId,
                ImageUrl = message.ImageUrl,
            });

            // partner
            await _appNotifyBusiness.SendNotifyFullyToUser(new()
            {
                TenantId = (int)message.TenantIdPartner,
                Title = "Bạn có một đơn hàng mới của gian hàng " + message.ProviderName,
                Message = $"Đơn hàng được đặt vào lúc {FormatDateTime(DateTime.Now)}",
                Action = AppNotificationAction.CreateOrderSuccess,
                Icon = AppNotificationIcon.CreateOrderSuccessIcon,
                AppType = (int)AppType.APP_PARTNER,
                Type = (int)TYPE_NOTIFICATION.SOCIAL,
                UserId = (long)message.PartnerId,
                PageId = (int)TAB_ID.NOTIFICATION_PARTNER,
                ProviderId = message.ProviderId,
                OrderId = (long)message.OrderId,
                ImageUrl = message.ImageUrl,
                TransactionId = message.TransactionId,
            });
        }

        private async Task PushNotificationShippingOrder(NotificationMessage message) // Đơn hàng đang được giao
        {
            await _appNotifyBusiness.SendNotifyFullyToUser(new()
            {
                TenantId = (int)message.TenantIdUser,
                Title = "Đang giao hàng",
                Message = "Đơn hàng với mã vận đơn " + message.TransactionCode + " từ Người bán " + message.ProviderName
                    + " đang trên đường giao tới bạn.",
                Action = AppNotificationAction.ShippingOrder,
                Icon = AppNotificationIcon.ShippingOrderIcon,
                AppType = (int)AppType.APP_USER,
                Type = (int)TYPE_NOTIFICATION.SOCIAL,
                PageId = (int)TAB_ID.NOTIFICATION_USER_SHOPPING,
                UserId = (long)message.UserId,
                ImageUrl = message.ImageUrl,
                OrderId = (long)message.OrderId,
                TransactionId = message.TransactionId,
            });
        }

        private async Task PushNotificationShipperCompletedOrder(NotificationMessage message) //Shipper giao hàng thành công
        {
            await _appNotifyBusiness.SendNotifyFullyToUser(new()
            {
                TenantId = (int)message.TenantIdUser,
                Title = "Xác nhận đã nhận hàng",
                Message = "Vui lòng chọn 'Đã nhận được hàng' cho đơn hàng " + message.TransactionCode,
                Action = AppNotificationAction.ShipperCompleted,
                Icon = AppNotificationIcon.ShipperCompletedIcon,
                AppType = (int)AppType.APP_USER,
                PageId = (int)TAB_ID.NOTIFICATION_USER_SHOPPING,
                Type = (int)TYPE_NOTIFICATION.SOCIAL,
                UserId = (long)message.UserId,
                ImageUrl = message.ImageUrl,
                OrderId = (long)message.OrderId,
                TransactionId = message.TransactionId,
            });
        }
        private async Task PushNotificationUserCancelOrder(NotificationMessage message) // Huỷ đơn hàng
        {
            await _appNotifyBusiness.SendNotifyFullyToUser(new()
            {
                TenantId = (int)message.TenantIdUser,
                Title = "Yêu cầu huỷ đơn hàng đã được chấp nhận",
                Message = "Yêu cầu huỷ đơn hàng của bạn đã được chấp nhận. Đơn hàng với mã " + message.TransactionCode + " đã được huỷ thành công.",
                Action = AppNotificationAction.UserCancel,
                Icon = AppNotificationIcon.UserCancelIcon,
                AppType = (int)AppType.APP_USER,
                PageId = (int)TAB_ID.NOTIFICATION_USER_SHOPPING,
                Type = (int)TYPE_NOTIFICATION.SOCIAL,
                UserId = (long)message.UserId,
                OrderId = message.OrderId,
                ImageUrl = message.ImageUrl,
                TransactionId = message.TransactionId,
            });
            await _appNotifyBusiness.SendNotifyFullyToUser(new()
            {
                TenantId = (int)message.TenantIdPartner,
                Title = "Đơn hàng của gian hàng " + message.ProviderName + " đã bị huỷ",
                Message = "Đơn hàng với mã " + message.TransactionCode + " đã bị huỷ.",
                Action = AppNotificationAction.UserCancel,
                Icon = AppNotificationIcon.UserCancelIcon,
                AppType = (int)AppType.APP_PARTNER,
                PageId = (int)TAB_ID.NOTIFICATION_PARTNER,
                Type = (int)TYPE_NOTIFICATION.SOCIAL,
                ProviderId = message.ProviderId,
                ImageUrl = message.ImageUrl,
                UserId = (long)message.PartnerId,
                OrderId = message.OrderId,
                TransactionId = message.TransactionId,
            });
        }
        private async Task PushNotificationUserRequestCancelOrder(NotificationMessage message) // Yêu cầu huỷ đơn hàng
        {
            await _appNotifyBusiness.SendNotifyFullyToUser(new()
            {
                TenantId = (int)message.TenantIdPartner,
                Title = "Bạn có một yêu cầu huỷ đơn mới",
                Message = $"Người mua yêu cầu hủy đơn hàng vào {FormatDateTime(DateTime.Now)}",
                Action = AppNotificationAction.UserRequestCancel,
                Icon = AppNotificationIcon.UserRequestCancelIcon,
                AppType = (int)AppType.APP_PARTNER,
                PageId = (int)TAB_ID.NOTIFICATION_PARTNER,
                Type = (int)TYPE_NOTIFICATION.SOCIAL,
                UserId = (long)message.PartnerId,
                ProviderId = message.ProviderId,
                OrderId = (long)message.OrderId,
                TransactionId = message.TransactionId,
                ImageUrl = message.ImageUrl,
            });
        }
        private async Task PushNotificationPartnerCancelOrder(NotificationMessage message) // Nhà cung cấp huỷ đơn hàng
        {
            await _appNotifyBusiness.SendNotifyFullyToUser(new()
            {
                TenantId = (int)message.TenantIdUser,
                Title = "Đơn hàng tại gian hàng " + message.ProviderName + " đã bị huỷ",
                Message = "Đơn hàng với mã " + message.TransactionCode + " của bạn đã bị huỷ bởi người bán.",
                Action = AppNotificationAction.PartnerCancelOrder,
                Icon = AppNotificationIcon.PartnerCancelOrderIcon,
                AppType = (int)AppType.APP_USER,
                PageId = (int)TAB_ID.NOTIFICATION_USER_SHOPPING,
                Type = (int)TYPE_NOTIFICATION.SOCIAL,
                UserId = (long)message.UserId,
                ImageUrl = message.ImageUrl,
                OrderId = message.OrderId,
                TransactionId = message.TransactionId,
            });
        }
        #endregion
    }
}
