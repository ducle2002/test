using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Yootek.EntityDb
{
	/// <summary>
	/// Khởi tạo đối tượng Danh mục ngành nghề
	/// </summary>
	///<author>ThoNV</author>
	///<created>24/11/2023</created>
    [Table("CareerCategory")]
    public class CareerCategory : FullAuditedEntity<long>, IMayHaveTenant
    {
       /// <summary>
		/// Tiêu đề
		/// </summary>
		[StringLength(250)]
		[Required]
		public  string Title { get; set; }
/// <summary>
		/// Mã
		/// </summary>
		[StringLength(50)]
		[Required]
		public  string Code { get; set; }
/// <summary>
		/// Mô tả
		/// </summary>
		public  string Description { get; set; }
/// <summary>
		/// Tenant
		/// </summary>
		public  int? TenantId { get; set; }
    }
}