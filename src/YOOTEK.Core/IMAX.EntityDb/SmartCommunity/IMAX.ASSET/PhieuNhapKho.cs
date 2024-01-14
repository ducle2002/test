using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Yootek.EntityDb
{
    /// <summary>
    /// Khởi tạo đối tượng Phiếu nhập kho
    /// </summary>
    ///<author>ThoNV</author>
    ///<created>01/10/2023</created>
    [Table("PhieuNhapKho")]
    public class PhieuNhapKho : FullAuditedEntity<long>, IMayHaveTenant
    {
        /// <summary>
        /// Mã phiếu nhập
        /// </summary>
        [StringLength(100)]        
        public string Code { get; set; }
        /// <summary>
        /// Ngày nhập
        /// </summary>
        [Required]
        public DateTime NgayNhap { get; set; }
        /// <summary>
        /// Kho tài sản
        /// </summary>
        public long? KhoTaiSanId { get; set; }
        /// <summary>
        /// Họ và tên người giao hàng
        /// </summary>
        [StringLength(250)]
        public string NguoiGiaoHang { get; set; }
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
        /// Tenant
        /// </summary>
        public int? TenantId { get; set; }
    }
}