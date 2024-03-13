using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Yootek.EntityDb
{
    /// <summary>
    /// Khởi tạo đối tượng Dịch vụ
    /// </summary>
    ///<author>ThoNV</author>
    ///<created>25/10/2023</created>
    [Table("DigitalServices")]
    public class DigitalServices : FullAuditedEntity<long>, IMayHaveTenant
    {
        /// <summary>
        /// Tên dịch vụ
        /// </summary>
        [StringLength(250)]
        [Required]
        public string Title { get; set; }
        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Mã
        /// </summary>
        [StringLength(50)]
        [Required]
        public string Code { get; set; }
        /// <summary>
        /// Khu đô thị
        /// </summary>
        [Required]
        public long UrbanId { get; set; }
        /// <summary>
        /// Danh mục dịch vụ
        /// </summary>
        [Required]
        public long Category { get; set; }
        /// <summary>
        /// Ảnh đại diện
        /// </summary>
        [Required]
        public string Image { get; set; }
        /// <summary>
        /// Hình ảnh mô tả
        /// </summary>
        public List<string> ImageDescription { get; set; }
        /// <summary>
        /// Địa chỉ
        /// </summary>
        [Required]
        public string Address { get; set; }
        /// <summary>
        /// Toạ độ
        /// </summary>
        public string Coordinates { get; set; }
        /// <summary>
        /// Thông tin liên hệ
        /// </summary>
        public string Contacts { get; set; }
        /// <summary>
        /// Tenant
        /// </summary>
        public int? TenantId { get; set; }
        public long? WorkTypeId { get; set; }

    }
}