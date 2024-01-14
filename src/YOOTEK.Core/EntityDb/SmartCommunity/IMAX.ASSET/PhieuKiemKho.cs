using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Yootek.EntityDb
{
    /// <summary>
    /// Khởi tạo đối tượng Phiếu kiểm kho tài sản
    /// </summary>
    ///<author>ThoNV</author>
    ///<created>01/10/2023</created>
    [Table("PhieuKiemKho")]
    public class PhieuKiemKho : FullAuditedEntity<long>, IMayHaveTenant
    {
        /// <summary>
        /// Mã phiếu
        /// </summary>
        [StringLength(100)]
        //[Required]
        public string Code { get; set; }
        /// <summary>
        /// Thời gian kiểm kho
        /// </summary>
        [Required]
        public DateTime NgayKiemKho { get; set; }
        /// <summary>
        /// Kho tài sản
        /// </summary>
        public long? KhoTaiSanId { get; set; }
        /// <summary>
        /// NguoiLapPhieu
        /// </summary>
        [StringLength(250)]
        public long? NguoiLapPhieuId { get; set; }
        /// <summary>
        /// File đính kèm
        /// </summary>        
        public string FileDinhKem { get; set; }
        /// <summary>
        /// Người xác nhận
        /// </summary>
        [StringLength(250)]
        public long? NguoiXacNhanId { get; set; }
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