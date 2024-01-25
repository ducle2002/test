using Abp.AutoMapper;
using Yootek.EntityDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Yootek.Yootek.Services.Yootek;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Yootek.Services.Dto
{
    [AutoMap(typeof(ObjectPartner))]
    public class ObjectDto : ObjectPartner
    {
        public IEnumerable<Rate> Rates { get; set; }
        public IEnumerable<Items> Items { get; set; }
        public double? Rate { get; set; }
        public int? NumberRate { get; set; }

        public double? Distance { get; set; }

        // public float[] LocationMap { get; set; }
        public int? CountNewOrder { get; set; }
    }

    [AutoMap(typeof(ObjectType))]
    public class ObjectTypeDto : ObjectType
    {
    }

    [AutoMap(typeof(Items))]
    public class ItemsDto : Items
    {
        public string ShopName { get; set; }
        public IEnumerable<Rate> Rates { get; set; }
        public int? CountRate { get; set; }
        public float? Rate { get; set; }
    }

    [AutoMap(typeof(ItemViewSetting))]
    public class ItemViewSettingDto : ItemViewSetting
    {
    }

    [AutoMap(typeof(Rate))]
    public class RateDto : Rate
    {
        public ItemsDto Item { get; set; }
        public Rate Answerd { get; set; }
        public bool? IsItemReview { get; set; }
        public bool? HasAnswered { get; set; }
    }


    [AutoMap(typeof(ItemType))]
    public class ItemTypeDto : ItemType
    {
    }

    [AutoMap(typeof(SetItems))]
    public class SetItemsDto : SetItems
    {
    }

    [AutoMap(typeof(Order))]
    public class OrderDto : Order
    {
        public SetItems SetItems { get; set; }
    }

    [AutoMap(typeof(Order))]
    public class OrderUserDto : Order
    {
        public string StoreProperties { get; set; }
        public SetItems SetItems { get; set; }
    }

    [AutoMap(typeof(Voucher))]
    public class VoucherDto : Voucher
    {
    }

    public class OrderFilterDto
    {
        public int? Type { get; set; }
        public int? State { get; set; }
        public int MaxResultCount { get; set; }
        public int SkipCount { get; set; }
    }

    [AutoMap(typeof(ReportStore))]
    public class ReportStoresDto : ReportStore
    {
    }

    public enum GetItemsRequestCase
    {
        User = 1,
        Provider = 2,
        Admin = 3
    }

    public class GetItemsDto
    {
        [CanBeNull] public string Search { get; set; }
        public int? Status { get; set; }
        public long? CategoryId { get; set; }
        public long? ProviderId { get; set; }
        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
        public int? TenantId { get; set; }
        public int? SkipCount { get; set; }
        public int? MaxResultCount { get; set; }

        public int? RequestCase { get; set; }
    }

    public class GetItemDetailDto
    {
        public long Id { get; set; }
    }
   
    public enum ITEM_STATUS
    {
        NORMAL = 1,   // trạng thái bình thường
        UNLIST = 4,   // trạng thái đã ẩn
    }

    public enum ItemCondition
    {
        NEW = 1,
        USED = 2
    }

    public class ItemTierVariation
    {
        public string? Name { get; set; }
        public List<string>? OptionList { get; set; }
    }

    public enum ItemAttributeInputType
    {
        Text = 1,
        TextArea = 2,
        Number = 3,
        Date = 4,
        Select = 5,
        MultiSelect = 6,
        Checkbox = 7,
        Radio = 8,
    }

    public enum ItemAttributeDataType
    {
        String = 1,
        Int = 2,
        Float = 3,
        Date = 4,
        Boolean = 5,
        Array = 6,
    }

    public class CreateItemDto
    {
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public int? ProviderId { get; set; }
        public int? CategoryId { get; set; }
        public string Sku { get; set; }
        public List<string> ImageUrlList { get; set; }
        public List<string> VideoUrlList { get; set; }
        public string Description { get; set; }
        public string SizeInfo { get; set; }
        public string LogisticInfo { get; set; }
        public ITEM_STATUS? Status { get; set; }
        public ItemCondition? Condition { get; set; }
        public string ComplaintPolicy { get; set; }
        public List<AttributeOfCreateItemDto> AttributeList { get; set; }
        public List<TierVariationOfCreateItemDto> TierVariationList { get; set; }
        public List<ModelOfCreateItemDto> ModelList { get; set; }
    }

    public class DeleteItemDto
    {
        public long Id { get; set; }
    }

    public class AttributeOfCreateItemDto
    {
        public int Id { get; set; }
        public List<string> UnitList { get; set; }
        public List<string> ValueList { get; set; }
    }

    public class TierVariationOfCreateItemDto
    {
        public string Name { get; set; }
        public List<string> OptionList { get; set; }
    }

    public class ModelOfCreateItemDto
    {
        public string Sku { get; set; }
        public int? Stock { get; set; }
        public double? OriginalPrice { get; set; }
        public double? CurrentPrice { get; set; }
        public List<int> TierIndex { get; set; }
        [CanBeNull] public string ImageUrl { get; set; }
    }

    public class CreateItemAttributeDto
    {
        public int? TenantId { get; set; }
        public long? CategoryId { get; set; }
        public string? Name { get; set; }
        public string? DisplayName { get; set; }
        public string? Description { get; set; }
        public ItemAttributeDataType? DataType { get; set; }
        public ItemAttributeInputType? InputType { get; set; }
        public bool? IsRequired { get; set; }
        public List<string>? UnitList { get; set; }
        public List<string>? ValueList { get; set; }
    }

    public class UpdateItemAttributeDto : CreateItemAttributeDto
    {
        public long Id { get; set; }
        public bool IsChooseToDelete { get; set; }
    }

    public class GetAllItemAttributesDto
    {
        public int? TenantId { get; set; }
        public long? CategoryId { get; set; }
        public string Search { get; set; }
    }

    public class CreateCategory
    {
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public long? ParentId { get; set; }
        public long? BusinessType { get; set; }
        public string IconUrl { get; set; }
        public bool? HasChildren { get; set; }
    }

    [AutoMap(typeof(CreateCategory))]
    public class UpdateCategory : CreateCategory
    {
        public long Id { get; set; }
    }

    public class CartItemDto
    {
        public long ItemModelId { get; set; }
        public int Quantity { get; set; }
    }

    public class CreateListItemAttributesDto
    {
        public List<CreateItemAttributeDto> ItemAttributes { get; set; }
    }

    //cart
    public class GetCartDto
    {
        public long UserId { get; set; }
    }

    public class UpdateCartDto
    {
        public List<CartItem> Items { get; set; }
    }

    public class CartItem
    {
        public long ItemModelId { get; set; }
        public int Quantity { get; set; }
        public long ProviderId { get; set; }
    }

    public class AddItemModelToCartDto
    {
        public long ItemModelId { get; set; }
        public int Quantity { get; set; }
        public long ProviderId { get; set; }
    }

    //checkout - item
    public class CheckoutItem
    {
        public int ItemModelId { get; set; }
        public int Number { get; set; }
    }

    public class CheckoutDto
    {
        public List<CheckoutItem> ItemModelList { get; set; }
    }

    //Order
    public class OrderItemModel : Entity<long>, IDeletionAudited, IMayHaveTenant
    {
        public string? Name { get; set; }
        public int? TenantId { get; set; }
        public long? ItemId { get; set; }
        public bool? IsDefault { get; set; }
        public string? Sku { get; set; }
        public int? Stock { get; set; }
        public double? OriginalPrice { get; set; }
        public double? CurrentPrice { get; set; }
        public string? ImageUrl { get; set; }

        // Eg: [0,2], [1]
        public List<int>? TierIndex { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletionTime { get; set; }
        public long? DeleterUserId { get; set; }
        public int Quantity { get; set; }
        public string? ItemName { get; set; }
    }

    public class RecipientAddressEntity
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string? Town { get; set; }
        public string City { get; set; }
        public string FullAddress { get; set; }
    }

    public class TrackingInfoEntity
    {
        public string TrackingItemTime { get; set; }
        public int TrackingItemState { get; set; }
        public string TrackingItemDetail { get; set; }
    }

    public class CreateOrderDto
    {
        public int? TenantId { get; set; }
        public int? ProviderId { get; set; }
        public string OrderCode { get; set; }
        public long TotalPrice { get; set; }
        public int Type { get; set; }
        public List<OrderItemModel>? OrderItemList { get; set; }
        public RecipientAddressEntity RecipientAddress { get; set; }
        public string? PaymentMethod { get; set; }
    }

    public class UpdateOrderDto
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public int? ProviderId { get; set; }
        public string OrderCode { get; set; }
        public long TotalPrice { get; set; }
        public int State { get; set; }
        public int Type { get; set; }
        public long OrdererId { get; set; }
        public List<OrderItemModel>? OrderItemList { get; set; }
        public List<TrackingInfoEntity>? TrackingInfo { get; set; }
        public RecipientAddressEntity RecipientAddress { get; set; }
        public string? PaymentMethod { get; set; }
    }

    public class UpdateStateOrderDto
    {
        public long Id { get; set; }
        public int CurrentState { get; set; }
        public int UpdateState { get; set; }
    }
}