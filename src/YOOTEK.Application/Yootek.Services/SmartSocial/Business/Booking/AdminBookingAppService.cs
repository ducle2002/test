/*using Abp;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.UI;
using Google.Protobuf.WellKnownTypes;
using Yootek.App.Grpc;
using Yootek.Application.Protos.Business.Bookings;
using Yootek.Authorization;
using Yootek.Common.DataResult;
using Yootek.Yootek.Services.Yootek.SmartSocial.Business.Bookings.Dto;
using System;
using System.Linq;
using System.Threading.Tasks;
namespace Yootek.Application.Yootek.Services.Yootek.SmartSocial.Business.Bookings.Admin
{
    public interface IAdminBookingAppService : IApplicationService
    {
        Task<object> GetAllBookingsAsync(GetBookingsInputDto input);
        Task<object> GetBookingByIdAsync(long id);
        Task<object> CreateBookingAsync(CreateBookingInputDto input);
        Task<object> UpdateBookingAsync(AdminUpdateBookingInputDto input);
        Task<object> UpdateStateBookingAsync(UpdateStateBookingInputDto input);
        Task<object> DeleteBookingAsync(long id);
    }
    [AbpAuthorize(PermissionNames.Social_Business_Bookings)]
    public class AdminBookingAppService : YootekAppServiceBase, IAdminBookingAppService
    {
        private readonly BookingProtoGrpc.BookingProtoGrpcClient _bookingProtoClient;
        public AdminBookingAppService(
            BookingProtoGrpc.BookingProtoGrpcClient bookingProtoClient
            )
        {
            _bookingProtoClient = bookingProtoClient;
        }
        public async Task<object> GetAllBookingsAsync(GetBookingsInputDto input)
        {
            try
            {
                GetAllBookingsRequest request = new()
                {
                    BookerId = input.BookerId ?? 0,
                    ProviderId = input.ProviderId ?? 0,
                    FormId = input.FormId ?? 0,
                    OrderBy = input.OrderBy ?? 0,
                    Type = input.Type ?? 0,
                    Search = input.Search ?? "",
                    MinPrice = input.MinPrice ?? 0,
                    MaxPrice = input.MaxPrice ?? 0,
                    DateFrom = ConvertDatetimeToTimestamp(input.DateFrom, true),
                    DateTo = ConvertDatetimeToTimestamp(input.DateTo, false),
                    SkipCount = input.SkipCount,
                    MaxResultCount = input.MaxResultCount,
                };
                GetAllBookingsResponse result = await _bookingProtoClient.GetAllBookingsAsync(request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Items, "Get bookings success", result.TotalCount);
            }
            catch (UserFriendlyException)
            {
                throw new UserFriendlyException("Cannot get bookings");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> GetBookingByIdAsync(long id)
        {
            try
            {
                GetBookingByIdRequest request = new() { Id = id, };
                GetBookingByIdResponse result = await _bookingProtoClient.GetBookingByIdAsync(request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Data, "Get booking detail success");
            }
            catch (UserFriendlyException)
            {
                throw new UserFriendlyException("Cannot get booking detail");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        private Timestamp ConvertDatetimeToTimestamp(DateTime? dt, bool IsDateFrom)
        {
            DateTime date;
            if (dt != null && IsDateFrom)
            {
                DateTime dateFromUnspecified = new DateTime(dt.Value.Year, dt.Value.Month, dt.Value.Day, dt.Value.Hour, dt.Value.Minute, dt.Value.Second);
                date = DateTime.SpecifyKind(dateFromUnspecified, DateTimeKind.Utc);
                return Timestamp.FromDateTime(date);
            }
            else if (dt == null && IsDateFrom)
            {
                DateTime dateFromUnspecified = new DateTime(1970, 1, 1);
                date = DateTime.SpecifyKind(dateFromUnspecified, DateTimeKind.Utc);
            }
            else if (dt != null && !IsDateFrom)
            {
                DateTime dateFromUnspecified = new DateTime(dt.Value.Year, dt.Value.Month, dt.Value.Day);
                date = DateTime.SpecifyKind(dateFromUnspecified, DateTimeKind.Utc);
            }
            else
            {
                DateTime dateFromUnspecified = DateTime.Now;
                date = DateTime.SpecifyKind(dateFromUnspecified, DateTimeKind.Utc);
            }
            return Timestamp.FromDateTime(date);
        }
        public async Task<object> CreateBookingAsync(CreateBookingInputDto input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(null))
                {
                    CreateBookingResponse result = await _bookingProtoClient.CreateBookingAsync(new CreateBookingRequest()
                    {
                        TenantId = input.TenantId ?? 0,
                        BookerId = (long)AbpSession.UserId,
                        ProviderId = input.ProviderId ?? 0,
                        Name = input.Name,
                        PhoneNumber = input.PhoneNumber,
                        Email = input.Email,
                        Description = input.Description,
                        TotalPrice = input.TotalPrice ?? 0,
                        Type = input.Type,
                        CheckIn = Timestamp.FromDateTime(DateTime.SpecifyKind(input.CheckIn, DateTimeKind.Utc)),
                        CheckOut = Timestamp.FromDateTime(DateTime.SpecifyKind(input.CheckOut, DateTimeKind.Utc)),
                        RecipientAddress = new PRecipientAddress()
                        {
                            City = input.RecipientAddress.City ?? "",
                            FullAddress = input.RecipientAddress.FullAddress ?? "",
                            District = input.RecipientAddress.FullAddress ?? "",
                            Name = input.RecipientAddress.Name ?? "",
                            Town = input.RecipientAddress.Town ?? "",
                            Phone = input.RecipientAddress.Phone ?? "",
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
                        PaymentMethod = input.PaymentMethod ?? "COD",
                    }, MetadataGrpc.MetaDataHeader(AbpSession));
                    return DataResult.ResultSuccess(result.Data, "Create Booking success");
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

        public async Task<object> UpdateBookingAsync(AdminUpdateBookingInputDto input)
        {
            try
            {
                UpdateBookingResponse result = await _bookingProtoClient.UpdateBookingAsync(new UpdateBookingRequest()
                {
                    Id = input.Id,
                    Name = input.Name,
                    PhoneNumber = input.PhoneNumber,
                    Email = input.Email,
                    Description = input.Description,
                    CheckIn = Timestamp.FromDateTime(DateTime.SpecifyKind(input.CheckIn, DateTimeKind.Utc)),
                    CheckOut = Timestamp.FromDateTime(DateTime.SpecifyKind(input.CheckOut, DateTimeKind.Utc)),
                    RecipientAddress = new PRecipientAddress()
                    {
                        City = input.RecipientAddress.City ?? "",
                        FullAddress = input.RecipientAddress.FullAddress ?? "",
                        District = input.RecipientAddress.FullAddress ?? "",
                        Name = input.RecipientAddress.Name ?? "",
                        Town = input.RecipientAddress.Town ?? "",
                        Phone = input.RecipientAddress.Phone ?? "",
                    },
                    *//*BookingItemList =
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
                            ItemName = x.ItemName ?? "",
                        })
                    },*//*
                    PaymentMethod = input.PaymentMethod ?? "COD",
                }, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Data, "Update Booking success");
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

        public async Task<object> UpdateStateBookingAsync(UpdateStateBookingInputDto input)
        {
            try
            {
                UpdateStateBookingResponse result = await _bookingProtoClient.UpdateStateBookingAsync(new UpdateStateBookingRequest()
                {
                    Id = input.Id,
                    CurrentState = input.CurrentState,
                    UpdateState = input.UpdateState,
                }, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Data, "Update state booking success !");
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

        public async Task<object> DeleteBookingAsync(long id)
        {
            try
            {
                DeleteBookingResponse result = await _bookingProtoClient.DeleteBookingAsync(new DeleteBookingRequest()
                {
                    Id = id
                }, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Data, "Delete Booking success");
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