using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Yootek.EntityDb
{
	/// <summary>
	/// Khởi tạo đối tượng Kho/vị trí tài sản
	/// </summary>
	///<author>ThoNV</author>
	///<created>01/10/2023</created>
    [Table("KhoTaiSan")]
    public class KhoTaiSan : FullAuditedEntity<long>, IMayHaveTenant
    {
       /// <summary>
		/// Mã kho
		/// </summary>
		[StringLength(100)]
		[Required]
		public  string Code { get; set; }
/// <summary>
		/// Tên kho
		/// </summary>
		[StringLength(250)]
		[Required]
		public  string Title { get; set; }
/// <summary>
		/// Địa điểm
		/// </summary>
		[StringLength(1000)]
		public  string DiaDiem { get; set; }
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