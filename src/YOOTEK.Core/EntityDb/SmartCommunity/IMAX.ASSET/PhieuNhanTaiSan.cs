using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Yootek.EntityDb
{
    /// <summary>
    /// Khởi tạo đối tượng Phiếu nhận tài sản
    /// </summary>
    ///<author>ThoNV</author>
    ///<created>01/10/2023</created>
    [Table("PhieuNhanTaiSan")]
    public class PhieuNhanTaiSan : FullAuditedEntity<long>, IMayHaveTenant
    {
        /// <summary>
        /// Mã phiếu nhận
        /// </summary>
        [StringLength(100)]
        //[Required]
        public string Code { get; set; }
        /// <summary>
        /// Ngày nhận
        /// </summary>
        [Required]
        public DateTime NgayNhan { get; set; }
        /// <summary>
        /// Kho tài sản
        /// </summary>
        public long? KhoTaiSanId { get; set; }
        /// <summary>
        /// Họ và tên người trả hàng
        /// </summary>
        [StringLength(250)]
        public string NguoiTraHang { get; set; }
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
    }
}