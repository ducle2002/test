using Abp.Application.Services;
using Abp.Runtime.Session;
using Abp.UI;
using Grpc.Core;
using Yootek.App.Grpc;
using Yootek.Application.Protos.Business.Items;
using Yootek.Application.Protos.Business.Providers;
using Yootek.Common.DataResult;
using Yootek.Yootek.Services.Yootek.SmartSocial.BusinessNEW.BusinessDto;
using Yootek.Services.SmartSocial.Items.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Yootek.App.ServiceHttpClient.Dto;
using Yootek.App.ServiceHttpClient.Dto.Business;
using Yootek.App.ServiceHttpClient.Yootek.SmartCommunity;

namespace Yootek.Services.SmartSocial.Items
{
    public interface IItemAppService : IApplicationService
    {
        /// <summary>
        /// 1. Đối tác :
        /// a. Lấy danh sách sản phẩm của cửa hàng mình tạo
        /// b. Thêm sửa xóa sản phẩm
        /// 2. Người dùng:
        /// a. Lấy danh sách sản phẩm được duyệt
        /// b. API lấy danh sách sản phẩm theo API search chung( có thể elasticsearch)
        /// </summary>

        // Item
        Task<DataResult> GetAllItemsByPartnerAsync(GetItemsByPartnerInputDto input);
        Task<DataResult> GetAllItemsByUserAsync(GetItemsByUserInputDto input);
        Task<DataResult> GetAllItemsRandomAsync(GetItemsRandomByUserInputDto input);
        Task<DataResult> GetAllItemsFavouriteAsync(GetAllItemFavouriteInputDto input);
        Task<DataResult> GetAllCvByOwnerAsync(GetAllCvByOwnerInputDto input);
        Task<DataResult> GetItemByIdAsync(long id);
        Task<DataResult> CreateItemAsync(CreateItemInputDto input);
        Task<DataResult> CreateItemBookingAsync(CreateItemBookingInputDto input);
        Task<DataResult> CreateItemCvAsync(CreateItemCvInputDto input);
        Task<DataResult> UpdateItemAsync(UpdateItemInputDto input);
        Task<DataResult> UpdateItemBookingAsync(UpdateItemBookingInputDto input);
        Task<DataResult> UpdateListStockItemModelAsync(UpdateListStockItemModelInputDto input);
        Task<DataResult> UpdateListPriceItemModelAsync(UpdateListPriceItemModelInputDto input);
        Task<DataResult> LikeItemAsync(LikeItemInputDto input);
        Task<DataResult> UnLikeItemAsync(UnLikeItemInputDto input);
        Task<DataResult> DeleteItemAsync(long id);
        Task<DataResult> HiddenItemAsync(long id);
        Task<DataResult> ShowItemAsync(long id);
        // Cart
        Task<DataResult> GetCartAsync();
        Task<DataResult> UpdateCartAsync(UpdateCartInputDto request);
        Task<DataResult> AddItemModelToCart(AddItemModelToCartInputDto request);
    }
    public class ItemAppService : YootekAppServiceBase, IItemAppService
    {
        private readonly ItemProtoGrpc.ItemProtoGrpcClient _itemProtoClient;
        private readonly ProviderProtoGrpc.ProviderProtoGrpcClient _providerProtoClient;

        public ItemAppService(
            ItemProtoGrpc.ItemProtoGrpcClient itemProtoClient,
            ProviderProtoGrpc.ProviderProtoGrpcClient providerProtoClient
            )
        {
            _itemProtoClient = itemProtoClient;
            _providerProtoClient = providerProtoClient;
        }
        public async Task<DataResult> GetAllItemsByPartnerAsync(GetItemsByPartnerInputDto input)
        {
            try
            {
                GetItemsByPartnerRequest request = new()
                {
                    Keyword = input.Keyword ?? "",
                    CategoryId = input.CategoryId ?? 0,
                    ProviderId = input.ProviderId ?? 0,
                    MinPrice = input.MinPrice ?? 0,
                    MaxPrice = input.MaxPrice ?? 0,
                    MinStock = input.MinStock ?? 0,
                    MaxStock = input.MaxStock ?? 0,
                    MinSales = input.MinSales ?? 0,
                    MaxSales = input.MaxSales ?? 0,
                    OrderBy = (int)(input.OrderBy ?? 0),
                    FormId = (int)(input.FormId ?? 0),
                    TenantId = AbpSession.TenantId ?? 0,
                    Rating = input.Rating ?? 0,
                    Condition = (int)(input.Condition ?? 0),
                    UserId = AbpSession.UserId ?? 0,
                    Type = input.Type ?? 0,
                    IsItemBooking = input.IsItemBooking,
                    SkipCount = input.SkipCount,
                    MaxResultCount = input.MaxResultCount,
                };
                GetItemsByPartnerResponse result = await _itemProtoClient.GetItemsByPartnerAsync(
                    request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Items, "Get items by partner success", result.TotalCount);
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

        public async Task<DataResult> GetAllItemsByUserAsync(GetItemsByUserInputDto input)
        {
            try
            {
                GetItemsByUserRequest request = new()
                {
                    Keyword = input.Keyword ?? "",
                    CategoryId = input.CategoryId ?? 0,
                    BusinessType = input.BusinessType ?? 0,
                    ProviderId = input.ProviderId ?? 0,
                    MinPrice = input.MinPrice ?? 0,
                    MaxPrice = input.MaxPrice ?? 0,
                    OrderBy = (int)(input.OrderBy ?? 0),
                    FormId = (int)input.FormId,
                    TenantId = input.TenantId ?? 0,
                    Rating = input.Rating ?? 0,
                    Condition = (int)(input.Condition ?? 0),
                    Type = input.Type ?? 0,
                    IsItemBooking = input.IsItemBooking,
                    SkipCount = input.SkipCount,
                    MaxResultCount = input.MaxResultCount,
                };
                GetItemsByUserResponse result = await _itemProtoClient.GetItemsByUserAsync(
                    request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Items, "Get items by user success", result.TotalCount);
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

        public async Task<DataResult> GetAllCvByOwnerAsync(GetAllCvByOwnerInputDto input)
        {
            try
            {
                GetAllCvByOwnerRequest request = new()
                { 
                    FormId = (int)(input.FormId ?? 0),
                    Type = input.Type ?? 0, 
                };
                GetAllCvByOwnerResponse result = await _itemProtoClient.GetAllCvByOwnerAsync(
                    request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Items, "Get Cv by Owner success", result.TotalCount);
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

        public async Task<DataResult> GetAllItemsMainPageAsync(GetItemsMainPageByUserInputDto input)
        {
            try
            {
                GetItemsByUserMainPageRequest request = new()
                {
                    ItemServiceType = input.ItemServiceType ?? 0,
                    SkipCount = input.SkipCount,
                    MaxResultCount = input.MaxResultCount,
                };
                GetItemsByUserMainPageResponse result = await _itemProtoClient.GetItemsByUserMainPageAsync(
                    request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Items, "Get items main page success", result.TotalCount);
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

        public async Task<DataResult> GetAllItemsRandomAsync(GetItemsRandomByUserInputDto input)
        {
            try
            {
                GetAllItemsRandomRequest request = new()
                {
                    ItemServiceType = input.ItemServiceType ?? 0,
                    SkipCount = input.SkipCount,
                    MaxResultCount = input.MaxResultCount,
                };
                GetAllItemsRandomResponse result = await _itemProtoClient.GetAllItemsRandomAsync(
                    request, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Items, "Get items random by user success", result.TotalCount);
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

        public async Task<DataResult> GetItemByIdAsync(long id)
        {
            try
            {
                GetItemDetailRequest request = new() { Id = id };
                GetItemDetailResponse result = await _itemProtoClient.GetItemDetailAsync(request,
                    MetadataGrpc.MetaDataHeader(AbpSession));
                if (result.Data != null && result.Data.CreatorUserId != AbpSession.UserId)
                {
                    await _itemProtoClient.UpdateViewCountAsync(new UpdateViewCountRequest()
                    {
                        Id = id
                    }, MetadataGrpc.MetaDataHeader(AbpSession));
                    await _itemProtoClient.IncreaseWeightCategoryAsync(new IncreaseWeightCategoryRequest()
                    {
                        UserId = (long)AbpSession.UserId,
                        ItemId = id,
                    }, MetadataGrpc.MetaDataHeader(AbpSession));
                }
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
        public async Task<DataResult> GetAllItemsFavouriteAsync(GetAllItemFavouriteInputDto input)
        {
            try
            {
                GetAllItemFavouriteResponse result = await _itemProtoClient.GetAllItemFavouriteAsync(
                    new GetAllItemFavouriteRequest()
                    {
                        SkipCount = input.SkipCount,
                        MaxResultCount = input.MaxResultCount,
                    },
                    MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Items, "Get list favourite success", result.TotalCount);
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
        public async Task<DataResult> CreateItemAsync(CreateItemInputDto input)
        {
            try
            {
                #region Validation
                ValidateCondition((int)input.Condition);
                #endregion
                CreateItemResponse result = await _itemProtoClient.CreateItemAsync(new CreateItemRequest()
                {
                    TenantId = input.TenantId ?? 0,
                    Name = input.Name,
                    Sku = input.Sku ?? "",  
                    Condition = (int)input.Condition,
                    Status = (int)ItemStatus.PENDING,
                    Description = input.Description ?? "",
                    CategoryId = (long)input.CategoryId,
                    ProviderId = (long)input.ProviderId,
                    ImageUrlList = { input.ImageUrlList ?? new List<string>() },
                    VideoUrlList = { input.VideoUrlList ?? new List<string>() },
                    ComplaintPolicy = input.ComplaintPolicy ?? "",
                    SizeInfo = input.SizeInfo ?? "",
                    LogisticInfo = input.LogisticInfo ?? "",
                    AttributeList =
                        {
                            input.AttributeList.Select(x => new PAttributeOfItem()
                            {
                                Id = x.Id,
                                UnitList = { x.UnitList },
                                ValueList = { x.ValueList }
                            })
                        },
                    TierVariationList =
                        {
                            input.TierVariationList.Select(x => new PItemTierVariation()
                            {
                                Name = x.Name,
                                OptionList = { x.OptionList }
                            })
                        },
                    ModelList =
                        {
                            input.ModelList.Select(x => new PAddModelToItem()
                            {
                                Sku = x.Sku ?? "",
                                Stock = x.Stock ?? 0,
                                ImageUrl = x.ImageUrl,
                                OriginalPrice = x.OriginalPrice ?? 0,
                                CurrentPrice = x.CurrentPrice ?? 0,
                                TierIndex = { x.TierIndex },
                            })
                        },
                    Type = input.Type,
                    IsItemBooking = false,
                    Properties = input.Properties ?? "",
                }, MetadataGrpc.MetaDataHeader(AbpSession)); ;
                return DataResult.ResultSuccess(result.Data, "Create item success");
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

        public async Task<DataResult> CreateItemBookingAsync(CreateItemBookingInputDto input)
        {
            try
            {
                CreateItemResponse result = await _itemProtoClient.CreateItemAsync(new CreateItemRequest()
                {
                    TenantId = input.TenantId ?? 0,
                    Name = input.Name,
                    Sku = "",
                    Condition = (int)ItemCondition.NEW,
                    Status = (int)ItemStatus.PENDING,
                    Description = input.Description ?? "",
                    CategoryId = 0,
                    ProviderId = (long)input.ProviderId,
                    ImageUrlList = { input.ImageUrlList ?? new List<string>() },
                    VideoUrlList = { input.VideoUrlList ?? new List<string>() },
                    ComplaintPolicy = "",
                    SizeInfo = input.SizeInfo ?? "",
                    LogisticInfo = "",
                    ModelList =
                        {
                            new PAddModelToItem()
                            {
                                Sku = "",
                                Stock = 0,
                                ImageUrl = input.ImageUrlList[0] ?? "",
                                OriginalPrice = input.ItemModel.OriginalPrice ?? 0,
                                CurrentPrice = input.ItemModel.CurrentPrice ?? 0,
                                TierIndex = { new List<int>() },
                            }
                        },
                    Type = input.Type,
                    IsItemBooking = true,
                    Properties = input.Properties ?? "",
                }, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Data, "Create item booking success");
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
        public async Task<DataResult> CreateItemCvAsync(CreateItemCvInputDto input)
        {
            try
            {
                if (AbpSession.UserId != null)
                {
                    CreateItemResponse result = await _itemProtoClient.CreateItemAsync(new CreateItemRequest()
                    {
                        TenantId = input.TenantId ?? 0,
                        Name = input.Name,
                        Sku = "",
                        Condition = (int)ItemCondition.NEW,
                        Status = (int)ItemStatus.PENDING,
                        Description = input.Description ?? "",
                        CategoryId = 0,
                        ProviderId = -1,
                        ImageUrlList = { input.ImageUrlList ?? new List<string>() },
                        VideoUrlList = { input.VideoUrlList ?? new List<string>() },
                        ComplaintPolicy = "",
                        SizeInfo = input.SizeInfo ?? "",
                        LogisticInfo = "",
                        ModelList =
                        {
                            new PAddModelToItem()
                            {
                                Sku = "",
                                Stock = 0,
                                ImageUrl = input.ImageUrlList[0] ?? "",
                                OriginalPrice = input.ItemModel.OriginalPrice ?? 0,
                                CurrentPrice = input.ItemModel.CurrentPrice ?? 0,
                                TierIndex = { new List<int>() },
                            }
                        },
                        Type = input.Type,
                        IsItemBooking = true,
                        Properties = input.Properties ?? "",
                    }, MetadataGrpc.MetaDataHeader(AbpSession));
                    return DataResult.ResultSuccess(result.Data, "Create CV success");
                }
                else
                {
                    throw new UserFriendlyException("Must Login");
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

        #region Update Item
        public async Task<DataResult> UpdateItemAsync(UpdateItemInputDto input)
        {
            try
            {
                #region Validation
                ValidateCondition((int)input.Condition);
                #endregion

                UpdateItemByPartnerResponse result = await _itemProtoClient.UpdateItemByPartnerAsync(new UpdateItemByPartnerRequest()
                {
                    Id = input.Id,
                    Name = input.Name,
                    Sku = input.Sku ?? "",
                    Description = input.Description ?? "",
                    ImageUrlList = { input.ImageUrlList ?? new List<string>() },
                    VideoUrlList = { input.VideoUrlList ?? new List<string>() },
                    SizeInfo = input.SizeInfo ?? "",
                    LogisticInfo = input.LogisticInfo ?? "",
                    Condition = (int)input.Condition,
                    ComplaintPolicy = input.ComplaintPolicy ?? "",
                    AttributeList =
                        {
                            input.AttributeList.Select(x => new PAttributeOfItem()
                            {
                                Id = x.Id,
                                UnitList = { x.UnitList },
                                ValueList = { x.ValueList }
                            })
                        },
                    TierVariationList =
                        {
                            input.TierVariationList.Select(x => new PItemTierVariation()
                            {
                                Name = x.Name,
                                OptionList = { x.OptionList }
                            })
                        },
                    ModelList =
                        {
                            input.ModelList.Select(x => new PItemModel()
                            {
                                Id = x.Id,
                                ItemId = 0,
                                TenantId = 0,
                                IsDefault = false,
                                Name = x.Name ?? "",
                                Sku = x.Sku,
                                Stock = x.Stock ?? 0,
                                ImageUrl = x.ImageUrl,
                                OriginalPrice = x.OriginalPrice ?? 0,
                                CurrentPrice = x.CurrentPrice ?? 0,
                                TierIndex = { new List<int>() },
                            })
                        },
                }, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Data, "Update item by partner success");
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
        #endregion

        #region Update Item Booking
        public async Task<DataResult> UpdateItemBookingAsync(UpdateItemBookingInputDto input)
        {
            try
            {
                #region Validation
                ValidateCondition((int)input.Condition);
                #endregion

                UpdateItemBookingByPartnerResponse result = await _itemProtoClient.UpdateItemBookingByPartnerAsync(new UpdateItemBookingByPartnerRequest()
                {
                    Id = input.Id,
                    Name = input.Name,
                    Properties = input.Properties ?? "",
                    Description = input.Description ?? "",
                    ImageUrlList = { input.ImageUrlList ?? new List<string>() },
                    VideoUrlList = { input.VideoUrlList ?? new List<string>() },
                    SizeInfo = input.SizeInfo ?? "",
                    LogisticInfo = input.LogisticInfo ?? "",
                    Condition = (int)input.Condition,
                    AttributeList =
                        {
                            input.AttributeList.Select(x => new PAttributeOfItem()
                            {
                                Id = x.Id,
                                UnitList = { x.UnitList },
                                ValueList = { x.ValueList }
                            })
                        },
                }, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Data, "Update item booking by partner success");
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
        #endregion

        #region Update Cv

        public async Task<DataResult> UpdateCvByOwner(UpdateCvByOwnerDto input)
        {
            try
            {
                #region Validation
                ValidateCondition((int)input.Condition);
                #endregion
                
                UpdateCvByOwnerResponse result = await _itemProtoClient.UpdateCvByOwnerAsync(new UpdateCvByOwnerRequest()
                {
                    Id = input.Id,
                    Name = input.Name,
                    Condition = (int)input.Condition,
                    Description = input.Description,
                    Properties = input.Properties,
                    ImageUrlList = { input.ImageUrlList ?? new List<string>() },
                    
                }, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Data, "Update cv by owner success");
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
        

        #endregion

        #region Update List Stock ItemModel
        public async Task<DataResult> UpdateListStockItemModelAsync(UpdateListStockItemModelInputDto input)
        {
            try
            {
                #region Validation 
                await ValidatePartnerPermission(input.Id, (long)AbpSession.UserId);
                #endregion

                UpdateListStockItemModelResponse result = await _itemProtoClient.UpdateListStockItemModelAsync(
                    new UpdateListStockItemModelRequest()
                    {
                        Items = { input.Items.Select(x =>
                        new PStockItemModel
                            {
                                Id = x.Id,
                                Stock = x.Stock,
                            })}
                    }, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Data, "Update list stock ItemModel success");
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
        #endregion

        #region Update List Price ItemModel
        public async Task<DataResult> UpdateListPriceItemModelAsync(UpdateListPriceItemModelInputDto input)
        {
            try
            {
                #region Validation 
                await ValidatePartnerPermission(input.Id, (long)AbpSession.UserId);
                #endregion

                UpdateListPriceItemModelResponse result = await _itemProtoClient.UpdateListPriceItemModelAsync(
                    new UpdateListPriceItemModelRequest()
                    {
                        Items = { input.Items.Select(x =>
                        new PPriceItemModel
                            {
                                Id = x.Id,
                                CurrentPrice = x.CurrentPrice,
                                OriginalPrice = x.OriginalPrice,
                            })}
                    }, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Data, "Update list price ItemModel success");
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
        #endregion

        #region Like item (add item to favourite)
        public async Task<DataResult> LikeItemAsync(LikeItemInputDto input)
        {
            try
            {
                AddItemFavouriteResponse result = await _itemProtoClient.AddItemFavouriteAsync(
                    new AddItemFavouriteRequest()
                    {
                        Id = input.Id,
                    }, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Data, "Add item to favourite success");
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
        #endregion

        #region UnLike item (add item to favourite)
        public async Task<DataResult> UnLikeItemAsync(UnLikeItemInputDto input)
        {
            try
            {
                RemoveItemFavouriteResponse result = await _itemProtoClient.RemoveItemFavouriteAsync(
                    new RemoveItemFavouriteRequest()
                    {
                        Id = input.Id,
                    }, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Data, "Remove item from favourite success");
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
        #endregion

        #region Hidden Item
        public async Task<DataResult> HiddenItemAsync(long id)
        {
            try
            {
                #region Validation 
                await ValidatePartnerPermission(id, (long)AbpSession.UserId);
                #endregion

                GetItemDetailResponse response = await _itemProtoClient.GetItemDetailAsync(
                    new GetItemDetailRequest() { Id = id }, MetadataGrpc.MetaDataHeader(AbpSession));
                if (response.Data.Status == (int)ItemStatus.ACTIVATED)
                {
                    UpdateStatusOfItemRequest request = new()
                    {
                        Id = id,
                        CurrentStatus = (int)ItemStatus.ACTIVATED,
                        UpdateStatus = (int)ItemStatus.HIDDEN,
                    };
                    UpdateStatusOfItemResponse result = await _itemProtoClient.UpdateStatusOfItemAsync(
                        request, MetadataGrpc.MetaDataHeader(AbpSession));
                    return DataResult.ResultSuccess(result.Data, "Hidden item success");
                };
                throw new UserFriendlyException("Cannot hidden item");
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
        #endregion

        #region Show Item
        public async Task<DataResult> ShowItemAsync(long id)
        {
            try
            {
                #region Validation 
                await ValidatePartnerPermission(id, (long)AbpSession.UserId);
                #endregion

                GetItemDetailResponse response = await _itemProtoClient.GetItemDetailAsync(
                    new GetItemDetailRequest() { Id = id }, MetadataGrpc.MetaDataHeader(AbpSession));
                if (response.Data.Status == (int)ItemStatus.HIDDEN)
                {
                    UpdateStatusOfItemRequest request = new()
                    {
                        Id = id,
                        CurrentStatus = (int)ItemStatus.HIDDEN,
                        UpdateStatus = (int)ItemStatus.ACTIVATED,
                    };
                    UpdateStatusOfItemResponse result = await _itemProtoClient.UpdateStatusOfItemAsync(
                        request, MetadataGrpc.MetaDataHeader(AbpSession));
                    return DataResult.ResultSuccess(result.Data, "Show item success");
                };
                throw new UserFriendlyException("Cannot show item");
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
        #endregion

        #region Delete Item
        public async Task<DataResult> DeleteItemAsync(long id)
        {
            try
            {
                #region Validation
                await ValidatePartnerPermission(id, (long)AbpSession.UserId);
                #endregion

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
        #endregion

        // Cart
        #region Get Cart
        public async Task<DataResult> GetCartAsync()
        {
            try
            {
                long userId = AbpSession.GetUserId();
                GetCartResponse result = await _itemProtoClient.GetCartAsync(new GetCartRequest()
                {
                    UserId = userId,
                }, MetadataGrpc.MetaDataHeader(AbpSession));
                return DataResult.ResultSuccess(result.Items, "Get cart success !", result.TotalCount);
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
        #endregion

        #region Update Cart
        public async Task<DataResult> UpdateCartAsync(UpdateCartInputDto request)
        {
            try
            {
                UpdateCartResponse result = await _itemProtoClient.UpdateCartAsync(new UpdateCartRequest()
                {
                    UserId = AbpSession.GetUserId(),
                    Items =
                    {
                        request.Items.Select(x => new PCartItem()
                        {
                            Quantity = x.Quantity,
                            ItemModelId = x.ItemModelId,
                            ProviderId = x.ProviderId,
                        })
                    },
                }, MetadataGrpc.MetaDataHeader(AbpSession));

                return DataResult.ResultSuccess(result.Data, "Update cart success !");
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
        #endregion

        #region Add ItemModel to Cart 
        public async Task<DataResult> AddItemModelToCart(AddItemModelToCartInputDto request)
        {
            try
            {
                AddItemModelToCartResponse result = await _itemProtoClient.AddItemModelToCartAsync(new AddItemModelToCartRequest()
                {
                    ItemModelId = request.ItemModelId,
                    Quantity = request.Quantity,
                    ProviderId = request.ProviderId,
                    UserId = AbpSession.GetUserId(),
                }, MetadataGrpc.MetaDataHeader(AbpSession));

                return DataResult.ResultSuccess(result.Data, "Add item model to cart success !");
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
        #endregion

        // Validation
        #region Validate
        private async Task<PItem> ValidatePartnerPermission(long itemId, long UserId)
        {
            GetItemDetailResponse response = await _itemProtoClient.GetItemDetailAsync(
                new GetItemDetailRequest() { Id = (long)itemId }, MetadataGrpc.MetaDataHeader(AbpSession));
            if (response.Data == null) throw new UserFriendlyException("Item not found");
            if (response.Data.CreatorUserId != UserId) throw new UserFriendlyException("You don't have permission");
            return response.Data;
        }
        private static void ValidateCondition(int conditionValue)
        {
            if (!Enum.IsDefined(typeof(ItemCondition), conditionValue))
            {
                throw new UserFriendlyException("Condition is invalid");
            }
        }
        #endregion
    }
}
