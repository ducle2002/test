using Yootek.Common;
using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;
using JetBrains.Annotations;

namespace Yootek.Services.SmartSocial.Vouchers.Dto
{
    public class Buyer
    {
        public long UserId { get; set; }
        public int Count { get; set; }
    }

    public class VoucherDto : EntityDto<long>
    {
        public int? TenantId { get; set; }
        public long ProviderId { get; set; }
        public int Type { get; set; }
        public int Scope { get; set; }
        public string? Description { get; set; }
        public int DiscountType { get; set; }
        public string VoucherCode { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public int CurrentUsage { get; set; }
        public long MinBasketPrice { get; set; }
        public long MaxPrice { get; set; }
        public int? Percentage { get; set; }
        public long? DiscountAmount { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public int State { get; set; }
        public bool IsAdminCreate { get; set; }
        public int MaxDistributionBuyer { get; set; }
        public List<Buyer> ListUser { get; set; }
        public long? CreatorUserId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? LastModificationTime { get; set; }
        public List<long>? ListItems { get; set; }
    }
    
    public class GetAllVouchersByPartnerInputDto : CommonInputDto
    {
        public long? ProviderId { get; set; }
        public int? FormId { get; set; }
        public int? OrderBy { get; set; }
    }

    public class GetAllVouchersByUserInputDto : CommonInputDto
    {
        public long? ProviderId { get; set; }
        public int? FormId { get; set; }
        public int? OrderBy { get; set; }
    }
    
    public class GetVoucherByIdInputDto : EntityDto<long>
    {
    }

    public class CreateVoucherInputDto
    {
        public int? TenantId { get; set; }
        public long? ProviderId { get; set; }
        public int Type { get; set; }
        public int Scope { get; set; }
        public string? Description { get; set; }
        public int DiscountType { get; set; }
        public string VoucherCode { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public long MinBasketPrice { get; set; }
        public long MaxPrice { get; set; }
        public int Percentage { get; set; }
        public int DiscountAmount { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public int MaxDistributionBuyer { get; set; }
        public List<long>? ListItems { get; set; }
        public List<int>? DisplayChannelList { get; set; }
    }

    public class CheckVoucherAvailableInputDto
    {
        public List<long> Items { get; set; }
    }

    public class UpdateVoucherInputDto : EntityDto<long>
    {
        public string? Description { get; set; }
        public string? Name { get; set; }
        public int? Quantity { get; set; }
        public long? MinBasketPrice { get; set; }
        public long? MaxPrice { get; set; }
        public int? Percentage { get; set; }
        public int? DiscountAmount { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public int? MaxDistributionBuyer { get; set; }
    }
    
    public class UpdateListUserDto : EntityDto<long>
    {
        public Buyer Item { get; set; }
    }

    public class ReceiveVoucherInputDto : EntityDto<long>
    {
    }
    public class DeleteVoucherInputDto : EntityDto<long>
    {
    }
    public class EndedVoucherInputDto : EntityDto<long>
    {
    }
    public class StartEarlyVoucherInputDto : EntityDto<long>
    {
    }

    #region admin
    public class GetAllVouchersByAdminInputDto : CommonInputDto
    {
        public long? ProviderId { get; set; }
        public int? FormId { get; set; }
    }
    #endregion
}
