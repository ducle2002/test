using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace IMAX.EntityDb
{
    /// <summary>
    /// Khởi tạo đối tượng Thông tin tài sản
    /// </summary>
    ///<author>ThoNV</author>
    ///<created>18/10/2023</created>
    [Table("TaiSanChiTiet")]
    public class TaiSanChiTiet : FullAuditedEntity<long>, IMayHaveTenant
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
        /// Hình thức
        /// </summary>
        [Required]
        public int HinhThuc { get; set; }
        /// <summary>
        /// Mã hệ thống
        /// </summary>
        [Required]
        public long MaHeThongId { get; set; }
        /// <summary>
        /// Nhóm tài sản
        /// </summary>
        [Required]
        public long NhomTaiSanId { get; set; }
        /// <summary>
        /// Trạng  thái
        /// </summary>
        [Required]
        public int TrangThai { get; set; }
        /// <summary>
        /// Block
        /// </summary>
        public long? BlockId { get; set; }
        /// <summary>
        /// Căn hộ
        /// </summary>
        public long? ApartmentId { get; set; }
        /// <summary>
        /// Mã số bảo hành
        /// </summary>
        [StringLength(100)]
        public string MaSoBaoHanh { get; set; }
        /// <summary>
        /// Giá trị tài sản
        /// </summary>
        public long? GiaTriTaiSan { get; set; }
        /// <summary>
        /// Ngày bắt đầu
        /// </summary>
        [Required]
        public DateTime? NgayBatDau { get; set; }
        /// <summary>
        /// Ngày kết thúc
        /// </summary>
        public DateTime? NgayKetThuc { get; set; }
        /// <summary>
        /// Ghi chú
        /// </summary>
        [StringLength(1000)]
        public string GhiChu { get; set; }
        /// <summary>
        /// Tòa nhà
        /// </summary>
        public long? BuildingId { get; set; }
        /// <summary>
        /// Tầng
        /// </summary>
        public long? FloorId { get; set; }
        /// <summary>
        /// Số lượng
        /// </summary>
        [StringLength(100)]
        public string SoLuong { get; set; }
        /// <summary>
        /// Xuất xứ
        /// </summary>
        [StringLength(250)]
        public string XuatXu { get; set; }
        /// <summary>
        /// Tenant
        /// </summary>
        public int? TenantId { get; set; }
        public string FieldDynamic { get; set; }
        [StringLength(512)]
        public string QrCode { get; set; }
    }
}