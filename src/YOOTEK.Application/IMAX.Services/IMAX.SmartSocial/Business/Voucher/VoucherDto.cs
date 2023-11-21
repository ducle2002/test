using IMAX.Common;
using System;
using System.Collections.Generic;

namespace IMAX.Services.SmartSocial.Vouchers.Dto
{
    public class Buyer
    {
        public long UserId { get; set; }
        public int Count { get; set; }
    }

    public class GetAllVouchersByPartnerInputDto : CommonInputDto
    {
        public int? TenantId { get; set; }
        public long? ProviderId { get; set; }
        public string Search { get; set; }
        public FORM_PARTNER_GET_VOUCHERS FormId { get; set; }
        public bool? IsAdminCreate { get; set; }
    }

    public class GetAllVouchersByUserInputDto : CommonInputDto
    {
        public int? TenantId { get; set; }
        public long? ProviderId { get; set; }
        public string Search { get; set; }
        public FORM_USER_GET_VOUCHERS FormId { get; set; }
        public bool? IsAdminCreate { get; set; }
    }

    public class CreateVoucherInputDto
    {
        public int? TenantId { get; set; }
        public long? ProviderId { get; set; }
        public int Type { get; set; }
        public int Scope { get; set; }
        public string Description { get; set; }
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
        public bool? IsAdminCreate { get; set; } = false;
        public int MaxDistributionBuyer { get; set; }
        public List<long> ListItems { get; set; }
    }

    public class CheckVoucherAvailableInputDto
    {
        public List<long> Items { get; set; }
    }

    public class UpdateVoucherInputDto
    {
        public long Id { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public long MinBasketPrice { get; set; }
        public long MaxPrice { get; set; }
        public int Percentage { get; set; }
        public int DiscountAmount { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public int MaxDistributionBuyer { get; set; }
    }

    public class ReceiveVoucherInputDto
    {
        public long Id { get; set; }
    }
    public class DeleteVoucherInputDto
    {
        public long Id { get; set; }
    }
    public class EndedVoucherInputDto
    {
        public long Id { get; set; }
    }
    public class StartEarlyVoucherInputDto
    {
        public long Id { get; set; }
    }

    #region admin
    public class AdminGetAllVouchersInputDto : CommonInputDto
    {
        public int? TenantId { get; set; }
        public long? ProviderId { get; set; }
        public string Search { get; set; }
        public FORM_ADMIN_GET_VOUCHERS FormId { get; set; }
        // public bool? IsAdminCreate { get; set; }
    }
    #endregion
    public enum VOUCHER_STATE
    {
        UPCOMING = 1,
        ACTIVATED = 2,
        EXPIRED = 3,
        HIDDEN = 4,
    }

    public enum FORM_ADMIN_GET_VOUCHERS
    {
        // ADMIN
        ALL = 10,
        ON_GOING = 11,
        UPCOMING = 12,
        EXPIRED = 13,
        HIDDEN = 14,
    }

    // enum
    public enum FORM_PARTNER_GET_VOUCHERS
    {
        // PARTNER
        ALL = 20,
        ON_GOING = 21,
        UPCOMING = 22,
        EXPIRED = 23,
        HIDDEN = 24,
    }

    public enum FORM_USER_GET_VOUCHERS
    {
        // USER
        ALL = 30,
        ON_GOING = 31,
        UPCOMING = 32,
        EXPIRED = 33,
    }
}
