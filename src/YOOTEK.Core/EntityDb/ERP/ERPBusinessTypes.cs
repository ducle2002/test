using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Yootek.EntityDb
{
    [Table("ERPBusinessTypes")]
    public class ERPBusinessType : FullAuditedEntity<long>
    {
        /// <summary>
		/// Loại
		/// </summary>
		public  int Types { get; set; }
/// <summary>
		/// Tiêu đề
		/// </summary>
		public  string Title { get; set; }
/// <summary>
		/// Mô tả
		/// </summary>
		public  string Description { get; set; }
    }
}
