using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Yootek.EntityDb
{
    /// <summary>
    /// Khởi tạo đối tượng Phiếu xuất kho
    /// </summary>
    ///<author>ThoNV</author>
    ///<created>01/10/2023</created>
    [Table("PhieuXuatKho")]
    public class PhieuXuatKho : FullAuditedEntity<long>, IMayHaveTenant
    {
        /// <summary>
        /// Mã phiếu xuất
        /// </summary>
        [StringLength(100)]
        //[Required]
        public string Code { get; set; }
        /// <summary>
        /// Ngày xuất
        /// </summary>
        [Required]
        public DateTime NgayXuat { get; set; }
        /// <summary>
        /// Kho tài sản
        /// </summary>
        public long? KhoTaiSanId { get; set; }
        /// <summary>
        /// Họ và tên người nhận hàng
        /// </summary>
        [StringLength(250)]
        public string NguoiNhan { get; set; }
        /// <summary>
        /// Người lập phiếu
        /// </summary>
        public long? NguoiLapPhieuId { get; set; }
        /// <summary>
        /// TongTien
        /// </summary>
        public decimal? TongTien { get; set; }
        /// <summary>
        /// Tổng tiền bằng chữ
        /// </summary>
        [StringLength(1000)]
        public string TongTienBangChu { get; set; }
        /// <summary>
        /// Kế toán
        /// </summary>
        public long? KeToanId { get; set; }
        /// <summary>
        /// File đính kèm
        /// </summary>
        public string FileDinhKem { get; set; }
        /// <summary>
        /// Thủ kho
        /// </summary>
        public long? ThuKhoId { get; set; }
        /// <summary>
        /// Trạng thái
        /// </summary>
        public int? TrangThai { get; set; }
        /// <summary>
        /// Lý do
        /// </summary>
        [StringLength(1000)]
        public string LyDo { get; set; }
        /// <summary>
        /// Tenant
        /// </summary>
        public int? TenantId { get; set; }
        public int NoiXuat { get; set; }
        public long? ToaNhaId { get; set; }
        public long? CanHoId { get; set; }
        public int? TrongNgoaiToaNha { get; set; }
        public long? KhoNhanId { get; set; }
    }
}