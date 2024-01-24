using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.UI;
using Grpc.Core;
using Yootek.App.Grpc;
using Yootek.Application.Protos.Business.Bookings;
using Yootek.Application.Protos.Business.Providers;
using Yootek.Common.DataResult;
using Yootek.Notifications;
using Yootek.Services.Notifications;
using Yootek.Services.SmartSocial.Bookings.Dto;
using Yootek.Services.SmartSocial.Orders.Dto;
using static Yootek.Notifications.AppNotifier;
using static Yootek.Services.Notifications.AppNotifyBusiness;
using Enum = System.Enum;
using PProvider = Yootek.Application.Protos.Business.Providers.PProvider;
using PRecipientAddress = Yootek.Application.Protos.Business.Bookings.PRecipientAddress;

namespace Yootek.Services.SmartSocial.Bookings
{
    public interface IBookingAppService : IApplicationService
    {
        Task<DataResult> GetAllBookingsByPartnerAsync(GetBookingsByPartnerInputDto input);
        Task<DataResult> GetAllBookingsByUserAsync(GetBookingsByUserInputDto input);
        Task<DataResult> GetBookingByIdAsync(long id);
        Task<DataResult> CreateBookingAsync(CreateBookingInputDto input);
        Task<DataResult> UpdateBookingAsync(UpdateBookingInputDto input);
        Task<DataResult> PartnerConfirmBookingAsync(ConfirmBookingInputDto input);
        Task<DataResult> PartnerRefuseBookingAsync(RefuseBookingInputDto input);
        Task<DataResult> PartnerCancelBookingAsync(CancelBookingInputDto input);
        Task<DataResult> UserConfirmBookingAsync(long id);
        Task<DataResult> UserCancelBookingAsync(CancelBookingInputDto input);
    }
    public class BookingAppService : YootekAppServiceBase, IBookingAppService
    {
        private readonly BookingProtoGrpc.BookingProtoGrpcClient _bookingProtoClient;
        private readonly ProviderProtoGrpc.ProviderProtoGrpcClient _providerProtoClient;
        private readonly IAppNotifier _appNotifier;
        private readonly IAppNotifyBusiness _appNotifyBusiness;
        public BookingAppService(
            BookingProtoGrpc.BookingProtoGrpcClient bookingProtoClient,
            ProviderProtoGrpc.ProviderProtoGrpcClient providerProtoClient,
            IAppNotifyBusiness appNotifyBusiness,
            IAppNotifier appNotifier
            )
        {
            _bookingProtoClient = bookingProtoClient;
            _providerProtoClient = providerProtoClient;
            _appNotifyBusiness = appNotifyBusiness;
            _appNotifier = appNotifier;
        }

        public async Task<DataResult> GetAllBookingsByPartnerAsync(GetBookingsByPartnerInputDto input)
        {
            try
            {
                #region Validation 
                ValidateFormIdPartner((int)input.FormId);
                #endregion

                GetAllBookingsByPartnerRequest request = new()
                {
                    BookerId = input.BookerId ?? 0,
                    ProviderId = input.ProviderId ?? 0,
                    PartnerId = (long)AbpSession.UserId,
                    FormId = (int)(input.FormId ?? 0),
                    OrderBy = (int)(input.OrderBy ?? 0),
                    Type = input.Type ?? 0,
                    Search = input.Search ?? "",
                    MinPrice = input.MinPrice ?? 0,
                    MaxPrice = input.MaxPrice ?? 0,
                    DateFrom = ConvertDatetimeToTimestamp(input.DateFrom ?? new DateTime(1970, 1, 1)),
                    DateTo = ConvertDatetimeToTimestamp(input.DateTo ?? DateTime.Now),
                    SkipCount = input.SkipCount,
                    MaxResultCount = input.MaxResultCount,
                };
                GetAllBookingsByPartnerResponse result = await _bookingProtoClient.GetAllBookingsByPartnerAsync(
                    request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Items, "Get bookings success", result.TotalCount);
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
        public async Task<DataResult> GetAllBookingsByUserAsync(GetBookingsByUserInputDto input)
        {
            try
            {
                #region Validation 
                ValidateFormIdUser((int)input.FormId);
                #endregion

                GetAllBookingsByUserRequest request = new()
                {
                    BookerId = (long)AbpSession.UserId,
                    ProviderId = input.ProviderId ?? 0,
                    PartnerId = 0,
                    FormId = (int)(input.FormId ?? 0),
                    OrderBy = (int)(input.OrderBy ?? 0),
                    Type = input.Type ?? 0,
                    Search = input.Search ?? "",
                    MinPrice = input.MinPrice ?? 0,
                    MaxPrice = input.MaxPrice ?? 0,
                    DateFrom = ConvertDatetimeToTimestamp(input.DateFrom ?? new DateTime(1970, 1, 1)),
                    DateTo = ConvertDatetimeToTimestamp(input.DateTo ?? DateTime.Now),
                    SkipCount = input.SkipCount,
                    MaxResultCount = input.MaxResultCount,
                };
                GetAllBookingsByUserResponse result = await _bookingProtoClient.GetAllBookingsByUserAsync(
                    request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Items, "Get bookings success", result.TotalCount);
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
        public async Task<DataResult> GetBookingByIdAsync(long id)
        {
            try
            {
                GetBookingByIdRequest request = new() { Id = id, };
                GetBookingByIdResponse response = await _bookingProtoClient.GetBookingByIdAsync(
                    request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(response.Data, "Get booking detail success");
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
        public async Task<DataResult> CreateBookingAsync(CreateBookingInputDto input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(null))
                {
                    #region Validation
                    PProvider provider = ValidateProvider(input.ProviderId);
                    #endregion 

                    CreateBookingResponse result = await _bookingProtoClient.CreateBookingAsync(new CreateBookingRequest()
                    {
                        TenantId = input.TenantId ?? 0,
                        BookerId = (long)AbpSession.UserId,
                        ProviderId = input.ProviderId ?? 0,
                        PartnerId = provider.CreatorUserId,
                        Name = input.Name,
                        PhoneNumber = input.PhoneNumber,
                        Email = input.Email,
                        Description = input.Description,
                        TotalPrice = input.TotalPrice ?? 0,
                        Type = input.Type,
                        CheckIn = ConvertDatetimeToTimestamp(input.CheckIn),
                        CheckOut = ConvertDatetimeToTimestamp(input.CheckOut),
                        RecipientAddress = new PRecipientAddress()
                        {
                            FullAddress = input.RecipientAddress.FullAddress ?? "",
                            Name = input.RecipientAddress.Name ?? "",
                            Phone = input.RecipientAddress.Phone ?? "",
                            ProvinceId = input.RecipientAddress.ProvinceId ?? "",
                            DistrictId = input.RecipientAddress.DistrictId ?? "",
                            WardId = input.RecipientAddress.WardId ?? "",
                        },
                        BookingItemList =
                        {
                            input.BookingItemList.Select(x => new PBookingItemModel()
                            {
                                Id = x.Id,
                                TenantId = x.TenantId ?? 0,
                                IsDefault = x.IsDefault ?? false,
                                Name = x.Name,
                                ItemId = x.ItemId ?? 0,
                                ImageUrl = x.ImageUrl,
                                OriginalPrice = x.OriginalPrice ?? 0,
                                CurrentPrice = x.CurrentPrice ?? 0,
                                Quantity = x.Quantity,
                                ItemName = x.ItemName,
                            })
                        },
                        PaymentMethod = (int)input.PaymentMethod,
                    }, MetadataGrpc.MetaDataHeader(AbpSession));
                    // push notification
                    await PushNotificationCreateBooking(new()
                    {
                        UserId = (long)AbpSession.UserId,
                        PartnerId = provider.CreatorUserId,
                        TenantIdUser = AbpSession.TenantId ?? 0,
                        TenantIdPartner = provider.TenantId,
                        ProviderId = provider.Id,
                        ProviderName = provider.Name,
                        ImageUrl = input.BookingItemList[0].ImageUrl,
                        BookingName = input.Name,
                        TransactionName = input.Name,
                    });
                    return DataResult.ResultSuccess(result.Data, "Create booking success");
                }
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
        public async Task<DataResult> UpdateBookingAsync(UpdateBookingInputDto input)
        {
            try
            {
                #region Validation
                PBooking booking = await ValidateBooking(input.Id);
                #endregion

                // check user or partner 
                if ((booking.CreatorUserId == AbpSession.UserId
                    && booking.State == (int)STATE_BOOKING.WAIT_FOR_CONFIRM)
                    || (booking.PartnerId == AbpSession.UserId
                    && (booking.State == (int)STATE_BOOKING.WAIT_FOR_CONFIRM)))
                {
                    UpdateBookingRequest request = new()
                    {
                        Id = input.Id,
                        Name = input.Name,
                        PhoneNumber = input.PhoneNumber,
                        Email = input.Email,
                        Description = input.Description,
                        CheckIn = ConvertDatetimeToTimestamp(input.CheckIn),
                        CheckOut = ConvertDatetimeToTimestamp(input.CheckOut),
                        RecipientAddress = new PRecipientAddress()
                        {
                            FullAddress = input.RecipientAddress.FullAddress ?? "",
                            Name = input.RecipientAddress.Name ?? "",
                            Phone = input.RecipientAddress.Phone ?? "",
                            ProvinceId = input.RecipientAddress.ProvinceId ?? "",
                            DistrictId = input.RecipientAddress.DistrictId ?? "",
                            WardId = input.RecipientAddress.WardId ?? "",
                        },
                        PaymentMethod = (int)input.PaymentMethod,
                    };
                    UpdateBookingResponse result = await _bookingProtoClient.UpdateBookingAsync(
                        request, MetadataGrpc.MetaDataHeader(AbpSession));
                    return DataResult.ResultSuccess(result.Data, "Update booking success");
                }
                throw new UserFriendlyException("You don't have permission || Booking cannot update!");
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

        // Partner 
        #region Partner confirm
        public async Task<DataResult> PartnerConfirmBookingAsync(ConfirmBookingInputDto input)
        {
            try
            {
                #region Validation
                PBooking booking = await ValidateBooking(input.Id);
                // check partner 
                if (booking.PartnerId != AbpSession.UserId)
                    throw new UserFriendlyException("You don't have permission");
                GetProviderByIdResponse resProvider = await _providerProtoClient.GetProviderByIdAsync(new GetProviderByIdRequest()
                {
                    Id = booking.ProviderId,
                    IsDataStatic = false,
                }, MetadataGrpc.MetaDataHeader(AbpSession));
                #endregion

                switch (input.Type)
                {
                    case TYPE_CONFIRM_BOOKING.CONFIRM:
                        await ConfirmAsync(booking, input);
                        await PushNotificationPartnerConfirmBooking(new()
                        {
                            UserId = booking.CreatorUserId,
                            PartnerId = resProvider.Data.CreatorUserId,
                            TenantIdUser = AbpSession.TenantId ?? 0,
                            TenantIdPartner = resProvider.Data.TenantId,
                            ProviderId = resProvider.Data.Id,
                            ProviderName = resProvider.Data.Name,
                            ImageUrl = booking.BookingItemList[0].ImageUrl,
                            BookingName = booking.Name,
                            BookingCode = booking.BookingCode,
                            BookingId = booking.Id,
                            TransactionId = booking.Id,
                            TransactionCode = booking.BookingCode,
                            TransactionName = booking.Name,
                        });
                        break;

                    // chưa phát triển
                    case TYPE_CONFIRM_BOOKING.CONFIRM_CANCEL:
                        await ConfirmCancelAsync(booking, input);
                        break;
                }
                return DataResult.ResultSuccess("Confirm booking success!");
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
        private async Task ConfirmAsync(PBooking booking, ConfirmBookingInputDto input)
        {
            if (booking.State != (int)STATE_BOOKING.WAIT_FOR_CONFIRM)
                throw new UserFriendlyException("Can not confirm booking");
            await _bookingProtoClient.UpdateStateBookingAsync(
                new UpdateStateBookingRequest()
                {
                    Id = input.Id,
                    CurrentState = booking.State,
                    UpdateState = (int)STATE_BOOKING.CONFIRMED,
                }, MetadataGrpc.MetaDataHeader(AbpSession));
        }
        private async Task ConfirmCancelAsync(PBooking booking, ConfirmBookingInputDto input)
        {
            if (booking.State == (int)STATE_BOOKING.WAIT_FOR_CONFIRM
                || booking.State == (int)STATE_BOOKING.CONFIRMED)
            {
                await _bookingProtoClient.UpdateStateBookingAsync(
                    new UpdateStateBookingRequest()
                    {
                        Id = input.Id,
                        CurrentState = booking.State,
                        UpdateState = (int)STATE_BOOKING.CANCELLATION_CANCELLED,
                    }, MetadataGrpc.MetaDataHeader(AbpSession));
            }
            else throw new UserFriendlyException("Can not confirm booking");
        }
        #endregion

        #region Partner refuse
        public async Task<DataResult> PartnerRefuseBookingAsync(RefuseBookingInputDto input)
        {
            try
            {
                #region Validation
                PBooking booking = await ValidateBooking(input.Id);
                // check partner 
                if (booking.PartnerId != AbpSession.UserId)
                    throw new UserFriendlyException("You don't have permission");
                #endregion

                switch (input.Type)
                {
                    case TYPE_REFUSE_BOOKING.REFUSE:
                        await RefuseAsync(booking, input);
                        break;
                    case TYPE_REFUSE_BOOKING.REFUSE_CANCEL:
                        await RefuseCancelAsync(booking, input);
                        break;
                }
                return DataResult.ResultSuccess("Refuse booking success!");
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
        private async Task RefuseAsync(PBooking booking, RefuseBookingInputDto input)
        {
            if (booking.State != (int)STATE_BOOKING.WAIT_FOR_CONFIRM)
                throw new UserFriendlyException("Can not refuse booking");
            await _bookingProtoClient.UpdateStateBookingAsync(
                new UpdateStateBookingRequest()
                {
                    Id = input.Id,
                    CurrentState = booking.State,
                    UpdateState = (int)STATE_BOOKING.CANCELLATION_CANCELLED,
                }, MetadataGrpc.MetaDataHeader(AbpSession));
        }
        private async Task RefuseCancelAsync(PBooking booking, RefuseBookingInputDto input)
        {
            if (booking.State == (int)STATE_BOOKING.CANCELLATION_TO_RESPOND)
            {
                await _bookingProtoClient.UpdateStateBookingAsync(
                    new UpdateStateBookingRequest()
                    {
                        Id = input.Id,
                        CurrentState = booking.State,
                        UpdateState = (int)STATE_BOOKING.CONFIRMED,
                    }, MetadataGrpc.MetaDataHeader(AbpSession));
            }
            else throw new UserFriendlyException("Can not refuse booking");
        }
        #endregion
        public async Task<DataResult> PartnerCancelBookingAsync(CancelBookingInputDto input)
        {
            try
            {
                #region Validation
                PBooking booking = await ValidateBooking(input.Id);
                // check partner
                if (booking.PartnerId != AbpSession.UserId)
                    throw new UserFriendlyException("You don't have permission");
                PProvider provider = ValidateProvider(booking.ProviderId);
                #endregion

                // check state booking
                if (booking.State == (int)STATE_BOOKING.WAIT_FOR_CONFIRM
                    || booking.State == (int)STATE_BOOKING.CONFIRMED)
                {
                    CancelBookingResponse result = await _bookingProtoClient.CancelBookingAsync(
                       new CancelBookingRequest()
                       {
                           Id = input.Id,
                           Reason = input.Reason ?? "",
                           UpdateState = (int)STATE_BOOKING.CANCELLATION_CANCELLED,
                       }, MetadataGrpc.MetaDataHeader(AbpSession));
                    // push notification
                    await PushNotificationPartnerCancelBooking(new()
                    {
                        UserId = booking.CreatorUserId,
                        PartnerId = provider.CreatorUserId,
                        TenantIdUser = AbpSession.TenantId ?? 0,
                        TenantIdPartner = provider.TenantId,
                        ProviderId = provider.Id,
                        ProviderName = provider.Name,
                        ImageUrl = booking.BookingItemList[0].ImageUrl,
                        BookingName = booking.Name,
                        BookingCode = booking.BookingCode,
                        BookingId = booking.Id,
                        TransactionId = booking.Id,
                        TransactionCode = booking.BookingCode,
                        TransactionName = booking.Name,
                    });
                    return DataResult.ResultSuccess(result.Data, "Cancellation request has been sent successfully!");
                }
                throw new UserFriendlyException("Cannot cancel booking");
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
        public async Task<DataResult> UserConfirmBookingAsync(long id)
        {
            try
            {
                #region Validation
                PBooking booking = await ValidateBooking(id);
                PProvider provider = ValidateProvider(booking.ProviderId);
                // check user 
                if (booking.CreatorUserId != AbpSession.UserId)
                    throw new UserFriendlyException("You don't have permission");
                // check state booking
                if (booking.State != (int)STATE_BOOKING.CONFIRMED)
                    throw new UserFriendlyException("Can not confirm completed booking");
                #endregion

                UpdateStateBookingResponse result = await _bookingProtoClient.UpdateStateBookingAsync(
                    new UpdateStateBookingRequest()
                    {
                        Id = id,
                        CurrentState = (int)STATE_BOOKING.CONFIRMED,
                        UpdateState = (int)STATE_BOOKING.USER_COMPLETED,
                    }, MetadataGrpc.MetaDataHeader(AbpSession));
                if (result != null)
                {
                    await PushNotificationCompletedBooking(new()
                    {
                        UserId = booking.CreatorUserId,
                        TenantIdPartner = provider.TenantId,
                        TenantIdUser = booking.TenantId,
                        ImageUrl = booking.BookingItemList[0].ImageUrl,
                        ProviderName = provider.Name,
                        ProviderId = provider.Id,
                        BookingId = booking.Id,
                        BookingCode = booking.BookingCode,
                        TransactionId = booking.Id,
                        TransactionCode = booking.BookingCode,
                    });
                }
                return DataResult.ResultSuccess(result.Data, "Confirm completed booking success!");
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
        public async Task<DataResult> UserCancelBookingAsync(CancelBookingInputDto input)
        {
            try
            {
                #region Validation
                PBooking booking = await ValidateBooking(input.Id);
                // check user
                if (booking.CreatorUserId != AbpSession.UserId)
                    throw new UserFriendlyException("You don't have permission");
                GetProviderByIdResponse resProvider = await _providerProtoClient.GetProviderByIdAsync(new GetProviderByIdRequest()
                {
                    Id = booking.ProviderId,
                    IsDataStatic = false,
                });
                #endregion

                // check state booking
                if (booking.State == (int)STATE_BOOKING.WAIT_FOR_CONFIRM)
                {
                    CancelBookingResponse result = await _bookingProtoClient.CancelBookingAsync(
                       new CancelBookingRequest()
                       {
                           Id = input.Id,
                           Reason = input.Reason ?? "",
                           UpdateState = (int)STATE_BOOKING.CANCELLATION_CANCELLED,
                       }, MetadataGrpc.MetaDataHeader(AbpSession));
                    // push notification
                    await PushNotificationUserCancelBooking(new()
                    {
                        UserId = booking.CreatorUserId,
                        PartnerId = resProvider.Data.CreatorUserId,
                        TenantIdUser = AbpSession.TenantId ?? 0,
                        TenantIdPartner = resProvider.Data.TenantId,
                        ProviderId = resProvider.Data.Id,
                        ProviderName = resProvider.Data.Name,
                        ImageUrl = booking.BookingItemList[0].ImageUrl,
                        BookingName = booking.Name,
                        BookingCode = booking.BookingCode,
                        BookingId = booking.Id,
                        TransactionId = booking.Id,
                        TransactionCode = booking.BookingCode,
                        TransactionName = booking.Name,
                    });
                    return DataResult.ResultSuccess(result.Data, "Cancellation request has been sent successfully!");
                }
                throw new UserFriendlyException("Cannot cancel booking");
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

        #region Validate
        private async Task<PBooking> ValidateBooking(long bookingId)
        {
            GetBookingByIdResponse response = await _bookingProtoClient.GetBookingByIdAsync(
                new GetBookingByIdRequest() { Id = (long)bookingId }, MetadataGrpc.MetaDataHeader(AbpSession));
            if (response.Data == null) throw new UserFriendlyException("Booking not found");
            return response.Data;
        }
        private static void ValidateFormIdUser(int formId)
        {
            if (!Enum.IsDefined(typeof(FORM_ID_BOOKING_USER), formId))
            {
                throw new UserFriendlyException("FormId is invalid");
            }
        }
        private static void ValidateFormIdPartner(int formId)
        {
            if (!Enum.IsDefined(typeof(FORM_ID_BOOKING_PARTNER), formId))
            {
                throw new UserFriendlyException("FormId is invalid");
            }
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
        private async Task PushNotificationCreateBooking(NotificationMessage message) // Tạo đơn booking dịch vụ
        {
            // user
            await _appNotifyBusiness.SendNotifyFullyToUser(new()
            {
                TenantId = message.TenantIdUser,
                Title = "Xác nhận đặt dịch vụ",
                Message = "Bạn đã đặt dịch vụ " + message.TransactionName + " thành công và đang chờ nhà cung cấp xác nhận.",
                Action = AppNotificationAction.CreateBookingSuccess,
                Icon = AppNotificationIcon.CreateBookingSuccessIcon,
                AppType = (int)AppType.APP_USER,
                Type = (int)TYPE_NOTIFICATION.SOCIAL,
                UserId = (long)message.UserId,
                BookingId = message.BookingId,
                TransactionId = message.TransactionId,
                ImageUrl = message.ImageUrl,
                PageId = (int?)TAB_ID.NOTIFICATION_USER_BOOKING,
            });

            // partner
            await _appNotifyBusiness.SendNotifyFullyToUser(new()
            {
                TenantId = message.TenantIdPartner,
                Title = "Bạn có đơn đặt dịch vụ mới",
                Message = $"Bạn có một đơn dịch vụ vào {FormatDateTime(DateTime.Now)}",
                Action = AppNotificationAction.CreateBookingSuccess,
                Icon = AppNotificationIcon.CreateBookingSuccessIcon,
                AppType = (int)AppType.APP_PARTNER,
                Type = (int)TYPE_NOTIFICATION.SOCIAL,
                UserId = (long)message.PartnerId,
                ProviderId = message.ProviderId,
                BookingId = (long)message.BookingId,
                TransactionId = message.TransactionId,
                PageId = (int?)TAB_ID.NOTIFICATION_PARTNER,
                ImageUrl = message.ImageUrl,
            });
        }

        private async Task PushNotificationPartnerConfirmBooking(NotificationMessage message) // Xác nhận đặt dịch vụ
        {
            // user
            await _appNotifyBusiness.SendNotifyFullyToUser(new()
            {
                TenantId = message.TenantIdUser,
                Title = "Xác nhận đơn đặt dịch vụ",
                Message = "Đơn đặt dịch vụ " + message.TransactionCode + " của bạn đã được xác nhận.",
                Action = AppNotificationAction.UserCompleted,
                Icon = AppNotificationIcon.UserCompletedIcon,
                AppType = (int)AppType.APP_USER,
                Type = (int)TYPE_NOTIFICATION.SOCIAL,
                UserId = (long)message.UserId,
                ImageUrl = message.ImageUrl,
                PageId = (int?)TAB_ID.NOTIFICATION_USER_BOOKING,
                BookingId = message.BookingId,
                TransactionId = message.TransactionId,
            });
        }

        private async Task PushNotificationUserCancelBooking(NotificationMessage message) // Huỷ dịch vụ
        {
            // user
            await _appNotifyBusiness.SendNotifyFullyToUser(new()
            {
                TenantId = message.TenantIdUser,
                Title = "Hủy dịch vụ",
                Message = "Hủy dịch vụ thành công",
                Action = AppNotificationAction.UserCancel,
                Icon = AppNotificationIcon.UserCancelIcon,
                AppType = (int)AppType.APP_USER,
                Type = (int)TYPE_NOTIFICATION.SOCIAL,
                UserId = (long)message.UserId,
                BookingId = message.BookingId,
                TransactionId = message.TransactionId,
                ImageUrl = message.ImageUrl,
                PageId = (int?)TAB_ID.NOTIFICATION_USER_BOOKING,
                ProviderId = message.ProviderId,
            });

            // partner
            await _appNotifyBusiness.SendNotifyFullyToUser(new()
            {
                TenantId = message.TenantIdPartner,
                Title = "Hủy dịch vụ",
                Message = "Đơn đặt dịch vụ " + message.TransactionCode + " đã được huỷ bởi người đặt.",
                Action = AppNotificationAction.UserCancel,
                Icon = AppNotificationIcon.UserCancelIcon,
                AppType = (int)AppType.APP_PARTNER,
                Type = (int)TYPE_NOTIFICATION.SOCIAL,
                UserId = (long)message.PartnerId,
                ProviderId = message.ProviderId,
                BookingId = message.BookingId,
                TransactionId = message.TransactionId,
                ImageUrl = message.ImageUrl,
                PageId = (int)TAB_ID.NOTIFICATION_PARTNER,
            });
        }

        private async Task PushNotificationPartnerCancelBooking(NotificationMessage message) // Nhà cung cấp huỷ dịch vụ
        {
            // user
            await _appNotifyBusiness.SendNotifyFullyToUser(new()
            {
                TenantId = message.TenantIdUser,
                Title = "Hủy dịch vụ",
                Message = "Nhà cung cấp đã huỷ đơn đặt dịch vụ " + message.TransactionCode + " của bạn.",
                Action = AppNotificationAction.PartnerCancel,
                Icon = AppNotificationIcon.PartnerCancelIcon,
                AppType = (int)AppType.APP_USER,
                Type = (int)TYPE_NOTIFICATION.SOCIAL,
                UserId = (long)message.UserId,
                PageId = (int?)TAB_ID.NOTIFICATION_USER_BOOKING,
                BookingId = message.BookingId,
                TransactionId = message.TransactionId,
                ImageUrl = message.ImageUrl,
                ProviderId = message.ProviderId,
            });
        }

        private async Task PushNotificationCompletedBooking(NotificationMessage message) // Hoàn thành đơn booking
        {
            // partner
            await _appNotifyBusiness.SendNotifyFullyToUser(new()
            {
                TenantId = message.TenantIdPartner,
                Title = "Dịch vụ hoàn thành",
                Message = "Đơn đặt dịch vụ " + message.TransactionCode + " đã được xác nhận hoàn thành.",
                Action = AppNotificationAction.UserCompleted,
                Icon = AppNotificationIcon.UserCompletedIcon,
                AppType = (int)AppType.APP_PARTNER,
                Type = (int)TYPE_NOTIFICATION.SOCIAL,
                UserId = (long)message.PartnerId,
                ProviderId = message.ProviderId,
                BookingId = message.BookingId,
                TransactionId = message.TransactionId,
                ImageUrl = message.ImageUrl,
                PageId = (int)TAB_ID.NOTIFICATION_PARTNER,
            });
        }
        #endregion
    }
}
