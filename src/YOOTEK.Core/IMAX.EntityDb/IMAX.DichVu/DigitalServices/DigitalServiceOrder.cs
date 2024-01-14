using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Yootek.EntityDb
{
	/// <summary>
	/// Khởi tạo đối tượng Dịch vụ
	/// </summary>
	///<author>ThoNV</author>
	///<created>25/10/2023</created>
    [Table("DigitalServiceOrder")]
    public class DigitalServiceOrder : FullAuditedEntity<long>, IMayHaveTenant
    {
       /// <summary>
		/// Địa chỉ nhận
		/// </summary>
		[Required]
		public  string Address { get; set; }
/// <summary>
		/// Ghi chú
		/// </summary>
		public  string Note { get; set; }
/// <summary>
		/// Trạng thái
		/// </summary>
		[Required]
		public  int Status { get; set; }
/// <summary>
		/// Khu đô thị
		/// </summary>
		[Required]
		public  long UrbanId { get; set; }
/// <summary>
		/// Tổng tiền
		/// </summary>
		[Required]
		public  long TotalAmount { get; set; }
/// <summary>
		/// Nội dung phản hồi
		/// </summary>
		public  string ResponseContent { get; set; }
/// <summary>
		/// Đánh giá
		/// </summary>
		public  int? RatingScore { get; set; }
/// <summary>
		/// Bình luận
		/// </summary>
		public  string Comments { get; set; }
/// <summary>
		/// Dịch vụ
		/// </summary>
		[Required]
		public  long ServiceId { get; set; }
/// <summary>
		/// Dịch vụ chi tiết
		/// </summary>
		public  string ServiceDetails { get; set; }
/// <summary>
		/// Tenant
		/// </summary>
		public  int? TenantId { get; set; }
        public DateTime? OrderDate { get; set; }
    }
}