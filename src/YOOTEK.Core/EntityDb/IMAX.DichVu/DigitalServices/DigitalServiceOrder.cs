using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yootek.Organizations.Interface;
namespace Yootek.EntityDb
{
    /// <summary>
    /// Khởi tạo đối tượng Dịch vụ
    /// </summary>
    ///<author>ThoNV</author>
    ///<created>25/10/2023</created>
    [Table("DigitalServiceOrder")]
    public class DigitalServiceOrder : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveBuilding, IMayHaveUrban
    {

        [StringLength(256)]
        public string ApartmentCode { get; set; }
        [StringLength(256)]
        public string Code { get; set; }
        /// <summary>
        /// Địa chỉ nhận
        /// </summary>
        [Required]
        public string Address { get; set; }
        /// <summary>
        /// Ghi chú
        /// </summary>
        public string Note { get; set; }
        /// <summary>
        /// Trạng thái
        /// </summary>
        [Required]
        public int Status { get; set; }
        /// <summary>
        /// Trạng thái thanh toán
        /// </summary>
        public DigitalServicePaymentState PaymentState { get; set; }
        /// <summary>
        /// Khu đô thị
        /// </summary>
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        /// <summary>
        /// Tổng tiền
        /// </summary>
        [Required]
        public long TotalAmount { get; set; }
        /// <summary>
        /// Dư nợ
        /// </summary>
        public decimal? TotalDebtOrBalance { get; set; }
        /// <summary>
        /// Nội dung phản hồi
        /// </summary>
        public string ResponseContent { get; set; }
        /// <summary>
        /// Đánh giá
        /// </summary>
        public int? RatingScore { get; set; }
        /// <summary>
        /// Bình luận
        /// </summary>
        public string Comments { get; set; }
        /// <summary>
        /// Dịch vụ
        /// </summary>
        [Required]
        public long ServiceId { get; set; }
        /// <summary>
        /// Dịch vụ chi tiết
        /// </summary>
        public string ServiceDetails { get; set; }
        /// <summary>
        /// Tenant
        /// </summary>
        public int? TenantId { get; set; }
        public DateTime? OrderDate { get; set; }
    }

    public enum DigitalServicePaymentState
    {
        Pending = 1,
        Paid = 2,
        Debt = 3,
        Balance = 4
    }
}