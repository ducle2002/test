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
    [Table("ComponentPage")]
    public class ComponentPage : FullAuditedEntity<long>
    {
        [StringLength(1000)]
        public string Title1 { get; set; }
        [StringLength(1000)]
        public string Title2 { get; set; }

        public string Content { get; set; }
        [StringLength(2000)]
        public string Image1 { get; set; }
        [StringLength(2000)]
        public string Image2 { get; set; }
        [StringLength(2000)]
        public string ImageBackground { get; set; }
        public int Type { get; set; }
        public int Status { get; set; }
        public int Order { get; set; }
        [StringLength(2000)]
        public string Button { get; set; }
        [StringLength(1000)]
        public string Language { get; set; }
    }
}
