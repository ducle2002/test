using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Yootek.EntityDb
{
	/// <summary>
	/// Khởi tạo đối tượng Nhà sản xuất
	/// </summary>
	///<author>ThoNV</author>
	///<created>01/10/2023</created>
    [Table("NhaSanXuat")]
    public class NhaSanXuat : FullAuditedEntity<long>, IMayHaveTenant
    {
       /// <summary>
		/// Mã nhà sản xuất
		/// </summary>
		[StringLength(100)]
		[Required]
		public  string Code { get; set; }
/// <summary>
		/// Tên nhà sản xuất
		/// </summary>
		[StringLength(250)]
		[Required]
		public  string Title { get; set; }
/// <summary>
		/// Mô tả
		/// </summary>
		[StringLength(1000)]
		public  string Description { get; set; }
/// <summary>
		/// Địa chỉ
		/// </summary>
		[StringLength(1000)]
		public  string Address { get; set; }
/// <summary>
		/// Điện thoại
		/// </summary>
		[StringLength(50)]
		public  string Phone { get; set; }
/// <summary>
		/// Tenant
		/// </summary>
		public  int? TenantId { get; set; }
    }
}