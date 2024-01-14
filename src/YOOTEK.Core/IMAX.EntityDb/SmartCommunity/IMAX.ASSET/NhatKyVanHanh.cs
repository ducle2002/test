using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Yootek.EntityDb
{
    /// <summary>
    /// Khởi tạo đối tượng Nhật ký vận hành
    /// </summary>
    ///<author>ThoNV</author>
    ///<created>21/10/2023</created>
    [Table("NhatKyVanHanh")]
    public class NhatKyVanHanh : FullAuditedEntity<long>, IMayHaveTenant
    {
        /// <summary>
        /// Tài sản
        /// </summary>
        // [Required]
        public long TaiSanId { get; set; }
        /// <summary>
        /// Bộ phận theo dõi
        /// </summary>
        [StringLength(1000)]
        public string BoPhanTheoDoi { get; set; }
        /// <summary>
        /// Ngày sửa chữa/kiểm tra
        /// </summary>
        public DateTime? NgaySuaChua { get; set; }
        /// <summary>
        /// Ngày checklist
        /// </summary>
        public DateTime? NgayCheckList { get; set; }
        /// <summary>
        /// Tenant
        /// </summary>
        public int? TenantId { get; set; }
        /// <summary>
        /// Người kiểm tra
        /// </summary>
        public long NguoiKiemTraId { get; set; }
        public int TrangThai { get; set; }
        public string NoiDung { get; set; }
    }
}