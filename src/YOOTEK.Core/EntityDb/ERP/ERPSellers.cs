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
    [Table("ERPSellers")]
    public class ERPSeller : FullAuditedEntity<long>, IMayHaveTenant
    {
        /// <summary>
		/// Tên cửa hàng
		/// </summary>
		public  string Title { get; set; }
/// <summary>
		/// Số điện thoại
		/// </summary>
		public  string Phone { get; set; }
/// <summary>
		/// Địa chỉ Email
		/// </summary>
		[CanBeNull]public  string Email { get; set; }
/// <summary>
		/// Loại
		/// </summary>
		public  int Types { get; set; }
/// <summary>
		/// Loại hình kinh doanh
		/// </summary>
		public  long ?  BusinessTypeId { get; set; }
/// <summary>
		/// Đơn vị tiền tệ
		/// </summary>
		public  long ?  CurrencyUnitId { get; set; }
/// <summary>
		/// Địa chỉ
		/// </summary>
		public  string Address { get; set; }
/// <summary>
		/// Tỉnh/ Thành phố
		/// </summary>
		[CanBeNull]public  string ProvinceCode { get; set; }
/// <summary>
		/// Quận/ Huyện
		/// </summary>
		[CanBeNull]public  string DistrictCode { get; set; }
/// <summary>
		/// Phường/ Xã
		/// </summary>
		[CanBeNull]public  string WardCode { get; set; }
/// <summary>
		/// Tenant
		/// </summary>
		public  int? TenantId { get; set; }
    }
}
