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
    [Table("QAComments")]
    public class QAComment : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public string Comment { get; set; }
        public string FileUrl { get; set; }
        public long ForumId { get; set; }
        public bool? IsAdmin { get; set; }

    }
}
