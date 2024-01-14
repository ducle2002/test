using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.EntityDb
{
    [Table("News")]
    public class News : Entity<long>
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime DatePost { get; set; }
        public string Poster { get; set; }
        public long? NewsTypeId { get; set; }
        public string UrlImage { get; set; }
    }
}
