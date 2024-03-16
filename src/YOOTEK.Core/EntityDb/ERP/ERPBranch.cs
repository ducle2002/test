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
    [Table("ERPBranchs")]
    public class ERPBranch : FullAuditedEntity<long>, IMayHaveTenant
    {
        /// <summary>
		/// Chi nhánh mặc định
		/// </summary>
		public  bool ?  IsDefault { get; set; }
/// <summary>
		/// Tên chi nhánh
		/// </summary>
		public  string Title { get; set; }
/// <summary>
		/// Số điện thoại
		/// </summary>
		public  string Phone { get; set; }
/// <summary>
		/// Mã chi nhánh
		/// </summary>
		[CanBeNull]public  string Code { get; set; }
/// <summary>
		/// Cửa hàng ID
		/// </summary>
		public  long SellerId { get; set; }
/// <summary>
		/// Mã bưu điện
		/// </summary>
		[CanBeNull]public  string ZipCode { get; set; }
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
