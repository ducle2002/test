using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Yootek.EntityDb.Yootek.DichVu.CheckingObj
{
    [Table("CheckingObject")]
    public class CheckingObject : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public long? ObjectId { get; set; }
        public int? Type { get; set; }
    }
}
