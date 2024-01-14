using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Yootek.EntityDb
{
    /// <summary>
    /// Khởi tạo đối tượng Tài sản phiếu xuất kho
    /// </summary>
    ///<author>ThoNV</author>
    ///<created>01/10/2023</created>
    [Table("PhieuXuatKhoToTaiSan")]
    public class PhieuXuatKhoToTaiSan : FullAuditedEntity<long>, IMayHaveTenant
    {
        /// <summary>
        /// Phiếu xuất kho
        /// </summary>
        //[Required]
        public long PhieuXuatKhoId { get; set; }
        /// <summary>
        /// Tài sản
        /// </summary>
        [Required]
        public long TaiSanId { get; set; }
        /// <summary>
        /// Số lượng
        /// </summary>
        public int SoLuong { get; set; }
        /// <summary>
        /// Đơn giá
        /// </summary>
        public decimal? DonGia { get; set; }
        public long DonViTinhId { get; set; }
        /// <summary>
        /// Thành tiền
        /// </summary>
        public decimal? ThanhTien { get; set; }
        /// <summary>
        /// Ghi chú
        /// </summary>
        [StringLength(1000)]
        public string GhiChu { get; set; }
        /// <summary>
        /// Tenant
        /// </summary>
        public int? TenantId { get; set; }
        public DateTime? BaoHanhTu { get; set; }
        public DateTime? BaoHanhDen { get; set; }
        public int? ThoiGianBaoHanh { get; set; }
        public int? TongSoLuong { get; set; }
        public int? TrangThai { get; set; }
    }
}