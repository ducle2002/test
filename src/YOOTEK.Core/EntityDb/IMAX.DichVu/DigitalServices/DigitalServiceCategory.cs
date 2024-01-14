using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Yootek.EntityDb
{
    /// <summary>
    /// Khởi tạo đối tượng Danh mục dịch vụ
    /// </summary>
    ///<author>ThoNV</author>
    ///<created>25/10/2023</created>
    [Table("DigitalServiceCategory")]
    public class DigitalServiceCategory : FullAuditedEntity<long>, IMayHaveTenant
    {
        /// <summary>
        /// Danh mục dịch vụ
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
        /// Tenant
        /// </summary>
        public int? TenantId { get; set; }
    }
}