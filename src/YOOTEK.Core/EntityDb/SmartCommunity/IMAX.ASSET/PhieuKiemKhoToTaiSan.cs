using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Yootek.EntityDb
{
    /// <summary>
    /// Khởi tạo đối tượng Tài sản phiếu kiểm kho
    /// </summary>
    ///<author>ThoNV</author>
    ///<created>01/10/2023</created>
    [Table("PhieuKiemKhoToTaiSan")]
    public class PhieuKiemKhoToTaiSan : FullAuditedEntity<long>, IMayHaveTenant
    {
        /// <summary>
        /// Phiếu kiểm kho
        /// </summary>
        //[Required]
        public long PhieuKiemKhoId { get; set; }
        /// <summary>
        /// Tài sản
        /// </summary>
        [Required]
        public long TaiSanId { get; set; }
        /// <summary>
        /// Tổng số lượng
        /// </summary>
        public int? TongSoLuong { get; set; }
        /// <summary>
        /// Số lượng đạt
        /// </summary>
        public int? SoLuongDat { get; set; }
        /// <summary>
        /// Số lượng hỏng
        /// </summary>
        public int? SoLuongHong { get; set; }
        public long DonViTinhId { get; set; }
        /// <summary>
        /// Ghi chú
        /// </summary>
        [StringLength(1000)]
        public string GhiChu { get; set; }
        /// <summary>
        /// Tenant
        /// </summary>
        public int? TenantId { get; set; }
    }
}