using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Yootek.EntityDb
{
    [Table("ERPStaffs")]
    public class ERPStaff : FullAuditedEntity<long>, IMayHaveTenant
    {
        /// <summary>
		/// Họ và tên
		/// </summary>
		public  string Fullname { get; set; }
/// <summary>
		/// Số điện thoại
		/// </summary>
		public  string Phone { get; set; }
/// <summary>
		/// Cửa hàng ID
		/// </summary>
		public  long SellerId { get; set; }
/// <summary>
		/// Ghi chú
		/// </summary>
		[CanBeNull]public  string Notes { get; set; }
/// <summary>
		/// Tenant
		/// </summary>
		public  int? TenantId { get; set; }
    }
}
