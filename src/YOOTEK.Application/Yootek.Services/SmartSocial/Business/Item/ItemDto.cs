#nullable enable
using Yootek.Common;
using Yootek.Yootek.Services.Yootek.SmartSocial.BusinessNEW.BusinessDto;
using System.Collections.Generic;

namespace Yootek.Services.SmartSocial.Items.Dto
{
    public class UpdateCartInputDto
    {
        public List<CartItem> Items { get; set; }
    }

    public class AddItemModelToCartInputDto
    {
        public long ItemModelId { get; set; }
        public int Quantity { get; set; }
        public long ProviderId { get; set; }
    }

    public class GetItemsByAdminInputDto : CommonInputDto
    {
        public string Search { get; set; }
        public long? CategoryId { get; set; }
        public long? ProviderId { get; set; }
        public OrderByItem? OrderBy { get; set; }
        public int? FormId { get; set; }
        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
        public int? TenantId { get; set; }
        public int? MinStock { get; set; }
        public int? MaxStock { get; set; }
        public int? MinSales { get; set; }
        public int? MaxSales { get; set; }
        public int? Rating { get; set; }
        public ItemCondition? Condition { get; set; }
    }
    public class GetItemsMainPageByUserInputDto : CommonInputDto
    {
        public int? ItemServiceType { get; set; }
    }
    public class GetItemsRandomByUserInputDto : CommonInputDto
    {
        public int? ItemServiceType { get; set; }
    }
    public class GetItemsByPartnerInputDto : CommonInputDto
    {
        public long? CategoryId { get; set; }
        public long? ProviderId { get; set; }
        public OrderByItem? OrderBy { get; set; }
        public FORM_PARTNER_GET_ITEMS? FormId { get; set; }
        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
        public int? TenantId { get; set; }
        public int? MinStock { get; set; }
        public int? MaxStock { get; set; }
        public int? MinSales { get; set; }
        public int? MaxSales { get; set; }
        public int? Rating { get; set; }
        public int? Type { get; set; }
        public ItemCondition? Condition { get; set; }
        public bool IsItemBooking { get; set; }
    }
    public class GetItemsByUserInputDto : CommonInputDto
    {
        public long? CategoryId { get; set; }
        public int? BusinessType { get; set; }
        public int? Type { get; set; }
        public long? ProviderId { get; set; }
        public OrderByItem? OrderBy { get; set; }
        public FORM_USER_GET_ITEMS FormId { get; set; }
        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
        public int? TenantId { get; set; }
        public int? Rating { get; set; }
        public ItemCondition? Condition { get; set; }
        public bool IsItemBooking { get; set; }
    }

    public class GetAllCvByOwnerInputDto : CommonInputDto
    {
        public int? Type { get; set; }
        public FORM_PARTNER_GET_ITEMS? FormId { get; set; }
    }

    public class GetAllItemFavouriteInputDto : CommonInputDto
    {
    }
    public class CreateItemInputDto
    {
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public long ProviderId { get; set; }
        public long CategoryId { get; set; }
        public string? Sku { get; set; }
        public List<string> ImageUrlList { get; set; }
        public List<string>? VideoUrlList { get; set; } = new List<string>();
        public string? Description { get; set; }
        public string? SizeInfo { get; set; }
        public string? LogisticInfo { get; set; }
        public int Type { get; set; }
        public ItemCondition Condition { get; set; }
        public string? ComplaintPolicy { get; set; }
        public List<AttributeOfCreateItemDto>? AttributeList { get; set; } = new List<AttributeOfCreateItemDto>();
        public List<TierVariationOfCreateItemDto> TierVariationList { get; set; } =
            new List<TierVariationOfCreateItemDto>();
        public List<ModelOfCreateItemDto> ModelList { get; set; }
        public string? Properties { get; set; }
    }

    public class CreateItemBookingInputDto
    {
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public long? ProviderId { get; set; }
        public List<string> ImageUrlList { get; set; }
        public List<string> VideoUrlList { get; set; }
        public string Description { get; set; }
        public string SizeInfo { get; set; }
        public int Type { get; set; }
        public string Properties { get; set; }
        public ModelOfCreateItemBookingDto ItemModel { get; set; }
    }
    public class CreateItemCvInputDto
    {
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public long? ProviderId { get; set; }
        public List<string> ImageUrlList { get; set; }
        public List<string> VideoUrlList { get; set; }
        public string Description { get; set; }
        public string SizeInfo { get; set; }
        public int Type { get; set; }
        public string Properties { get; set; }
        public ModelOfCreateItemBookingDto ItemModel { get; set; }
    }

    public class ModelOfCreateItemBookingDto
    {
        public double? OriginalPrice { get; set; }
        public double? CurrentPrice { get; set; }
    }

    public class UpdateItemInputDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string? Sku { get; set; }
        public List<string>? ImageUrlList { get; set; }
        public List<string>? VideoUrlList { get; set; }
        public string? Description { get; set; }
        public string? SizeInfo { get; set; }
        public string? LogisticInfo { get; set; }
        public ItemCondition Condition { get; set; }
        public string? ComplaintPolicy { get; set; }
        public List<AttributeOfCreateItemDto> AttributeList { get; set; }
        public List<TierVariationOfCreateItemDto> TierVariationList { get; set; }
        public List<ItemModelUpdate> ModelList { get; set; }
    }

    public class ItemModelUpdate
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Sku { get; set; }
        public int? Stock { get; set; }
        public double? OriginalPrice { get; set; }
        public double? CurrentPrice { get; set; }
        public string ImageUrl { get; set; }
    }

    public class UpdateItemBookingInputDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public List<string>? ImageUrlList { get; set; }
        public List<string>? VideoUrlList { get; set; }
        public string? Description { get; set; }
        public string? SizeInfo { get; set; }
        public string? LogisticInfo { get; set; }
        public ItemCondition Condition { get; set; }
        public List<AttributeOfCreateItemDto> AttributeList { get; set; }
        public string? Properties { get; set; }
    }

    public class UpdateCvByOwnerDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public List<string>? ImageUrlList { get; set; }
        public string? Description { get; set; }
        public ItemCondition Condition { get; set; }
        public string? Properties { get; set; }
    }
    public class LikeItemInputDto
    {
        public long Id { get; set; }
    }
    public class UnLikeItemInputDto
    {
        public long Id { get; set; }
    }
    public class StockItemModel
    {
        public long Id { get; set; }
        public int Stock { get; set; }
    }
    public class UpdateListStockItemModelInputDto
    {
        public long Id { get; set; } // Id của Item
        public List<StockItemModel> Items { get; set; }
    }

    public class PriceItemModel
    {
        public long Id { get; set; }
        public int OriginalPrice { get; set; }
        public int CurrentPrice { get; set; }
    }
    public class UpdateListPriceItemModelInputDto
    {
        public long Id { get; set; } // Id của Item
        public List<PriceItemModel> Items { get; set; }
    }

    public class ApproveListItemsInputDto
    {
        public List<long> Ids { get; set; }
    }
    public class ApproveItemInputDto
    {
        public long Id { get; set; }
    }

    public enum ItemStatus
    {
        PENDING = 1,
        ACTIVATED = 2,   // đang hoạt động

        // VIOLATION = 3,   // vi phạm, bị tạm khóa 
        DEBOOSTED = 31,
        BANNED = 32,
        DELETED_BY_ADMIN = 33,

        DELETED = 4,  // đã xóa (có thể khôi phục)
        HIDDEN = 5,  //  trạng thái ẩn
        BLOCK = 6,  // ngừng kinh doanh
    }
    public enum ItemStatusCreateAndUpdate
    {
        PENDING = 1,
        HIDDEN = 5,
    }
    public enum OrderByItem
    {
        POPULAR = 1,  // phổ biến (view count)
        NEWEST = 2,   // gần đây 
        TOP_SALES = 3,  // bán chạy (số lượng đã bán)
        PRICE_ASC = 4,   // giá tăng dần 
        PRICE_DESC = 5,   // giá giảm dần  
        STOCK_ASC = 6,  // tồn kho tăng dần
        STOCK_DESC = 7,    // tồn kho giảm dần
        SALES_ASC = 8,      // doanh số tăng dần (số lượng đã bán)
        SALES_DESC = 9,     // doanh số giảm dần (số lượng đã bán)
        RATING_ASC = 10,    // sao đánh giá tăng dần
        RATING_DESC = 11,   // sao đánh giá giảm dần
    }
    public enum FORM_USER_GET_ITEMS
    {
        ALL = 30,
        BLOCK = 31,
    }
    public enum FORM_PARTNER_GET_ITEMS
    {
        ALL = 10,
        LIVE = 11,
        SOLD_OUT = 12,
        REVIEWING = 13,
        VIOLATION = 14,
        VIOLATION_BANNED = 141,
        VIOLATION_DEBOOSTED = 142,
        VIOLATION_ADMIN_DELETED = 143,
        DELISTED = 15,
        INACTIVED = 16,
    }
}
