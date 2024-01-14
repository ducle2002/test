using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Yootek.EntityDb
{
    /// <summary>
    /// Khởi tạo đối tượng Nhóm tài sản
    /// </summary>
    ///<author>ThoNV</author>
    ///<created>30/09/2023</created>
    [Table("NhomTaiSan")]
    public class NhomTaiSan : FullAuditedEntity<long>, IMayHaveTenant
    {
        /// <summary>
        /// Mã nhóm
        /// </summary>
        [StringLength(100)]
        [Required]
        public string Code { get; set; }
        /// <summary>
        /// Tên nhóm
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
        /// Tenant
        /// </summary>
        public int? TenantId { get; set; }
        /// <summary>
        /// Nhóm cha
        /// </summary>
        public long? ParentId { get; set; }
        public long MaHeThongId { get; set; }
    }
}