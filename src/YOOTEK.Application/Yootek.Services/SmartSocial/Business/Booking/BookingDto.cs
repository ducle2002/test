using Yootek.Common;
using Yootek.Yootek.Services.Yootek.SmartSocial.BusinessNEW.BusinessDto;
using Yootek.Services.SmartSocial.Orders.Dto;
using System;
using System.Collections.Generic;

namespace Yootek.Services.SmartSocial.Bookings.Dto
{
    public class GetBookingsInputDto : CommonInputDto
    {
        public string? Search { get; set; }
        public long? BookerId { get; set; }
        public long? ProviderId { get; set; }
        public int? FormId { get; set; }
        public int? OrderBy { get; set; }
        public int? Type { get; set; }
        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class GetBookingsByPartnerInputDto : CommonInputDto
    {
        public int? TenantId { get; set; }
        public long? BookerId { get; set; }
        public string? Search { get; set; }
        public long? ProviderId { get; set; }
        public FORM_ID_BOOKING_PARTNER? FormId { get; set; } = FORM_ID_BOOKING_PARTNER.FORM_PARTNER_BOOKING_GETALL;
        public ORDER_BY_BOOKING? OrderBy { get; set; }
        public int? Type { get; set; }
        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class GetBookingsByUserInputDto : CommonInputDto
    {
        public int? TenantId { get; set; }
        public long? BookerId { get; set; }
        public string? Search { get; set; }
        public long? ProviderId { get; set; }
        public FORM_ID_BOOKING_USER? FormId { get; set; } = FORM_ID_BOOKING_USER.FORM_USER_BOOKING_GETALL;
        public ORDER_BY_BOOKING? OrderBy { get; set; }
        public int? Type { get; set; }
        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class CreateBookingInputDto
    {
        public int? TenantId { get; set; }
        public long? ProviderId { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string? Description { get; set; }
        public string? Email { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int Type { get; set; }
        public double? TotalPrice { get; set; }
        public List<BookingItemModel> BookingItemList { get; set; }
        public RecipientAddressEntity RecipientAddress { get; set; }
        public PAYMENT_METHOD PaymentMethod { get; set; }
    }
    public class BookingItemModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? TenantId { get; set; }
        public long? ItemId { get; set; }
        public bool? IsDefault { get; set; }
        public double? OriginalPrice { get; set; }
        public double? CurrentPrice { get; set; }
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }
        public string? ItemName { get; set; }
    }
    public class UpdateBookingByAdminInputDto
    {
        public long Id { get; set; }
        public int TenantId { get; set; }
        public long ProviderId { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string? Description { get; set; }
        public string? Email { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public string BookingCode { get; set; }
        public double TotalPrice { get; set; }
        public int Type { get; set; }
        public long BookerId { get; set; }
        public List<BookingItemModel> BookingItemList { get; set; }
        public RecipientAddressEntity RecipientAddress { get; set; }
        public PAYMENT_METHOD PaymentMethod { get; set; }
    }
    public class UpdateBookingInputDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string? Description { get; set; }
        public string? Email { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public RecipientAddressEntity RecipientAddress { get; set; }
        public PAYMENT_METHOD PaymentMethod { get; set; }
    }

    public class UpdateStateBookingInputDto
    {
        public long Id { get; set; }
        public int CurrentState { get; set; }
        public int UpdateState { get; set; }
    }

    public class ConfirmBookingInputDto
    {
        public long Id { get; set; }
        public TYPE_CONFIRM_BOOKING Type { get; set; }
    }
    public class RefuseBookingInputDto
    {
        public long Id { get; set; }
        public TYPE_REFUSE_BOOKING Type { get; set; }
    }
    public class CancelBookingInputDto
    {
        public long Id { get; set; }
        public string Reason { get; set; }
    }
    public enum TYPE_CONFIRM_BOOKING
    {
        CONFIRM = 1,
        CONFIRM_CANCEL = 2,
    }
    public enum TYPE_REFUSE_BOOKING
    {
        REFUSE = 1,
        REFUSE_CANCEL = 2,
    }
    public enum STATE_BOOKING
    {
        WAIT_FOR_CONFIRM = 1,

        CONFIRMED = 2,  // partner confirm

        USER_COMPLETED = 3,

        CANCELLATION = 4,
        CANCELLATION_TO_RESPOND = 41,
        CANCELLATION_CANCELLED = 42,

        RETURN_REFUND = 5,
        RETURN_REFUND_NEW_REQUEST = 51,
        RETURN_REFUND_RESPONDED = 52,
        RETURN_REFUND_COMPLETED = 53,
    }
    public enum FORM_ID_BOOKING_PARTNER
    {
        FORM_PARTNER_BOOKING_GETALL = 20,
        FORM_PARTNER_BOOKING_TO_PAY = 21,
        FORM_PARTNER_BOOKING_CONFIRMED = 22,
        FORM_PARTNER_BOOKING_COMPLETE = 23,
        FORM_PARTNER_BOOKING_CANCELLATION = 24,
        FORM_PARTNER_BOOKING_CANCELLATION_TO_RESPOND = 241,
        FORM_PARTNER_BOOKING_CANCELLATION_CANCELLED = 242,
        FORM_PARTNER_BOOKING_REFUND = 25,
        FORM_PARTNER_BOOKING_RETURN_REFUND_NEW_REQUEST = 251,
        FORM_PARTNER_BOOKING_RETURN_REFUND_RESPONDED = 252,
        FORM_PARTNER_BOOKING_RETURN_REFUND_COMPLETED = 253,
    }

    public enum ORDER_BY_BOOKING
    {
        NEWEST = 1,  // mới nhất
        LATEST = 2,   // cũ nhất
        PRICE_ASC = 3,   // giá tăng dần 
        PRICE_DESC = 4,   // giá giảm dần  
    }

    public enum FORM_ID_BOOKING_USER
    {
        FORM_USER_BOOKING_GETALL = 30,
        FORM_USER_BOOKING_TO_PAY = 31,
        FORM_USER_BOOKING_CONFIRMED = 32,
        FORM_USER_BOOKING_COMPLETE = 33,
        FORM_USER_BOOKING_CANCELED = 34,
        FORM_USER_BOOKING_USER_REFUND = 35,
    }
}
