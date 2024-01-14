using Abp.Domain.Entities;
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
    [Table("UserFeedbackLike")]
    public class CitizenReflectLike : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? CommentId { get; set; }
        public int? Type { get; set; }
        public int? TenantId { get; set; }
        public int? StateLike { get; set; }
        public string Name { get; set; }
    }
}
