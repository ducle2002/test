using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Yootek.EntityDb
{
	/// <summary>
	/// Khởi tạo đối tượng Đơn vị tài sản
	/// </summary>
	///<author>ThoNV</author>
	///<created>01/10/2023</created>
    [Table("DonViTaiSan")]
    public class DonViTaiSan : FullAuditedEntity<long>, IMayHaveTenant
    {
       /// <summary>
		/// Mã đơn vị
		/// </summary>
		[StringLength(100)]
		[Required]
		public  string Code { get; set; }
/// <summary>
		/// Tên đơn vị
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
		/// Tenant
		/// </summary>
		public  int? TenantId { get; set; }
    }
}