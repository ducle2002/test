using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using Yootek.Common;
using JetBrains.Annotations;

namespace Yootek.App.ServiceHttpClient.Dto.Business
{
    public class OrderDetailDto : EntityDto<long>
    {
        public int? TenantId { get; set; }
        public long ProviderId { get; set; }

        [StringLength(256)] public string OrderCode { get; set; }
        public int PaymentStatus { get; set; }
        public long PartnerId { get; set; }
        public string? Description { get; set; }
        public long OrdererId { get; set; }
        public List<ItemModelDto> OrderItemList { get; set; }
        public double TotalPrice { get; set; }
        public RecipientAddressDto RecipientAddress { get; set; }
        public int State { get; set; }
        public int PaymentMethod { get; set; }
        public string Properties { get; set; }
        public DetailCancel? DetailCancel { get; set; }
        public string? Transaction { get; set; }
        [CanBeNull] public List<TrackingInfoDto> TrackingInfo { get; set; }
        [CanBeNull] public string ProviderName { get; set; }
    }

    public class DetailCancel
    {
        public string Reason { get; set; }
        public DateTime? CreationTime { get; set; }
        public List<string>? Urls { get; set; }
    }

    public class GetListEcofarmOrdersByPartnerDto : CommonInputDto
    {
        public long ProviderId { get; set; }
        public int? FormId { get; set; }
        public int? OrderBy { get; set; }
    }

    public class GetListEcofarmOrdersByUserDto : CommonInputDto
    {
        public int? FormId { get; set; }
        public int? OrderBy { get; set; }
    }

    public class GetOrderEcofarmDto : EntityDto<long>
    {
    }

    public class GetCountOrderEcoFarmsDto
    {
        public long ProviderId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class GetItemRankingDto : SortedInputDto
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int FormId { get; set; }
        public int? TenantId { get; set; }
        public long ProviderId { get; set; }
    }

    public class GetRevenueEcoFarmDto
    {
        public long ProviderId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Type { get; set; }
    }

    public class GetStatisticDto
    {
        public long ProviderId { get; set; }
        public int FormId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int Type { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
    }

    public class GetStatisticSomeProviderDto
    {
        public List<long> ListProviderId { get; set; }
        public int FormId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int Type { get; set; }
        public int Year { get; set; }
        public int? Month { get; set; }
        public int? Day { get; set; }
    }

    public class ItemRankingEcoFarmDto
    {
        public long ItemId { get; set; }
        public string ItemDto { get; set; }
        public int Count { get; set; }
        public long Sales { get; set; }
    }

    public class StatisticOrderEcoFarmDto
    {
        public List<long> ListCount { get; set; }
        public List<long> ListRevenue { get; set; }
    }

    public class StatisticSomeProviderOrderEcoFarmDto
    {
        public Dictionary<long, long> ListCount { get; set; }
        public Dictionary<long, long> ListRevenue { get; set; }
    }

    public class CountOrderEcoFarmsDto
    {
        public int ToPay { get; set; }
        public int ToShip { get; set; }
        public int Shipping { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
        public int ReturnRefund { get; set; }
    }

    public class CreateOrderEcofarmDto
    {
        public long ProviderId { get; set; }
        public double TotalPrice { get; set; }
        [CanBeNull] public string? Description { get; set; }
        public int? State { get; set; }
        public List<OrderItemModelDto> OrderItemList { get; set; }
        public RecipientAddressDto RecipientAddress { get; set; }
        public int PaymentMethod { get; set; }
        public long PartnerId { get; set; }
        [CanBeNull] public List<long> ListVouchers { get; set; }
    }

    public class UpdateOrderEcofarmDto : EntityDto<long>
    {
        public string? Description { get; set; }
        public RecipientAddressDto RecipientAddress { get; set; }
    }

    public class RatingOrderEcofarmDto
    {
        public long Id { get; set; }
        public List<RatingOrderDto> Items { get; set; }
    }

    public class RatingOrderDto
    {
        public int? TenantId { get; set; }
        public long? ItemId { get; set; }
        public long? ProviderId { get; set; }
        public int? RatePoint { get; set; }
        public int? Type { get; set; }
        public string? Comment { get; set; }
        public string? FileUrl { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Avatar { get; set; }
        public long? AnswerRateId { get; set; }
        public long? OrderId { get; set; }
        public long? BookingId { get; set; }
    }

    public class UpdateStateOrderEcofarmDto : EntityDto<long>
    {
        public int State { get; set; }
        [CanBeNull] public DetailCancel DetailCancel { get; set; }
    }

    public class DeleteOrderEcofarmDto : EntityDto<long>
    {
    }

    public class ConfirmOrderByPartnerDto : EntityDto<long>
    {
        public EOrderTypeConfirm? TypeAction { get; set; }
    }

    public class RefuseOrderByPartnerDto : EntityDto<long>
    {
        public EOrderTypeRefuse? TypeAction { get; set; }
    }

    public class CancelOrderDto : EntityDto<long>
    {
        public string Reason { get; set; }
        [CanBeNull] public List<string> Urls { get; set; }
    }

    public enum EOrderTypeConfirm
    {
        CONFIRM = 1,
        CONFIRM_SHIPPING = 2,
        CONFIRM_SHIPPER_COMPLETED = 3,
        CONFIRM_CANCEL = 4,
        CONFIRM_RETURN_REFUND = 5
    }

    public enum EOrderTypeRefuse
    {
        REFUSE = 1,
        REFUSE_CANCEL = 2,
        REFUSE_RETURN_REFUND = 3
    }


    public class OrderItemModelDto
    {
        public long Id { get; set; }
        public int Quantity { get; set; }
    }

    public class RecipientAddressDto
    {
        public string Name { get; set; }
        public string? Phone { get; set; }
        public string? ProvinceId { get; set; }
        public string? DistrictId { get; set; }
        public string? WardId { get; set; }
        public string? ProvinceName { get; set; }
        public string? DistrictName { get; set; }
        public string? WardName { get; set; }
        public string FullAddress { get; set; }
    }

    public class TrackingInfoDto
    {
        public string TrackingItemTime { get; set; }
        public int TrackingItemState { get; set; }
        public string TrackingItemDetail { get; set; }
    }
}