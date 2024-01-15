using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Yootek.EntityDb
{
    /// <summary>
    /// Khởi tạo đối tượng Phiếu giao tài sản
    /// </summary>
    ///<author>ThoNV</author>
    ///<created>01/10/2023</created>
    [Table("PhieuGiaoTaiSan")]
    public class PhieuGiaoTaiSan : FullAuditedEntity<long>, IMayHaveTenant
    {
        /// <summary>
        /// Mã phiếu giao
        /// </summary>
        [StringLength(100)]
        //[Required]
        public string Code { get; set; }
        /// <summary>
        /// Ngày giao
        /// </summary>
        [Required]
        public DateTime NgayGiao { get; set; }
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
        [StringLength(250)]
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