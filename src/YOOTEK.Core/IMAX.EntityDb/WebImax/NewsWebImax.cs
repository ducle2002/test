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
    [Table("NewsWebImax")]
    public class NewsWebImax : FullAuditedEntity<long>
    {
        [StringLength(1000)]
        public string Title { get; set; }
        [StringLength(2000)]
        public string Description { get; set; }
        public int State { get; set; }
        [StringLength(2000)]
        public string Avatar { get; set; }
        public string Content { get; set; }
        [StringLength(1000)]
        public string TitleShort { get; set; }
        [StringLength(2000)]
        public string DescriptionShort { get; set; }
        [StringLength(2000)]
        public string Keywords { get; set; }
        public int Category { get; set; }
        public int Top { get; set; }
        [StringLength(1000)]
        public string KeyUrl { get; set; }
    }
}
