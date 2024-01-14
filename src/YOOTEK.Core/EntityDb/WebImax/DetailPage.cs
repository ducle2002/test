using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.EntityDb
{
    [Table("DetailPage")]
    public class DetailPage : FullAuditedEntity<long>
    {
        [StringLength(1000)]
        public string Title { get; set; }

        public string Description { get; set; }
        [StringLength(2000)]
        public string Keywords { get; set; }
        [StringLength(2000)]
        public string Image { get; set; }
        [StringLength(2000)]
        public int Type { get; set; }
        public int Status { get; set; }
        [StringLength(1000)]
        public string Language { get; set; }
    }
}
