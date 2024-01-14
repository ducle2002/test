using Yootek.Common;
using Yootek.Yootek.Services.Yootek.SmartSocial.BusinessNEW.BusinessDto;
using Yootek.Services.SmartSocial.Rates.Dto;
using System;
using System.Collections.Generic;

namespace Yootek.Services.SmartSocial.Orders.Dto
{
    public enum TAB_ID
    {
        NOTIFICATION_PARTNER = 1,
        NOTIFICATION_USER_SHOPPING = 2,
        NOTIFICATION_USER_BOOKING = 3,
        NOTIFICATION_USER_FORUM = 4,

        // eco-farm
        NOTIFICATION_USER_REGISTER_PACKAGE = 5,
        NOTIFICATION_USER_ACTIVITY_PACKAGE = 6,

    }
    public class GetOrdersInputDto : CommonInputDto
    {
        public int? TenantId { get; set; }
        public string? Search { get; set; }
        public long? OrdererId { get; set; }
        public ORDER_BY_ORDER? OrderBy { get; set; }
        public long? ProviderId { get; set; }
        public int? FormId { get; set; }
        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class GetOrdersByPartnerInputDto : CommonInputDto
    {
        public int? TenantId { get; set; }
        public string? Search { get; set; }
        public long? OrdererId { get; set; }
        public long? ProviderId { get; set; }
        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
        public FORM_ID_PARTNER_GET_ORDER FormId { get; set; }
        public ORDER_BY_ORDER? OrderBy { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class GetStatisticOrderInputDto
    {
        public long ProviderId { get; set; }
        public FORM_ID_STATISTIC FormId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int Type { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
    }

    public class GetStatisticSomeProvidersInputDto
    {
        public long[] ListProviderId { get; set; }
        public FORM_ID_STATISTIC FormId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int Type { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
    }
    public enum FORM_ID_STATISTIC
    {
        NEW = 1,
        COMPLETED = 2,
    }
    public class GetOrdersByUserInputDto : CommonInputDto
    {
        public int? TenantId { get; set; }
        public string? Search { get; set; }
        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
        public FORM_ID_USER_GET_ORDER FormId { get; set; }
        public ORDER_BY_ORDER? OrderBy { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
    public class GetCountOrdersInputDto
    {
        public long? ProviderId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
    public class GetRevenueInputDto
    {
        public long ProviderId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Type { get; set; }
    }
    public class GetItemRankingInputDto
    {
        public int TenantId { get; set; }
        public long ProviderId { get; set; }
        public FORM_ID_ITEM_RANKING FormId { get; set; }
        public SORT_BY SortBy { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
    public enum PAYMENT_METHOD
    {
        COD = 1,
        MOMO = 2,
        VNPAY = 3,
        VIETELPAY = 4,
    }
    public enum FORM_ID_ITEM_RANKING
    {
        SALES = 1,
        UNITS_SOLD = 2,
    }
    public enum SORT_BY
    {
        ASC = 1,
        DESC = 2,
    }
    public enum ORDER_BY_ORDER
    {
        NEWEST = 1,  // mới nhất
        LATEST = 2,   // cũ nhất
        PRICE_ASC = 3,   // giá tăng dần 
        PRICE_DESC = 4,   // giá giảm dần  
    }

    public enum FORM_ID_PARTNER_GET_ORDER
    {
        FORM_PARTNER_ORDER_GETALL = 20,
        FORM_PARTNER_ORDER_TO_PAY = 21,
        FORM_PARTNER_ORDER_TO_SHIP = 22,
        FORM_PARTNER_ORDER_TO_SHIP_TO_PROCESS = 221,
        FORM_PARTNER_ORDER_TO_SHIP_PROCESSED = 222,
        FORM_PARTNER_ORDER_SHIPPING = 23,
        FORM_PARTNER_ORDER_COMPLETED = 24,
        FORM_PARTNER_ORDER_CANCELLATION = 25,
        FORM_PARTNER_ORDER_CANCELLATION_TO_RESPOND = 251,
        FORM_PARTNER_ORDER_CANCELLATION_CANCELLED = 252,
        FORM_PARTNER_ORDER_RETURN_REFUND = 26,
        FORM_PARTNER_ORDER_RETURN_REFUND_NEW_REQUEST = 261,
        FORM_PARTNER_ORDER_RETURN_REFUND_TO_RESPOND = 262,
        FORM_PARTNER_ORDER_RETURN_REFUND_RESPONDED = 263,
        FORM_PARTNER_ORDER_RETURN_REFUND_COMPLETED = 264,
    }
    public enum FORM_ID_USER_GET_ORDER
    {
        FORM_USER_ORDER_GETALL = 30,
        FORM_USER_ORDER_UNPAID = 31,
        FORM_USER_ORDER_TO_SHIP = 32,
        FORM_USER_ORDER_SHIPPING = 33,
        FORM_USER_ORDER_COMPLETED = 34,
        FORM_USER_ORDER_CANCELLATION = 35,
        FORM_USER_ORDER_RETURN_REFUND = 36,
    }

    public class CreateOrderInputDto
    {
        public int? TenantId { get; set; }
        public long? ProviderId { get; set; }
        public double? TotalPrice { get; set; }
        public List<OrderItemModel> OrderItemList { get; set; }
        public RecipientAddressEntity RecipientAddress { get; set; }
        public PAYMENT_METHOD PaymentMethod { get; set; }
    }
    public class CreateOrderByUserInputDto
    {
        public int? TenantId { get; set; }
        public long ProviderId { get; set; }
        public double TotalPrice { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public List<OrderItemModel> OrderItemList { get; set; }
        public RecipientAddressEntity RecipientAddress { get; set; }
        public PAYMENT_METHOD PaymentMethod { get; set; }
        public List<long>? ListVouchers { get; set; } = new List<long>();
    }

    public class CancelOrderInputDto
    {
        public long Id { get; set; }
        public string Reason { get; set; }
    }
    public class AdminUpdateOrderInputDto
    {
        public long Id { get; set; }
        public RecipientAddressEntity RecipientAddress { get; set; }
        public PAYMENT_METHOD PaymentMethod { get; set; }
    }
    public class UpdateOrderInputDto
    {
        public long Id { get; set; }
        public RecipientAddressEntity RecipientAddress { get; set; }
    }
    public class RatingOrderInputDto
    {
        public long Id { get; set; }
        public List<CreateRateInputDto> Items { get; set; }
    }
    public class UpdateStateOrderInputDto
    {
        public long Id { get; set; }
        public int CurrentState { get; set; }
        public int UpdateState { get; set; }
    }

    public class ConfirmOrderInputDto
    {
        public long Id { get; set; }
        public TYPE_CONFIRM_ORDER? Type { get; set; }
    }

    public class RefuseOrderInputDto
    {
        public long Id { get; set; }
        public TYPE_REFUSE_ORDER? Type { get; set; }
    }
    public enum TYPE_CONFIRM_ORDER
    {
        CONFIRM = 1,
        CONFIRM_SHIPPING = 2,
        CONFIRM_SHIPPER_COMPLETED = 3,
        CONFIRM_CANCEL = 4,
        CONFIRM_RETURN_REFUND = 5,
    }
    public enum TYPE_REFUSE_ORDER
    {
        REFUSE = 1,
        REFUSE_CANCEL = 2,
        REFUSE_RETURN_REFUND = 3,
    }
    public enum STATE_ORDER
    {
        TO_PAY = 1,

        TO_SHIP_TO_PROCESS = 21,

        SHIPPING = 3,

        SHIPPER_COMPLETED = 41,
        USER_COMPLETED = 42,
        USER_RATING = 43,

        CANCELLATION_TO_RESPOND = 51,
        CANCELLATION_CANCELLED = 52,

        RETURN_REFUND_NEW_REQUEST = 61,
        RETURN_REFUND_TO_RESPOND = 62,
        RETURN_REFUND_RESPONDED = 63,
        RETURN_REFUND_COMPLETED = 64,
    }
}
