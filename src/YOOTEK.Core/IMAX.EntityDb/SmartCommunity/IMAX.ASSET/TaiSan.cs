using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Yootek.EntityDb
{
    /// <summary>
    /// Khởi tạo đối tượng Danh mục tài sản
    /// </summary>
    ///<author>ThoNV</author>
    ///<created>01/10/2023</created>
    [Table("TaiSan")]
    public class TaiSan : FullAuditedEntity<long>, IMayHaveTenant
    {
        /// <summary>
        /// Mã tài sản
        /// </summary>
        [StringLength(100)]
        [Required]
        public string Code { get; set; }
        /// <summary>
        /// Tên tài sản
        /// </summary>
        [StringLength(250)]
        [Required]
        public string Title { get; set; }
        /// <summary>
        /// Mô tả
        /// </summary>
        [StringLength(1000)]
        public string Description { get; set; }
        /// <summary>
        /// Số seri
        /// </summary>
        [StringLength(100)]
        public string SerialNumber { get; set; }
        /// <summary>
        /// Nhóm tài sản
        /// </summary>
        public long? NhomTaiSanId { get; set; }
        /// <summary>
        /// Loại tài sản
        /// </summary>
        public long? LoaiTaiSanId { get; set; }
        /// <summary>
        /// Nhà sản xuất
        /// </summary>
        public long? NhaSanXuatId { get; set; }
        /// <summary>
        /// Đơn vị tính
        /// </summary>
        public long? DonViTinhId { get; set; }
        /// <summary>
        /// Kho tài sản
        /// </summary>
        public long? KhoTaiSanId { get; set; }
        /// <summary>
        /// Tổng số lượng
        /// </summary>
        public int TongSoLuong { get; set; }
        /// <summary>
        /// Số lượng còn lại
        /// </summary>
        public int SoLuongTrongKho { get; set; }
        /// <summary>
        /// Số lượng hỏng
        /// </summary>
        public int SoLuongHong { get; set; }
        /// <summary>
        /// Số lượng đang sử dụng
        /// </summary>
        public int SoLuongDangSuDung { get; set; }
        /// <summary>
        /// Giá nhập
        /// </summary>
        public decimal? GiaNhap { get; set; }
        /// <summary>
        /// Giá xuất
        /// </summary>
        public decimal? GiaXuat { get; set; }
        /// <summary>
        /// Ảnh đại diện
        /// </summary>
        [StringLength(1000)]
        public string AnhDaiDien { get; set; }
        /// <summary>
        /// Hình ảnh
        /// </summary>
         public List<string>? HinhAnh { get; set; }
        /// <summary>
        /// Số lượng cảnh bảo
        /// </summary>
        public int SoLuongCanhBao { get; set; }
        /// <summary>
        /// Tỷ lệ hao mòn
        /// </summary>
        public decimal? TyLeHaoMon { get; set; }
        /// <summary>
        /// Trạng Thái
        /// </summary>
        public int TrangThai { get; set; }
        /// <summary>
        /// Tenant
        /// </summary>
        public int? TenantId { get; set; }
        public int ThoiGianBaoHanh { get; set; }
        public long? MaHeThongId { get; set; }
    }
}