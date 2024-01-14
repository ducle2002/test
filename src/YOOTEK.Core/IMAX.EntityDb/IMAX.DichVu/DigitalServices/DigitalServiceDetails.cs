using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Yootek.EntityDb
{
	/// <summary>
	/// Khởi tạo đối tượng Chi tiết dịch vụ
	/// </summary>
	///<author>ThoNV</author>
	///<created>25/10/2023</created>
    [Table("DigitalServiceDetails")]
    public class DigitalServiceDetails : FullAuditedEntity<long>, IMayHaveTenant
    {
       /// <summary>
		/// Mã
		/// </summary>
		[Required]
		public  string Code { get; set; }
/// <summary>
		/// Tiêu đề
		/// </summary>
		[StringLength(250)]
		[Required]
		public  string Title { get; set; }
/// <summary>
		/// DỊch vụ
		/// </summary>
		public  long ?  ServicesId { get; set; }
/// <summary>
		/// Giá dịch vụ
		/// </summary>
		[Required]
		public  long Price { get; set; }
/// <summary>
		/// Mô tả
		/// </summary>
		public  string Description { get; set; }
/// <summary>
		/// Hình ảnh
		/// </summary>
		[Required]
		public  string Image { get; set; }
/// <summary>
		/// Tenant
		/// </summary>
		public  int? TenantId { get; set; }
    }
}