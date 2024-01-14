using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Yootek.EntityDb.Yootek.MobileAppFeedback
{
    [Table("MobileAppFeedback")]
    public class AppFeedback : FullAuditedEntity<long>
    {
        public string Feedback { get; set; }
        public string FileUrl { get; set; }
    }
}
